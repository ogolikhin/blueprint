import {BPDropdownAction, BPDropdownItemAction, IDialogService, IDialogSettings} from "../../../../../shared";
import {IUserStoryService} from "../../../services/user-story.svc";
import {IApplicationError, IMessageService, ILocalizationService, ErrorCode} from "../../../../../core";
import {ISelectionManager} from "../../../../../managers/selection-manager";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {StatefulProcessSubArtifact} from "../../../process-subartifact";
import {IProcess, IUserStory} from "../../../models/process-models";
import {ProcessShapeType} from "../../../models/enums";
import {IProcessDiagramCommunication, ProcessEvents} from "../../diagram/process-diagram-communication";
import {DialogTypeEnum} from "../../../../../shared/widgets/bp-dialog/bp-dialog";

export class GenerateUserStoriesAction extends BPDropdownAction {
    private userStoryService: IUserStoryService;
    private messageService: IMessageService;
    private localization: ILocalizationService;
    private dialogService: IDialogService;
    private processDiagramManager: IProcessDiagramCommunication;

    constructor(
        process: StatefulProcessArtifact,
        selectionManager: ISelectionManager,
        userStoryService: IUserStoryService,
        messageService: IMessageService,
        localization: ILocalizationService,
        dialogService: IDialogService,
        processDiagramManager: IProcessDiagramCommunication
    ) {
        if (!selectionManager) {
            throw new Error("Selection manager is not provided or is null");
        }

        if (!userStoryService) {
            throw new Error("User Story service is not provided or is null");
        }

        if (!messageService) {
            throw new Error("Message service is not provided or is null");
        }

        if (!localization) {
            throw new Error("Localization service is not provided or is null");
        }

        super(
            () => !process.artifactState.readonly,
            "fonticon fonticon2-news",
            localization.get("ST_Generate_Toolbar_Button"),
            undefined,
            new BPDropdownItemAction(
                localization.get("ST_Generate_Contextual_Toolbar_Button"),
                () => {
                    const subArtifact = selectionManager.getSubArtifact() as StatefulProcessSubArtifact;
                    this.execute(process, subArtifact.id);
                },
                () => {
                    if (process.artifactState.readonly) {
                        return false;
                    }

                    const subArtifact = selectionManager.getSubArtifact() as StatefulProcessSubArtifact;
                    if (!subArtifact) {
                        return false;
                    }

                    const subArtifactType: ProcessShapeType = subArtifact.propertyValues["clientType"].value;
                    if (subArtifactType !== ProcessShapeType.UserTask) {
                        return false;
                    }

                    if (subArtifact.id < 0) {
                        return false;
                    }

                    return true;
                }
            ),
            new BPDropdownItemAction(
                localization.get("ST_Generate_All_Contextual_Toolbar_Button"),
                () => {
                    this.execute(process);
                },
                () => {
                    if (process.artifactState.readonly) {
                        return false;
                    }

                    return true;
                }
            )
        );

        this.userStoryService = userStoryService;
        this.messageService = messageService;
        this.localization = localization;
        this.dialogService = dialogService;
        this.processDiagramManager = processDiagramManager;
    }

    private execute(process: StatefulProcessArtifact, userTaskId?: number) {
        if (!process) {
            return;
        }

        if (process && process.artifactState && process.artifactState.readonly) {
            this.messageService.addError(this.localization.get("ST_View_OpenedInReadonly_Message"));
            return;
        }

        if (!process.artifactState.published) {
            const settings = <IDialogSettings>{
                type: DialogTypeEnum.Confirm,
                header: this.localization.get("App_DialogTitle_Confirmation"),
                message: this.localization.get("ST_Confirm_Publish_Before_Generate_User_Story"),
                okButton: this.localization.get("ST_Confirm_Publish_ConfirmButton_Label")
            };

            this.dialogService.open(settings)
                .then(() => {
                    process.publish()
                        .then(() => {
                            this.generateUserStories(process, userTaskId);
                        });
                });
        } else {
            this.generateUserStories(process, userTaskId);
        }
    }

    private generateUserStories(process: StatefulProcessArtifact, userTaskId?: number): void {
        const projectId = process.projectId;
        const processId = process.id;

        this.userStoryService.generateUserStories(process.projectId, process.id, userTaskId)
            .then((userStories: IUserStory[]) => {
                this.processDiagramManager.action(ProcessEvents.UserStoriesGenerated, userStories);
                this.showSuccessMessage(userTaskId);
            })
            .catch((reason: IApplicationError) => {
                let message: string = reason.message;
                
                if (reason.errorCode === ErrorCode.ArtifactNotPublished) {
                    message = this.localization.get("ST_User_Stories_Generation_Failed_LockedByOtherUser_Message");
                }

                this.showErrorMessage(message);
            });
    }

    private showSuccessMessage(userTaskId?: number): void {
        const userStoriesGeneratedMessage = 
            userTaskId ? 
            this.localization.get("ST_User_Story_Generated_Message") : 
            this.localization.get("ST_User_Stories_Generated_Message");
        this.messageService.addInfo(userStoriesGeneratedMessage);
    }

    private showErrorMessage(message: string): void {
        this.messageService.addError(message);
    }
}
