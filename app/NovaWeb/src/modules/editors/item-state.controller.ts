import {Models} from "../main/models";
import {IArtifactManager, IProjectManager} from "../managers";
import {IStatefulArtifact} from "../managers/artifact-manager";
import {IStatefulArtifactFactory} from "../managers/artifact-manager/artifact/artifact.factory";
import {IItemInfoService, IItemInfoResult} from "../core/navigation/item-info.svc";
import {IApplicationError} from "../core/error/applicationError";
import {HttpStatusCode} from "../core/http/http-status-code";
import {INavigationService} from "../core/navigation/navigation.svc";
import {IMessageService} from "../core/messages/message.svc";
import {MessageType} from "../core/messages/message";
import {ILocalizationService} from "../core/localization/localizationService";
import {ItemTypePredefined} from "../main/models/enums";
import {ILoadingOverlayService} from "../core/loading-overlay/loading-overlay.svc";

export class ItemStateController {

    public static $inject = [
        "$state",
        "artifactManager",
        "projectManager",
        "messageService",
        "localization",
        "navigationService",
        "itemInfoService",
        "loadingOverlayService",
        "statefulArtifactFactory",
        "$rootScope"
    ];

    constructor(private $state: angular.ui.IStateService,
                private artifactManager: IArtifactManager,
                private projectManager: IProjectManager,
                private messageService: IMessageService,
                private localization: ILocalizationService,
                private navigationService: INavigationService,
                private itemInfoService: IItemInfoService,
                private loadingOverlayService: ILoadingOverlayService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private $rootScope: ng.IRootScopeService) {
        const id: number = parseInt($state.params["id"], 10);
        const version = parseInt($state.params["version"], 10);

            if (_.isFinite(id)) {
                this.clearStickyMessages();

                const artifact = artifactManager.get(id);

                this.$rootScope.$applyAsync(() => {
                    if (artifact && !artifact.artifactState.deleted && !version) {
                        artifact.unload();
                        this.navigateToSubRoute(artifact);
                    } else {
                        this.getItemInfo(id, version);
                    }
                });
            }
    }

    private getItemInfo(id: number, version: number) {
        const loadingGetItemInfoId = this.loadingOverlayService.beginLoading();
        this.itemInfoService.get(id).then((result: IItemInfoResult) => {

            if (this.itemInfoService.isSubArtifact(result)) {
                // navigate to subartifact's artifact
                this.navigationService.navigateTo({id: result.id, redirect: true});

            } else if (this.itemInfoService.isProject(result)) {
                this.projectManager.openProject(result).then(() => {
                    this.navigationService.reloadCurrentState();
                });
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
                        this.messageService.addError("Artifact_Version_NotFound", true);
                        this.navigationService.navigateToMain(true);
                        return;
                    }
                    artifact.version = version;
                    statefulArtifact.artifactState.historical = true;
                } else if (result.isDeleted) {
                    statefulArtifact.artifactState.deleted = true;
                    statefulArtifact.artifactState.deletedDateTime = result.deletedDateTime;
                    statefulArtifact.artifactState.deletedById = result.deletedByUser.id;
                    statefulArtifact.artifactState.deletedByDisplayName = result.deletedByUser.displayName;
                    statefulArtifact.artifactState.historical = true;
                }

                this.navigateToSubRoute(statefulArtifact, version);
            } else {
                this.messageService.addError("Artifact_GoTo_NotAvailable", true);
            }
        }).catch(error => {
            this.navigationService.navigateToMain(true);
            // Forbidden and ServerError responces are handled in http-error-interceptor.ts
            if (error.statusCode === HttpStatusCode.NotFound) {
                this.messageService.addError("HttpError_NotFound", true);
            }
        }).finally(() => {
            this.loadingOverlayService.endLoading(loadingGetItemInfoId);
        });
    }

    private isCollection(itemType: Models.ItemTypePredefined): boolean {
        return itemType === ItemTypePredefined.CollectionFolder || itemType === ItemTypePredefined.ArtifactCollection;
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

    private clearStickyMessages() {
        this.messageService.messages.forEach(message => {
            if (!message.canBeClosedManually && message.messageType !== MessageType.Info) {
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
                stateName = "main.item.general";
                break;
            case Models.ItemTypePredefined.CollectionFolder:
                // Temporary decision while collections root description is not editable.
                // if artifact is Collections root node
                if (artifact.itemTypeId === ItemTypePredefined.Collections) {
                    stateName = "main.item.general";
                } else {
                    stateName = "main.item.details";
                }
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
