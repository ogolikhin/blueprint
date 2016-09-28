import { ILocalizationService, ISettingsService, IMessageService } from "../../../core";
import { Models} from "../../../main";
import { ISession } from "../../../shell";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import { IDialogSettings, IDialogService } from "../../../shared";
import { IUploadStatusDialogData } from "../../../shared/widgets";
import { BpFileUploadStatusController } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import { Helper } from "../../../shared/utils/helper";
import { ArtifactPickerDialogController, IArtifactPickerOptions } from "../../../main/components/bp-artifact-picker";
import { IArtifactManager } from "../../../managers";
import { IStatefulItem } from "../../../managers/models";
import { 
    // IArtifactAttachmentsResultSet, 
    IArtifactAttachmentsService, 
    IArtifactDocRef, 
    IStatefulArtifact,
    IStatefulSubArtifact,
    IArtifactAttachment
} from "../../../managers/artifact-manager";

export class BPAttachmentsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-attachments-panel.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPAttachmentsPanelController;
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export class BPAttachmentsPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "$q",
        "localization",
        "artifactManager",
        "session",
        "artifactAttachments",
        "settings",
        "dialogService",
        "messageService"
    ];

    public attachmentsList: IArtifactAttachment[];
    public docRefList: IArtifactDocRef[];
    public item: IStatefulItem;
    public isItemReadOnly: boolean;

    public categoryFilter: number;
    public isLoadingAttachments: boolean = false;
    public isLoadingDocRefs: boolean = false;
    public filesToUpload: any;

    private maxAttachmentFilesizeDefault: number = 10485760; // 10 MB
    private maxNumberAttachmentsDefault: number = 50;
    
    constructor(
        $q: ng.IQService,
        private localization: ILocalizationService,
        protected artifactManager: IArtifactManager,
        private session: ISession,
        private artifactAttachments: IArtifactAttachmentsService,
        private settingsService: ISettingsService,
        private dialogService: IDialogService,
        private messageService: IMessageService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, artifactManager.selection, bpAccordionPanel);
    }
    
    public addDocRef(): void {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../../../main/components/dialogs/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get("App_UP_Attachments_Document_Picker_Title")
        };

        const dialogData: IArtifactPickerOptions = {
            selectableItemTypes: [Models.ItemTypePredefined.Document],
            showSubArtifacts: false
        };

        this.dialogService.open(dialogSettings, dialogData).then((items: Models.IItem[]) => {
            if (items.length === 1) {
                const artifact = items[0];
                const newDoc = <IArtifactDocRef>{
                    artifactName: artifact.name,
                    artifactId: artifact.id,
                    userId: this.session.currentUser.id,
                    userName: this.session.currentUser.displayName,
                    itemTypePrefix: artifact.prefix,
                    referencedDate: new Date().toISOString()
                };
                var isContainingDocRef = this.docRefList.filter((docref) => { return docref.artifactId === newDoc.artifactId; }).length > 0;
                if (isContainingDocRef) {
                    this.messageService.addError(this.localization.get("App_UP_Attachments_Add_Same_DocRef_Error"));
                } else {
                    this.docRefList = this.item.docRefs.add([newDoc]);
                }
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
                        let fileExt: RegExpMatchArray = uploadedFile.name.match(/([^.]*)$/);
                        return <IArtifactAttachment>{
                            userId: this.session.currentUser.id,
                            userName: this.session.currentUser.displayName,
                            fileName: uploadedFile.name,
                            fileType: fileExt[0] ? fileExt[0] : "",
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

    public deleteAttachment(attachment: IArtifactAttachment) {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Ok", "OK"),
            header: this.localization.get("App_UP_Attachments_Delete_Header", "Delete Attachment"),
            message: this.localization.get("App_UP_Attachments_Delete_Confirm", "Attachment will be deleted. Continue?"),
        };
        this.dialogService.open(dialogSettings).then(() => {
            this.item.attachments.remove([attachment]);
        });
    }

    public deleteDocRef(docRef: IArtifactDocRef) {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Ok", "OK"),
            header: this.localization.get("App_UP_Attachments_Delete_Header", "Delete Document Reference"),
            message: this.localization.get("App_UP_Attachments_Delete_Confirm", "Document Reference will be deleted. Continue?"),
        };
        this.dialogService.open(dialogSettings).then(() => {
            this.item.docRefs.remove([docRef]);
        });
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        this.item = subArtifact || artifact;
        this.getAttachments();

        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    private getAttachments() {
        this.attachmentsList = [];

        if (this.item) {
            this.isLoadingAttachments = true;
            this.item.attachments.get().then((attachments: IArtifactAttachment[]) => {
                this.attachmentsList = attachments;
                
                // get doc refs here because they're included in attachments payload
                this.getDocRefs();
            }).finally(() => {
                this.isItemReadOnly = this.item.artifactState.readonly || this.item.deleted;
                this.isLoadingAttachments = false;
            });
        }
    }

    private getDocRefs() {
        this.docRefList = [];

        if (this.item) {
            this.isLoadingDocRefs = true;
            // don't refresh because they were already retrieved with attachments
            this.item.docRefs.get(false).then((docrefs: IArtifactDocRef[]) => {
                this.docRefList = docrefs;
            }).finally(() => {
                this.isLoadingDocRefs = false;
            });
        }
    }
}
