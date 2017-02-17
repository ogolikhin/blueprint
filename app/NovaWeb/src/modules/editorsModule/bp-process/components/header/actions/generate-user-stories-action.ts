import {BPDropdownAction, BPDropdownItemAction} from "../../../../../shared/widgets/bp-toolbar/actions";
import {IDialogService, IDialogSettings} from "../../../../../shared/widgets/bp-dialog/bp-dialog";
import {IUserStoryService} from "../../../services/user-story.svc";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {IUserStory} from "../../../models/process-models";
import {ReuseSettings} from "../../../../../main/models/enums";
import {IProcessDiagramCommunication, ProcessEvents} from "../../diagram/process-diagram-communication";
import {DialogTypeEnum} from "../../../../../shared/widgets/bp-dialog/bp-dialog";
import {IApplicationError} from "../../../../../shell/error/applicationError";
import {ErrorCode} from "../../../../../shell/error/error-code";
import {ILoadingOverlayService} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";
import {IDiagramNode} from "../../diagram/presentation/graph/models/process-graph-interfaces";
import {NodeType} from "../../diagram/presentation/graph/models/process-graph-constants";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {IProjectExplorerService} from "../../../../../main/components/bp-explorer/project-explorer.service";
import {AnalyticsCategories, AnalyticsActions} from "../../../../../main/components/analytics";
import {IExtendedAnalyticsService} from "../../../../../main/components/analytics/analytics";

export class GenerateUserStoriesAction extends BPDropdownAction {
    private selectionChangedHandle: string;
    private selection: IDiagramNode[];

    constructor(
        private process: StatefulProcessArtifact,
        private userStoryService: IUserStoryService,
        private messageService: IMessageService,
        private localization: ILocalizationService,
        private dialogService: IDialogService,
        private loadingOverlayService: ILoadingOverlayService,
        private processDiagramManager: IProcessDiagramCommunication,
        private projectExplorerService: IProjectExplorerService,
        private analytics: IExtendedAnalyticsService
    ) {
        super();
        this.actions.push(
            new BPDropdownItemAction(
                this.localization.get("ST_US_Generate_From_UserTask_Label"),
                () => this.executeGenerateFromTask(),
                () => this.canExecuteGenerateFromTask(),
                "fonticon fonticon2-news"
            ),
            new BPDropdownItemAction(
                this.localization.get("ST_US_Generate_All_Label"),
                () => this.executeGenerateAll(),
                () => this.canExecuteGenerateAll(),
                "fonticon fonticon2-news"
            )
        );

        this.selectionChangedHandle = this.processDiagramManager.register(ProcessEvents.SelectionChanged, this.onSelectionChanged);
    }

    public get icon(): string {
        return "fonticon fonticon2-news";
    }

    public get tooltip(): string {
        return this.localization.get("ST_US_Generate_Dropdown_Tooltip");
    }

    public get disabled(): boolean {
        return !this.canExecute();
    }

