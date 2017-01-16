import {Models} from "../../main/models";
import {IArtifactManager, IProjectManager} from "../../managers";
import {IStatefulArtifact} from "../../managers/artifact-manager";
import {IStatefulArtifactFactory} from "../../managers/artifact-manager/artifact/artifact.factory";
import {IItemInfoService, IItemInfoResult} from "../../core/navigation/item-info.svc";
import {IApplicationError} from "../../core/error/applicationError";
import {HttpStatusCode} from "../../core/http/http-status-code";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {IMessageService} from "../../core/messages/message.svc";
import {MessageType} from "../../core/messages/message";
import {ILocalizationService} from "../../core/localization/localizationService";
import {ItemTypePredefined} from "../../main/models/enums";

export class ItemStateController {

    public activeEditor: string;

    public static $inject = [
        "$stateParams",
        "artifactManager",
        "projectManager",
        "messageService",
        "localization",
        "navigationService",
        "itemInfoService",
        "statefulArtifactFactory",
        "$timeout",
        "itemInfo"
    ];

    constructor(private $stateParams: ng.ui.IStateParamsService,
                private artifactManager: IArtifactManager,
                private projectManager: IProjectManager,
                private messageService: IMessageService,
                private localization: ILocalizationService,
                private navigationService: INavigationService,
                private itemInfoService: IItemInfoService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private $timeout: ng.ITimeoutService,
                private itemInfo: IItemInfoResult) {

        const version = parseInt($stateParams["version"], 10);

        // TODO: remove ArtifactManager caching in future US
        const artifact = artifactManager.get(itemInfo.id);
        if (artifact && !artifact.artifactState.deleted && !version) {
            artifact.unload();
        }

        this.activeEditor = null;
        this.clearStickyMessages();
        this.initItemInfo(itemInfo, _.isFinite(version) ? version : undefined);
    }

    private initItemInfo(itemInfo: IItemInfoResult, version: number) {
        if (this.itemInfoService.isSubArtifact(itemInfo)) {
            // navigate to subartifact's artifact
            this.navigationService.navigateTo({id: itemInfo.id, redirect: true});

        } else if (this.itemInfoService.isProject(itemInfo)) {
            itemInfo.predefinedType = ItemTypePredefined.Project;

            this.projectManager.openProject(itemInfo.id).then(() => {
                const project = this.createArtifact(itemInfo);
                project.itemTypeId = ItemTypePredefined.Project;
                project.itemTypeName = "Project";

                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(project);

                this.$timeout(() => {
                    this.setSelectedArtifact(statefulArtifact);
                    this.setActiveEditor(statefulArtifact);
                });
            });
        } else if (this.itemInfoService.isArtifact(itemInfo) && !this.isBaselineOrReview(itemInfo.predefinedType)) {
            const artifact = this.createArtifact(itemInfo);
            const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(artifact);

            if (version) {
                if (itemInfo.versionCount < version) {
                    this.messageService.addError("Artifact_Version_NotFound", true);
                    this.navigationService.navigateToMain(true);
                    return;
                }
                artifact.version = version;
                statefulArtifact.artifactState.historical = true;
            } else if (itemInfo.isDeleted) {
                statefulArtifact.artifactState.deleted = true;
                statefulArtifact.artifactState.deletedDateTime = itemInfo.deletedDateTime;
                statefulArtifact.artifactState.deletedById = itemInfo.deletedByUser.id;
                statefulArtifact.artifactState.deletedByDisplayName = itemInfo.deletedByUser.displayName;
                statefulArtifact.artifactState.historical = true;
            }

            this.$timeout(() => {
                this.setSelectedArtifact(statefulArtifact);
                this.setActiveEditor(statefulArtifact);
            });
        } else {
            this.messageService.addError("Artifact_GoTo_NotAvailable", true);
        }
    }

    private createArtifact(itemInfo: IItemInfoResult): Models.IArtifact {
        return {
            id: itemInfo.id,
            projectId: itemInfo.projectId,
            name: itemInfo.name,
            parentId: itemInfo.parentId,
            predefinedType: itemInfo.predefinedType,
            prefix: itemInfo.prefix,
            version: itemInfo.version,
            orderIndex: itemInfo.orderIndex,
            lockedByUser: itemInfo.lockedByUser,
            lockedDateTime: itemInfo.lockedDateTime,
            permissions: itemInfo.permissions
        } as Models.IArtifact;
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
            if (!message.canBeClosedManually && message.messageType !== MessageType.Info
                 && message.messageType !== MessageType.LinkInfo) {
                this.messageService.deleteMessageById(message.id);
            }
        });
    }

    private setSelectedArtifact(artifact: IStatefulArtifact) {
        // do not select artifact in explorer if navigated from another artifact
        if (!this.$stateParams["path"]) {
            this.artifactManager.selection.setExplorerArtifact(artifact);
        }

        this.artifactManager.selection.setArtifact(artifact);
        artifact.errorObservable().subscribeOnNext(this.onArtifactError, this);
    }

    private setActiveEditor(artifact: IStatefulArtifact) {
        switch (artifact.predefinedType) {
            case Models.ItemTypePredefined.GenericDiagram:
            case Models.ItemTypePredefined.BusinessProcess:
            case Models.ItemTypePredefined.DomainDiagram:
            case Models.ItemTypePredefined.Storyboard:
            case Models.ItemTypePredefined.UseCaseDiagram:
            case Models.ItemTypePredefined.UseCase:
            case Models.ItemTypePredefined.UIMockup:
                this.activeEditor = "diagram";
                break;
            case Models.ItemTypePredefined.Glossary:
                this.activeEditor = "glossary";
                break;
            case Models.ItemTypePredefined.Project:
                this.activeEditor = "general";
                break;
            case Models.ItemTypePredefined.CollectionFolder:
                if (artifact.parentId === artifact.projectId) {
                    this.activeEditor = "general";
                } else {
                    this.activeEditor = "details";
                }
                break;
            case Models.ItemTypePredefined.ArtifactCollection:
                this.activeEditor = "collection";
                break;
            case Models.ItemTypePredefined.Process:
                this.activeEditor = "process";
                break;
            default:
                this.activeEditor = "details";
        }
    }

    private onArtifactError(error: IApplicationError) {
        if (error.statusCode === HttpStatusCode.NotFound) {
            this.navigationService.reloadCurrentState();

        } else if (error.statusCode === HttpStatusCode.Forbidden ||
            error.statusCode === HttpStatusCode.ServerError ||
            error.statusCode === HttpStatusCode.Unauthorized
        ) {
            this.navigationService.navigateToMain();
        }
    }
}
