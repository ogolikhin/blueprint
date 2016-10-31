import {BPDropdownAction, BPDropdownItemAction, IDialogService, IDialogSettings} from "../../../../../shared";
import {IUserStoryService} from "../../../services/user-story.svc";
import {IApplicationError, IMessageService, ILocalizationService, ErrorCode} from "../../../../../core";
import {ISelectionManager} from "../../../../../managers/selection-manager";
import {IStatefulSubArtifact} from "../../../../../managers/artifact-manager/sub-artifact";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {StatefulProcessSubArtifact} from "../../../process-subartifact";
import {IProcess, IUserStory} from "../../../models/process-models";
import {ProcessShapeType} from "../../../models/enums";
import {ItemTypePredefined} from "../../../../../main/models/enums";
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
        userStoryService: IUserStoryService,
        selectionManager: ISelectionManager,
        messageService: IMessageService,
        localization: ILocalizationService,
        dialogService: IDialogService,
        processDiagramManager: IProcessDiagramCommunication
    ) {
        if (!userStoryService) {
            throw new Error("User story service is not provided or is null");
        }

        if (!selectionManager) {
            throw new Error("Selection manager is not provided or is null");
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

        if (!processDiagramManager) {
            throw new Error("Process diagram manager is not provided or is null");
        }

        super(
            () => this.canExecute(process),
            "fonticon fonticon2-news",
            localization.get("ST_US_Generate_Dropdown_Tooltip"),
            undefined,
            new BPDropdownItemAction(
                localization.get("ST_US_Generate_From_UserTask_Label"),
                () => this.executeGenerateFromTask(process, selectionManager.getSubArtifact()), 
                () => this.canExecuteGenerateFromTask(process, selectionManager.getSubArtifact()),
            ),
            new BPDropdownItemAction(
                localization.get("ST_US_Generate_All_Label"),
                () => this.executeGenerateAll(process),
                () => this.canExecuteGenerateAll(process)
            )
        );

        this.userStoryService = userStoryService;
        this.messageService = messageService;
        this.localization = localization;
        this.dialogService = dialogService;
        this.processDiagramManager = processDiagramManager;
    }

    private canExecuteGenerateFromTask(
        process: StatefulProcessArtifact,
        subArtifact: IStatefulSubArtifact
    ): boolean {
        if (!process || !process.artifactState) {
            return false;
        }
        
        if (process.artifactState.readonly) {
            return false;
        }

        if (!subArtifact || subArtifact.predefinedType !== ItemTypePredefined.PROShape) {
            return false;
        }

        if (subArtifact.id < 0) {
            return false;
        }

        const processShape = <StatefulProcessSubArtifact>subArtifact;
        const processShapeType: ProcessShapeType = processShape.propertyValues["clientType"].value;

        if (processShapeType !== ProcessShapeType.UserTask) {
            return false;
        }

        return true;
    }

    private executeGenerateFromTask(
        process: StatefulProcessArtifact,
        subArtifact: IStatefulSubArtifact
    ): void {
        if (!this.canExecuteGenerateFromTask(process, subArtifact)) {
            return;
        }
        
        this.execute(process, subArtifact.id);
    }

    private canExecuteGenerateAll(process: StatefulProcessArtifact): boolean {
        if (!process || !process.artifactState) {
            return false;
        }

        return !process.artifactState.readonly;
    }

    private executeGenerateAll(process: StatefulProcessArtifact): void {
        if (!this.canExecuteGenerateAll(process)) {
            return;
        }

        this.execute(process);
    }

    private canExecute(process: StatefulProcessArtifact): boolean {
        return process && process.artifactState && !process.artifactState.readonly;
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
                    process.publish()
                        .then(() => {
                            this.generateUserStories(process, userTaskId);
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
                const userStoriesGeneratedMessage = 
                    userTaskId ? 
                    this.localization.get("ST_US_Generate_From_UserTask_Success_Message") : 
                    this.localization.get("ST_US_Generate_All_Success_Message");
                this.messageService.addInfo(userStoriesGeneratedMessage);
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
