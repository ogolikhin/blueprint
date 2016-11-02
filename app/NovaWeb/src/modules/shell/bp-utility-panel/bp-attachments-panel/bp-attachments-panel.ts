import {ILocalizationService, ISettingsService, IMessageService} from "../../../core";
import {Models, Enums} from "../../../main";
import {ISession} from "../../../shell";
import {IBpAccordionPanelController} from "../../../main/components/bp-accordion/bp-accordion";
import {BPBaseUtilityPanelController} from "../bp-base-utility-panel";
import {IDialogSettings, IDialogService} from "../../../shared";
import {IUploadStatusDialogData} from "../../../shared/widgets";
import {BpFileUploadStatusController} from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import {Helper} from "../../../shared/utils/helper";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";
import {IArtifactManager} from "../../../managers";
import {IStatefulItem} from "../../../managers/artifact-manager";
import {
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
    public categoryFilter: number;
    public filesToUpload: any;

    private maxAttachmentFilesizeDefault: number = 10485760; // 10 MB
    private maxNumberAttachmentsDefault: number = 50;
    private subscribers: Rx.IDisposable[];

    constructor($q: ng.IQService,
                private localization: ILocalizationService,
                protected artifactManager: IArtifactManager,
                private session: ISession,
                private artifactAttachments: IArtifactAttachmentsService,
                private settingsService: ISettingsService,
                private dialogService: IDialogService,
                private messageService: IMessageService,
                public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, artifactManager.selection, bpAccordionPanel);

        this.subscribers = [];
    }

    public addDocRef(): void {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
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
                const isContainingDocRef = this.docRefList.filter((docref) => {
                        return docref.artifactId === newDoc.artifactId;
                    }).length > 0;
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
            message: this.localization.get("App_UP_Attachments_Delete_Confirm", "Attachment will be deleted. Continue?")
        };
        this.dialogService.open(dialogSettings).then(() => {
            this.item.attachments.remove([attachment]);
        });
    }

    public deleteDocRef(docRef: IArtifactDocRef) {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Ok", "OK"),
            header: this.localization.get("App_UP_Attachments_Delete_Header", "Delete Document Reference"),
            message: this.localization.get("App_UP_Attachments_Delete_Confirm", "Document Reference will be deleted. Continue?")
        };
        this.dialogService.open(dialogSettings).then(() => {
            this.item.docRefs.remove([docRef]);
        });
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        this.item = subArtifact || artifact;

        this.clearAttachmentList();

        this.subscribers = this.subscribers.filter(sub => {
            sub.dispose();
            return false;
        });

        if (this.item) {
            // If artifact does not exist of the server, just initialize with empty lists
            if (!Helper.hasArtifactEverBeenSavedOrPublished(this.item)) {
                if (this.item.attachments.isLoading || this.item.docRefs.isLoading) {

                    this.item.attachments.initialize(this.attachmentsList);
                    this.item.docRefs.initialize(this.docRefList);
                }
            }
            const attachmentsSubscriber = this.item.attachments.getObservable().subscribe(this.attachmentsUpdated);
            const attachmentErrorSubscriber = this.item.attachments.errorObservable().subscribe(this.clearAttachmentList);
            const docRefsSubscriber = this.item.docRefs.getObservable().subscribe(this.docRefsUpdated, this.docRefsUpdated);
            const docRefErrorSubscriber = this.item.docRefs.errorObservable().subscribe(this.clearAttachmentList);

            this.subscribers = [attachmentsSubscriber, docRefsSubscriber, attachmentErrorSubscriber, docRefErrorSubscriber];
        }

        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    private clearAttachmentList = () => {
        this.attachmentsList = [];
        this.docRefList = [];
    }

    private attachmentsUpdated = (attachments: IArtifactAttachment[]) => {
        this.attachmentsList = attachments;
    }

    private docRefsUpdated = (docRefs: IArtifactDocRef[]) => {
        this.docRefList = docRefs;
    }

}
