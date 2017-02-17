import {HttpStatusCode} from "../../commonModule/httpInterceptor/http-status-code";
import {IItemInfoResult, IItemInfoService} from "../../commonModule/itemInfo/itemInfo.service";
import {ILocalizationService} from "../../commonModule/localization/localization.service";
import {INavigationService} from "../../commonModule/navigation/navigation.service";
import {MessageType} from "../../main/components/messages/message";
import {IMessageService} from "../../main/components/messages/message.svc";
import {Models} from "../../main/models";
import {ItemTypePredefined} from "../../main/models/itemTypePredefined.enum";
import {IStatefulArtifact} from "../../managers/artifact-manager";
import {IStatefulArtifactFactory} from "../../managers/artifact-manager/artifact/artifact.factory";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {IApplicationError} from "../../shell/error/applicationError";
import {IProjectExplorerService} from "../../main/components/bp-explorer/project-explorer.service";

export class ItemStateController {

    public activeEditor: string;

    public static $inject = [
        "$stateParams",
        "selectionManager",
        "projectExplorerService",
        "messageService",
        "localization",
        "navigationService",
        "itemInfoService",
        "statefulArtifactFactory",
        "$timeout",
        "itemInfo"
    ];

    constructor(private $stateParams: ng.ui.IStateParamsService,
                private selectionManager: ISelectionManager,
                private projectExplorerService: IProjectExplorerService,
                private messageService: IMessageService,
                private localization: ILocalizationService,
                private navigationService: INavigationService,
                private itemInfoService: IItemInfoService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private $timeout: ng.ITimeoutService,
                private itemInfo: IItemInfoResult) {

        const version = parseInt($stateParams["version"], 10);

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

            this.projectExplorerService.openProject(itemInfo).then(() => {
                const projectNode = this.projectExplorerService.getProject(itemInfo.id);
                const project = this.createArtifact(itemInfo);
                project.itemTypeId = ItemTypePredefined.Project;
                project.itemTypeName = "Project";
                project.description = projectNode ? projectNode.model.description : "";

                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(project);

                this.$timeout(() => {
                    this.setSelectedArtifact(statefulArtifact);
                    this.setActiveEditor(statefulArtifact);
                });
            });
        } else if (this.itemInfoService.isArtifact(itemInfo)) {
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
            this.selectionManager.setExplorerArtifact(artifact);
            this.projectExplorerService.setSelectionId(artifact.id);
        }

        this.selectionManager.setArtifact(artifact);
        artifact.errorObservable().subscribeOnNext(this.onArtifactError, this);
    }

    private setActiveEditor(artifact: IStatefulArtifact) {
        switch (artifact.predefinedType) {
            case ItemTypePredefined.GenericDiagram:
            case ItemTypePredefined.BusinessProcess:
            case ItemTypePredefined.DomainDiagram:
            case ItemTypePredefined.Storyboard:
            case ItemTypePredefined.UseCaseDiagram:
            case ItemTypePredefined.UseCase:
            case ItemTypePredefined.UIMockup:
                this.activeEditor = "diagram";
                break;
            case ItemTypePredefined.Glossary:
                this.activeEditor = "glossary";
                break;
            case ItemTypePredefined.Project:
                this.activeEditor = "general";
                break;
            case ItemTypePredefined.BaselineFolder:
                // Cannot use artifact.itemTypeId === ItemTypePredefined.BaselinesAndReviews here
                this.activeEditor = artifact.parentId === artifact.projectId ? "general" : "details";
                break;
            case ItemTypePredefined.CollectionFolder:
                // Cannot use artifact.itemTypeId === ItemTypePredefined.Collections here
                this.activeEditor = artifact.parentId === artifact.projectId ? "general" : "details";
                break;
            case ItemTypePredefined.ArtifactCollection:
                this.activeEditor = "collection";
                break;
            case ItemTypePredefined.Process:
                this.activeEditor = "process";
                break;
            case ItemTypePredefined.ArtifactBaseline:
                this.activeEditor = "baseline";
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
