import { ILocalizationService, ISettingsService, IStateManager, ItemState } from "../../../core";
import { ISelectionManager, Models} from "../../../main";
import { ISession } from "../../../shell";
import { IArtifactAttachmentsResultSet, IArtifactAttachments, IArtifactDocRef } from "./artifact-attachments.svc";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import { IDialogSettings, IDialogService, IDialogData } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import { Helper } from "../../../shared/utils/helper";
import { ArtifactPickerController, IArtifactPickerFilter } from "../../../main/components/dialogs/bp-artifact-picker/bp-artifact-picker";

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
        "session",
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
        private session: ISession,
        private artifactAttachments: IArtifactAttachments,
        private settingsService: ISettingsService,
        private dialogService: IDialogService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, selectionManager, stateManager, bpAccordionPanel);
    }
    
    public addDocRef(): void {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../../../main/components/dialogs/bp-artifact-picker/bp-artifact-picker.html"),
            controller: ArtifactPickerController,
            css: "nova-open-project",
            header: "Add Document Reference"
        };

        const dialogData: IArtifactPickerFilter = {
            ItemTypePredefines: [Models.ItemTypePredefined.Document]
        };

        this.dialogService.open(dialogSettings, dialogData).then((artifact: Models.IArtifact) => {
            if (artifact) {
                this.artifactAttachmentsList.documentReferences.push(<IArtifactDocRef>{
                    artifactName: artifact.name,
                    artifactId: artifact.id,
                    userId: this.session.currentUser.id,
                    userName: this.session.currentUser.displayName,
                    itemTypePrefix: artifact.prefix,
                    referencedDate: new Date().toISOString()
                });
            }
        });
    }

    public onFileSelect(files: File[], callback?: Function) {
        const openUploadStatus = () => {
            const dialogSettings = <IDialogSettings>{
                okButton: "Attach", //this.localization.get("App_Button_Open"),
                template: require("../../../shared/widgets/bp-file-upload-status/bp-file-upload-status.html"),
                controller: BpFileUploadStatusController,
                css: "nova-file-upload-status",
                header: "File Upload"
            };

            const maxAttachmentFilesizeDefault: number = 10 * 1024 * 1024;
            const curNumOfAttachments: number = this.artifactAttachmentsList 
                    && this.artifactAttachmentsList.attachments 
                    && this.artifactAttachmentsList.attachments.length || 0;
            const dialogData: IUploadStatusDialogData = {
                files: files,
                maxAttachmentFilesize: this.settingsService.getNumber("MaxAttachmentFilesize", maxAttachmentFilesizeDefault),
                maxNumberAttachments: this.settingsService.getNumber("MaxNumberAttachments", 5) - curNumOfAttachments
            };

            this.dialogService.open(dialogSettings, dialogData).then((uploadList: any[]) => {
                if (callback) {
                    callback();
                }
                // TODO: add state manager handling

                if (uploadList) {
                    uploadList.map((uploadedFile: any) => {
                        this.artifactAttachmentsList.attachments.push({
                            userId: this.session.currentUser.id,
                            userName: this.session.currentUser.displayName,
                            fileName: uploadedFile.name,
                            attachmentId: null,
                            guid: uploadedFile.guid,
                            uploadedDate: null
                        });
                    });
                }
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
