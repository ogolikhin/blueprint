import {Models} from "../../main/models";
import {IStatefulArtifact} from "../../managers/artifact-manager";
import {IStatefulArtifactFactory} from "../../managers/artifact-manager/artifact/artifact.factory";
import {IItemInfoService, IItemInfoResult} from "../../commonModule/itemInfo/itemInfo.service";
import {IApplicationError} from "../../shell/error/applicationError";
import {HttpStatusCode} from "../../commonModule/httpInterceptor/http-status-code";
import {INavigationService} from "../../commonModule/navigation/navigation.service";
import {ILocalizationService} from "../../commonModule/localization/localization.service";
import {ItemTypePredefined} from "../../main/models/enums";
import {IMessageService} from "../../main/components/messages/message.svc";
import {MessageType} from "../../main/components/messages/message";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {IProjectManager} from "../../managers/project-manager/project-manager";

export class ItemStateController {

    public activeEditor: string;

    public static $inject = [
        "$stateParams",
        "selectionManager",
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
                private selectionManager: ISelectionManager,
                private projectManager: IProjectManager,
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

            this.projectManager.openProject(itemInfo).then(() => {
                const projectNode = this.projectManager.getProject(itemInfo.id);
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
        }

        this.selectionManager.setArtifact(artifact);
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
            case Models.ItemTypePredefined.BaselineFolder:
                this.activeEditor = artifact.itemTypeId === Models.ItemTypePredefined.BaselinesAndReviews ? "general" : "details";
                break;
            case Models.ItemTypePredefined.CollectionFolder:
                this.activeEditor = artifact.itemTypeId === Models.ItemTypePredefined.Collections ? "general" : "details";
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
