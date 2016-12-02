import {BPDropdownAction, BPDropdownItemAction} from "../../../../../shared/widgets/bp-toolbar/actions";
import {IDialogService, IDialogSettings} from "../../../../../shared/widgets/bp-dialog/bp-dialog";
import {IUserStoryService} from "../../../services/user-story.svc";
import {ISelectionManager} from "../../../../../managers/selection-manager";
import {IStatefulSubArtifact} from "../../../../../managers/artifact-manager/sub-artifact";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {StatefulProcessSubArtifact} from "../../../process-subartifact";
import {IUserStory} from "../../../models/process-models";
import {ProcessShapeType} from "../../../models/enums";
import {ItemTypePredefined} from "../../../../../main/models/enums";
import {IProcessDiagramCommunication, ProcessEvents} from "../../diagram/process-diagram-communication";
import {DialogTypeEnum} from "../../../../../shared/widgets/bp-dialog/bp-dialog";
import {IApplicationError} from "../../../../../core/error/applicationError";
import {ErrorCode} from "../../../../../core/error/error-code";
import {ILoadingOverlayService} from "../../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../../core/localization/localizationService";
import {IDiagramNode} from "../../diagram/presentation/graph/models/process-graph-interfaces";
import {NodeType} from "../../diagram/presentation/graph/models/process-graph-constants";

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
        private processDiagramManager: IProcessDiagramCommunication
    ) {
        super();

        if (!userStoryService) {
            throw new Error("User story service is not provided or is null");
        }

        if (!messageService) {
            throw new Error("Message service is not provided or is null");
        }

        if (!localization) {
            throw new Error("Localization service is not provided or is null");
        }

        if (!dialogService) {
            throw new Error("Dialog service is not provided or is null");
        }

        if (!loadingOverlayService) {
            throw new Error("Loading overlay service is not provided or is null");
        }

        if (!processDiagramManager) {
            throw new Error("Process diagram manager is not provided or is null");
        }

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
        return this.canExecute();
    }

    public dispose(): void {
        if (this.processDiagramManager) {
            this.processDiagramManager.unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandle);
        }
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
                okButton: this.localization.get("App_Button_PublishAndContinue")
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

    private generateUserStories(process: StatefulProcessArtifact, userTaskId?: number): ng.IPromise<any> {
        const projectId = process.projectId;
        const processId = process.id;

        return this.userStoryService.generateUserStories(process.projectId, process.id, userTaskId)
            .then((userStories: IUserStory[]) => {
                this.processDiagramManager.action(ProcessEvents.UserStoriesGenerated, userStories);

                const userStoriesGeneratedMessage =
                    userTaskId ?
                        this.localization.get("ST_US_Generate_From_UserTask_Success_Message") :
                        this.localization.get("ST_US_Generate_All_Success_Message");
                this.messageService.addInfo(userStoriesGeneratedMessage);

                return process.refresh(false);
            })
            .catch((reason: IApplicationError) => {
                let message: string = this.localization.get("ST_US_Generate_Generic_Failure_Message");

                if (reason) {
                    switch (reason.errorCode) {
                        case ErrorCode.ArtifactNotPublished:
                            message = this.localization.get("ST_US_Generate_LockedByOtherUser_Failure_Message");
                            break;
                    }
                }

                this.messageService.addError(message);
            });
    }
}
