import {Models} from "../main/models";
import {IArtifactManager} from "../managers";
import {IStatefulArtifact} from "../managers/artifact-manager";
import {IStatefulArtifactFactory} from "../managers/artifact-manager/artifact";
import {IItemInfoService, IItemInfoResult} from "../core/navigation/item-info.svc";
import {IApplicationError} from "../core/error/applicationError";
import {HttpStatusCode} from "../core/http/http-status-code";
import {INavigationService} from "../core/navigation/navigation.svc";
import {IMessageService} from "../core/messages/message.svc";
import {MessageType, Message} from "../core/messages/message";
import {ILocalizationService} from "../core/localization/localizationService";

export class ItemStateController {

    public static $inject = [
        "$state",
        "artifactManager",
        "messageService",
        "localization",
        "navigationService",
        "itemInfoService",
        "statefulArtifactFactory"
    ];

    constructor(private $state: angular.ui.IStateService,
                private artifactManager: IArtifactManager,
                private messageService: IMessageService,
                private localization: ILocalizationService,
                private navigationService: INavigationService,
                private itemInfoService: IItemInfoService,
                private statefulArtifactFactory: IStatefulArtifactFactory) {
        const id: number = parseInt($state.params["id"], 10);
        const version = parseInt($state.params["version"], 10);

        if (_.isFinite(id)) {
            this.clearLockedMessages();

            const artifact = artifactManager.get(id);

            if (artifact && !artifact.artifactState.deleted && !version) {
                artifact.unload();
                this.navigateToSubRoute(artifact);
            } else {
                this.getItemInfo(id, version);
            }
        }
    }

    private getItemInfo(id: number, version: number) {
        this.itemInfoService.get(id).then((result: IItemInfoResult) => {

            if (this.itemInfoService.isSubArtifact(result)) {
                // navigate to subartifact's artifact
                this.navigationService.navigateTo({id: result.id, redirect: true});

            } else if (this.itemInfoService.isProject(result)) {
                // TODO: implement project navigation in the future US
                this.messageService.addError("This artifact type cannot be opened directly using the Go To feature.", true);
                this.navigationService.navigateToMain();

            } else if (this.itemInfoService.isArtifact(result) && !this.isBaselineOrReview(result.predefinedType)) {
                const artifact: Models.IArtifact = {
                    id: result.id,
                    projectId: result.projectId,
                    name: result.name,
                    parentId: result.parentId,
                    predefinedType: result.predefinedType,
                    prefix: result.prefix,
                    version: result.version,
                    orderIndex: result.orderIndex,
                    lockedByUser: result.lockedByUser,
                    lockedDateTime: result.lockedDateTime,
                    permissions: result.permissions
                };

                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(artifact);
                if (_.isFinite(version)) {
                    if (result.versionCount < version) {
                        this.messageService.addError("The specified artifact version does not exist", true);
                        this.navigationService.navigateToMain(true);
                        return;
                    }
                    artifact.version = version;
                    statefulArtifact.artifactState.historical = true;
                    
                } else if (result.isDeleted) {
                    statefulArtifact.artifactState.deleted = true;
                    statefulArtifact.artifactState.historical = true;
                    
                    const localizedDate = this.localization.current.formatShortDateTime(result.deletedDateTime);
                    const deletedMessage = `Deleted by user '${result.deletedByUser.displayName}' on '${localizedDate}'`;
                    this.messageService.addMessage(new Message(MessageType.Deleted, deletedMessage, true));
                }

                this.navigateToSubRoute(statefulArtifact, version);
            } else {
                this.messageService.addError("This artifact type cannot be opened directly using the Go To feature.", true);
            }
        }).catch(error => {
            this.navigationService.navigateToMain(true);
            // Forbidden and ServerError responces are handled in http-error-interceptor.ts
            if (error.statusCode === HttpStatusCode.NotFound) {
                this.messageService.addError("HttpError_NotFound", true);
            }
        });
    }

    private isBaselineOrReview(itemType: Models.ItemTypePredefined) {
        const invalidTypes = [
            Models.ItemTypePredefined.ArtifactBaseline,
            Models.ItemTypePredefined.BaselineFolder,
            Models.ItemTypePredefined.Baseline,
            Models.ItemTypePredefined.ArtifactReviewPackage
        ];

        return invalidTypes.indexOf(itemType) >= 0;
    }

    private clearLockedMessages() {
        this.messageService.messages.forEach(message => {
            if (message.messageType === MessageType.Deleted) {
                this.messageService.deleteMessageById(message.id);
            }
        });
    }

    private setSelectedArtifact(artifact: IStatefulArtifact) {
        // do not select artifact in explorer if navigated from another artifact
        if (!this.$state.params["path"]) {
            this.artifactManager.selection.setExplorerArtifact(artifact);
        }
        
        this.artifactManager.selection.setArtifact(artifact);
        artifact.errorObservable().subscribeOnNext(this.onArtifactError);
    }

    private navigateToSubRoute(artifact: IStatefulArtifact, version?: number) {
        this.setSelectedArtifact(artifact);

        let stateName: string;
        switch (artifact.predefinedType) {
            case Models.ItemTypePredefined.GenericDiagram:
            case Models.ItemTypePredefined.BusinessProcess:
            case Models.ItemTypePredefined.DomainDiagram:
            case Models.ItemTypePredefined.Storyboard:
            case Models.ItemTypePredefined.UseCaseDiagram:
            case Models.ItemTypePredefined.UseCase:
            case Models.ItemTypePredefined.UIMockup:
                stateName = "main.item.diagram";
                break;
            case Models.ItemTypePredefined.Glossary:
                stateName = "main.item.glossary";
                break;
            case Models.ItemTypePredefined.Project:
            case Models.ItemTypePredefined.CollectionFolder:
                stateName = "main.item.general";
                break;
            case Models.ItemTypePredefined.ArtifactCollection:
                stateName = "main.item.collection";
                break;
            case Models.ItemTypePredefined.Process:
                stateName = "main.item.process";
                break;
            default:
                stateName = "main.item.details";
        }
        // since URL doesn't change between "main.item" and "main.item.*",
        // must force reload on that exact state name
        const params = {
            id: artifact.id,
            version: _.isFinite(version) ? version : undefined
        };
        this.$state.go(stateName, params, {reload: stateName});
    }

    private onArtifactError = (error: IApplicationError) => {
        if (error.statusCode === HttpStatusCode.NotFound) {
            this.navigationService.reloadParentState();

        } else if (error.statusCode === HttpStatusCode.Forbidden ||
            error.statusCode === HttpStatusCode.ServerError ||
            error.statusCode === HttpStatusCode.Unauthorized
        ) {
            this.navigationService.navigateToMain();
        }
    }
}
