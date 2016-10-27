import * as angular from "angular";
import { Models } from "../main/models";
import { IArtifactManager } from "../managers";
import { IStatefulArtifact } from "../managers/artifact-manager";
import { IStatefulArtifactFactory } from "../managers/artifact-manager/artifact";
import { IMessageService, Message, MessageType } from "../shell";
import { IApplicationError, HttpStatusCode } from "../core";
import { INavigationService } from "../core/navigation";
import { ILocalizationService } from "../core/localization";
import { IItemInfoService, IItemInfoResult } from "../core/navigation/item-info.svc";

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

        const id = parseInt($state.params["id"], 10);

        if (_.isFinite(id)) {
            this.clearLockedMessages();

            const artifact = artifactManager.get(id);
            if (artifact) {
                artifact.unload();
                this.navigateToSubRoute(artifact);
            } else {
                this.getItemInfo(id);
            }
        }
    }

    public getItemInfo(id: number) {
        this.itemInfoService.get(id).then((result: IItemInfoResult) => {

            if (this.itemInfoService.isSubArtifact(result)) {
                // navigate to subartifact's artifact
                this.navigationService.navigateTo(result.id, true);

            } else if (this.itemInfoService.isProject(result)) {
                // TODO: implement project navigation in the future US
                this.messageService.addError("This artifact type cannot be opened directly using the Go To feature.");
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
                if (result.isDeleted) {
                    statefulArtifact.deleted = true;
                    statefulArtifact.historical = true;
                    const localizedDate = this.localization.current.formatShortDateTime(result.deletedDateTime);
                    const deletedMessage = `Read Only: Deleted by user '${result.deletedByUser.displayName}' on '${localizedDate}'`;
                    this.messageService.addMessage(new Message(MessageType.Lock, deletedMessage));
                }
                this.navigateToSubRoute(statefulArtifact);

            } else {
                this.messageService.addError("This artifact type cannot be opened directly using the Go To feature.");
            }
        }).catch(error => {
            this.navigationService.navigateToMain();
            // Forbidden and ServerError responces are handled in http-error-interceptor.ts
            if (error.statusCode === HttpStatusCode.NotFound) {
                this.messageService.addError("HttpError_NotFound");
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
            if (message.messageType === MessageType.Lock) {
                this.messageService.deleteMessageById(message.id);
            }
        });
    }

    private setSelectedArtifact(artifact: IStatefulArtifact) {
        
        this.artifactManager.selection.setExplorerArtifact(artifact);
        this.artifactManager.selection.setArtifact(artifact);
        this.artifactManager.selection.getArtifact().errorObservable().subscribeOnNext(this.onArtifactError);
        
    }

    public navigateToSubRoute(artifact: IStatefulArtifact) {
        this.setSelectedArtifact(artifact);
        const params =  {id: artifact.id};

        switch (artifact.predefinedType) {
            case Models.ItemTypePredefined.GenericDiagram:
            case Models.ItemTypePredefined.BusinessProcess:
            case Models.ItemTypePredefined.DomainDiagram:
            case Models.ItemTypePredefined.Storyboard:
            case Models.ItemTypePredefined.UseCaseDiagram:
            case Models.ItemTypePredefined.UseCase:
            case Models.ItemTypePredefined.UIMockup:
                this.$state.go("main.item.diagram", params);
                break;
            case Models.ItemTypePredefined.Glossary:
                this.$state.go("main.item.glossary", params);
                break;
            case Models.ItemTypePredefined.Project:
            case Models.ItemTypePredefined.CollectionFolder:
                this.$state.go("main.item.general", params);
                break;
            case Models.ItemTypePredefined.ArtifactCollection:
                this.$state.go("main.item.collection", params);
                break;
            case Models.ItemTypePredefined.Process:
                this.$state.go("main.item.process", params);
                break;
            default:
                this.$state.go("main.item.details", params);
        }
    }

    protected onArtifactError = (error: IApplicationError) => {
        if (error.statusCode === HttpStatusCode.NotFound) {
            const artifact = this.artifactManager.selection.getArtifact();
            this.navigationService.navigateToMain().finally(() => {
                if (artifact) {
                    this.artifactManager.remove(artifact.id);
                    this.navigationService.navigateTo(artifact.id);
                }
            });
            return;
        }
        if (error.statusCode === HttpStatusCode.Forbidden || 
            error.statusCode === HttpStatusCode.ServerError ||
            error.statusCode === HttpStatusCode.Unauthorized
            ) {
            this.navigationService.navigateToMain();
        }
    }
    
}
