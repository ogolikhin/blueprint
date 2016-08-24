import { ILocalizationService, ISettingsService, IStateManager, ItemState } from "../../../core";
import { ISelectionManager, Models} from "../../../main";
import { IArtifactAttachmentsResultSet, IArtifactAttachments } from "./artifact-attachments.svc";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import { Helper } from "../../../shared/utils/helper";

export class BPAttachmentsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-attachments-panel.html");
    public controller: Function = BPAttachmentsPanelController;
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export class BPAttachmentsPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "$q",
        "localization",
        "selectionManager",
        "stateManager",
        "artifactAttachments",
        "settings",
        "dialogService"
    ];

    public artifactAttachmentsList: IArtifactAttachmentsResultSet;
    public categoryFilter: number;
    public isLoading: boolean = false;
    public filesToUpload: any;
    
    constructor(
        $q: ng.IQService,
        private localization: ILocalizationService,
        protected selectionManager: ISelectionManager,
        protected stateManager: IStateManager,
        private artifactAttachments: IArtifactAttachments,
        private settingsService: ISettingsService,
        private dialogService: IDialogService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, selectionManager, stateManager, bpAccordionPanel);
    }
    
    public addDocRef(): void {
        alert("Add Doc Ref: US781");
    }

    public onFileSelect(files: File[]) {
        const openUploadStatus = () => {
            const dialogSettings = <IDialogSettings>{
                okButton: "Attach", //this.localization.get("App_Button_Open"),
                template: require("../../../shared/widgets/bp-file-upload-status/bp-file-upload-status.html"),
                controller: BpFileUploadStatusController,
                css: "nova-file-upload-status",
                header: "File Upload"
            };

            const maxAttachmentFilesizeDefault: number = 2 * 1024 * 1024; 
            const dialogData: IUploadStatusDialogData = {
                files: files,
                maxAttachmentFilesize: this.settingsService.getNumber("MaxAttachmentFilesize", maxAttachmentFilesizeDefault),
                maxNumberAttachments: this.settingsService.getNumber("MaxNumberAttachments", 5) - this.artifactAttachmentsList.attachments.length
            };

            this.dialogService.open(dialogSettings, dialogData).then((artifact: any) => {
                console.log("returned values");

                // 1. this.artifactService.lock
                // 2. mark as dirty
                // 3. add change sets to the state manager (should handle 1 & 2)
                // 4. add new attachments to the list
            });
        };

        openUploadStatus();
    }

    protected onSelectionChanged(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        this.artifactAttachmentsList = null;

        if (Helper.canUtilityPanelUseSelectedArtifact(artifact)) {
            return this.getAttachments(artifact.id, subArtifact ? subArtifact.id : null, timeout)
                .then( (result: IArtifactAttachmentsResultSet) => {
                    this.artifactAttachmentsList = result;
                });
        } else {
            this.artifactAttachmentsList = null;
        }
        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    private getAttachments(artifactId: number, subArtifactId: number = null, timeout: ng.IPromise<void>): ng.IPromise<IArtifactAttachmentsResultSet> {
        this.isLoading = true;
        return this.artifactAttachments.getArtifactAttachments(artifactId, subArtifactId, true, timeout)
            .then( (result: IArtifactAttachmentsResultSet) => {
                return result;
            })
            .finally( () => {
                this.isLoading = false;
            });
    }
}
