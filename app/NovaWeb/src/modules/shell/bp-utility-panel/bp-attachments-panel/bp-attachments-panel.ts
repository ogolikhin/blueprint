import { ILocalizationService, ISettingsService } from "../../../core";
import { ISelectionManager, Models} from "../../../main";
import { IArtifactAttachmentsResultSet, IArtifactAttachments } from "./artifact-attachments.svc";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";

export class BPAttachmentsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-attachments-panel.html");
    public controller: Function = BPAttachmentsPanelController;
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export class BPAttachmentsPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "localization",
        "selectionManager",
        "artifactAttachments",
        "settings",
        "dialogService"
    ];

    public artifactAttachmentsList: IArtifactAttachmentsResultSet;
    public categoryFilter: number;
    public isLoading: boolean = false;
    public filesToUpload: any;
    
    constructor(
        private localization: ILocalizationService,
        protected selectionManager: ISelectionManager,
        private artifactAttachments: IArtifactAttachments,
        private settingsService: ISettingsService,
        private dialogService: IDialogService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super(selectionManager, bpAccordionPanel);
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

            const dialogData: IUploadStatusDialogData = {
                files: files,
                maxAttachmentFilesize: this.settingsService.getNumber("MaxAttachmentFilesize", 2 * 1024 * 1024),
                maxNumberAttachments: this.settingsService.getNumber("MaxNumberAttachments", 5) - this.artifactAttachmentsList.attachments.length
            };

            this.dialogService.open(dialogSettings, dialogData).then((artifact: any) => {
                console.log("returned values");
            });
        };

        openUploadStatus();
    }

    protected onSelectionChanged = (artifact: Models.IArtifact, subArtifact: Models.ISubArtifact) => {
        this.artifactAttachmentsList = null;

        if (artifact !== null) {
            this.getAttachments(artifact.id, subArtifact ? subArtifact.id : null)
                .then( (result: IArtifactAttachmentsResultSet) => {
                    this.artifactAttachmentsList = result;
                });
        }
    }

    private getAttachments(artifactId: number, subArtifactId: number = null): ng.IPromise<IArtifactAttachmentsResultSet> {
        this.isLoading = true;
        return this.artifactAttachments.getArtifactAttachments(artifactId, subArtifactId)
            .then( (result: IArtifactAttachmentsResultSet) => {
                return result;
            })
            .finally( () => {
                this.isLoading = false;
            });
    }
}