    public dispose(): void {
        this.processDiagramManager.unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandle);
    }

    private onSelectionChanged = (elements: IDiagramNode[]) => {
        this.selection = elements;
    };

    private canExecuteGenerateFromTask(): boolean {
        if (!this.process || !this.process.artifactState) {
            return false;
        }

        if (this.process.artifactState.readonly) {
            return false;
        }

        if (!this.selection || this.selection.length !== 1) {
            return false;
        }

        const subArtifact: IDiagramNode = this.selection[0];

        //Subartifact is selected and selective readonly is set
        if (this.process.isReuseSettingSRO && this.process.isReuseSettingSRO(ReuseSettings.Subartifacts)) {
            return false;
        }

        if (!subArtifact.model || subArtifact.model.id < 0) {
            return false;
        }

        if (subArtifact.getNodeType() !== NodeType.UserTask) {
            return false;
        }

        return true;
    }

    private executeGenerateFromTask(): void {
        if (!this.canExecuteGenerateFromTask()) {
            return;
        }

        const subArtifact: IDiagramNode = this.selection[0];
        this.execute(this.process, subArtifact.model.id);
    }

    private canExecuteGenerateAll(): boolean {
        if (!this.process || !this.process.artifactState) {
            return false;
        }

        if (this.process.artifactState.readonly) {
            return false;
        }

        if (this.selection && this.selection.length > 1) {
            return false;
        }

        //artifact is selected and selective readonly is set
        if (this.process.isReuseSettingSRO(ReuseSettings.Subartifacts)) {
            return false;
        }

        return true;
    }

    private executeGenerateAll(): void {
        if (!this.canExecuteGenerateAll()) {
            return;
        }

        this.execute(this.process);
    }

    private canExecute(): boolean {
        return this.canExecuteGenerateFromTask() || this.canExecuteGenerateAll();
    }

    private execute(process: StatefulProcessArtifact, userTaskId?: number) {
        if (!process.artifactState.published) {
            const settings = <IDialogSettings>{
                type: DialogTypeEnum.Confirm,
                header: this.localization.get("App_DialogTitle_Confirmation"),
                message: this.localization.get("ST_US_Generate_Confirm_Publish"),
                okButton: this.localization.get("App_Button_PublishAndContinue"),
                css: "nova-messaging"
            };

            this.dialogService.open(settings)
                .then(() => {
                    const publishGenerateBusy: number = this.loadingOverlayService.beginLoading();
                    process.publish()
                        .then(() => {
                            return this.generateUserStories(process, userTaskId);
                        })
                        .catch((reason: IApplicationError) => {
                            let message: string = this.localization.get("Publish_Failure_Message");

                            if (reason) {
                                switch (reason.errorCode) {
                                    case ErrorCode.LockedByOtherUser:
                                        message = this.localization.get("Publish_Failure_LockedByOtherUser_Message");
                                        break;
                                }
                            }

                            this.messageService.addError(message);
                        })
                        .finally(() => {
                            this.loadingOverlayService.endLoading(publishGenerateBusy);
                        });
                });
        } else {
            const generateBusy: number = this.loadingOverlayService.beginLoading();
            this.generateUserStories(process, userTaskId)
                .finally(() => {
                    this.loadingOverlayService.endLoading(generateBusy);
                });
        }
    }

    private trackGenerateUserStoriesEvent(startTime: number, userStoriesCount: number, userTaskId?: number) {
        const category = AnalyticsCategories.userStory;
        let action = AnalyticsActions.generateAll;
        if (userTaskId && userTaskId > 0) {
            action = AnalyticsActions.generateSelected;
        }
        this.analytics.trackAnalyticsTemporalEvent(startTime, category, action, undefined, {metric1: userStoriesCount});
    }

    private generateUserStories(process: StatefulProcessArtifact, userTaskId?: number): ng.IPromise<any> {
        const projectId = process.projectId;
        const processId = process.id;

        const startTime = new Date().getTime();
        return this.userStoryService.generateUserStories(process.projectId, process.id, userTaskId)
            .then((userStories: IUserStory[]) => {
                this.processDiagramManager.action(ProcessEvents.UserStoriesGenerated, userStories);

                const userStoriesGeneratedMessage =
                    userTaskId ?
                        this.localization.get("ST_US_Generate_From_UserTask_Success_Message") :
                        this.localization.get("ST_US_Generate_All_Success_Message");
                this.messageService.addInfo(userStoriesGeneratedMessage);
                this.trackGenerateUserStoriesEvent(startTime, userStories.length, userTaskId);
                return process.refresh();
            })
            .then(() => {
                this.projectExplorerService.refresh(process.projectId);
            })
            .catch((reason: IApplicationError) => {
                let message: string = this.localization.get("ST_US_Generate_Generic_Failure_Message");

                if (reason) {
                    switch (reason.errorCode) {
                        case ErrorCode.ArtifactNotPublished:
                            message = this.localization.get("ST_US_Generate_LockedByOtherUser_Failure_Message");
                            break;
                        case ErrorCode.UserStoryArtifactTypeNotFound:
                            message = reason.message;
                            break;

                    }
                }

                this.messageService.addError(message);
            });
    }
}
