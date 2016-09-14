import { ILocalizationService, ISettingsService, IStateManager } from "../../../core";
import { Models} from "../../../main";
import { ISession } from "../../../shell";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import { Helper } from "../../../shared/utils/helper";
import { ArtifactPickerController, IArtifactPickerFilter } from "../../../main/components/dialogs/bp-artifact-picker/bp-artifact-picker";
import { ISelectionManager } from "../../../managers/selection-manager";
import { IStatefulItem } from "../../../managers/models";
import { 
    IArtifactAttachmentsResultSet, 
    IArtifactAttachmentsService, 
    IArtifactDocRef, 
    IStatefulArtifact,
    IStatefulSubArtifact,
    IArtifactAttachment
} from "../../../managers/artifact-manager";

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
        "selectionManager2",
        "stateManager",
        "session",
        "artifactAttachments",
        "settings",
        "dialogService"
    ];

    public attachmentsList: IArtifactAttachment[];
    public docRefList: IArtifactDocRef[];
    public item: IStatefulItem;

    public categoryFilter: number;
    public isLoading: boolean = false;
    public filesToUpload: any;

    private artifactIsDeleted: boolean = false;
    private maxAttachmentFilesizeDefault: number = 10485760; // 10 MB
    private maxNumberAttachmentsDefault: number = 50;
    
    constructor(
        $q: ng.IQService,
        private localization: ILocalizationService,
        protected selectionManager: ISelectionManager,
        protected stateManager: IStateManager,
        private session: ISession,
        private artifactAttachments: IArtifactAttachmentsService,
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
            header: this.localization.get("App_UP_Attachments_Document_Picker_Title")
        };

        const dialogData: IArtifactPickerFilter = {
            ItemTypePredefines: [Models.ItemTypePredefined.Document]
        };

        this.dialogService.open(dialogSettings, dialogData).then((artifact: Models.IArtifact) => {
            if (artifact) {
                // this.artifactAttachmentsList.documentReferences.push(<IArtifactDocRef>{
                //     artifactName: artifact.name,
                //     artifactId: artifact.id,
                //     userId: this.session.currentUser.id,
                //     userName: this.session.currentUser.displayName,
                //     itemTypePrefix: artifact.prefix,
                //     referencedDate: new Date().toISOString()
                // });
            }
        });
    }

    public onFileSelect(files: File[], callback?: Function) {
        const openUploadStatus = () => {
            const dialogSettings = <IDialogSettings>{
                okButton: this.localization.get("App_Button_Ok", "OK"),
                template: require("../../../shared/widgets/bp-file-upload-status/bp-file-upload-status.html"),
                controller: BpFileUploadStatusController,
                css: "nova-file-upload-status",
                header: this.localization.get("App_UP_Attachments_Upload_Dialog_Header", "File Upload")
            };

            const curNumOfAttachments: number = this.attachmentsList && this.attachmentsList.length || 0;
            let maxAttachmentFilesize: number = this.settingsService.getNumber("MaxAttachmentFilesize", this.maxAttachmentFilesizeDefault);
            let maxNumberAttachments: number = this.settingsService.getNumber("MaxNumberAttachments", this.maxNumberAttachmentsDefault);

            if (maxNumberAttachments < 0 || !Helper.isInt(maxNumberAttachments)) {
                maxNumberAttachments = this.maxNumberAttachmentsDefault;
            }
            if (maxAttachmentFilesize < 0 || !Helper.isInt(maxAttachmentFilesize)) {
                maxAttachmentFilesize = this.maxAttachmentFilesizeDefault;
            }

            const dialogData: IUploadStatusDialogData = {
                files: files,
                maxAttachmentFilesize: maxAttachmentFilesize,
                maxNumberAttachments: maxNumberAttachments - curNumOfAttachments
            };

            this.dialogService.open(dialogSettings, dialogData).then((uploadList: any[]) => {
                if (uploadList) {
                    const newAttachments = uploadList.map((uploadedFile: any) => {
                        return <IArtifactAttachment>{
                            userId: this.session.currentUser.id,
                            userName: this.session.currentUser.displayName,
                            fileName: uploadedFile.name,
                            attachmentId: null,
                            guid: uploadedFile.guid,
                            uploadedDate: null
                        };
                    });
                    this.attachmentsList = this.item.attachments.add(newAttachments);
                }
            }).finally(() => {
                if (callback) {
                    callback();
                }
            });
        };

        openUploadStatus();
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        this.attachmentsList = [];
        this.item = subArtifact || artifact;
        
        if (this.item) {
            this.isLoading = true;
            this.item.attachments.get().then((attachments: IArtifactAttachment[]) => {
                this.attachmentsList = attachments;
            }).finally(() => {
                this.isLoading = false;
            });

            // TODO: docRefSubscriber here
            // this.docRefSubscriber = this.item.docRefs...
        }

        // if (Helper.canUtilityPanelUseSelectedArtifact(artifact)) {
        //     return this.getAttachments(artifact.id, subArtifact ? subArtifact.id : null, timeout)
        //         .then((result: IArtifactAttachmentsResultSet) => {
        //             this.artifactIsDeleted = false;
        //             this.artifactAttachmentsList = result;
        //         }, (error) => {
        //             if (error && error.statusCode === 404) {
        //                 this.artifactIsDeleted = true;
        //             }
        //         });
        // } else {
        //     this.artifactAttachmentsList = null;
        // }
        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    public canAddNewFile() {
        return !this.artifactIsDeleted &&
            !(this.itemState && this.itemState.isReadonly);
    }

    /* tslint:disable:no-unused-variable */
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
    /* tslint:enable:no-unused-variable */
}
