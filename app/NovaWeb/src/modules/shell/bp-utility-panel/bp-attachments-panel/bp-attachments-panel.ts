import {ISettingsService} from "../../../commonModule/configuration/settings.service";
import {IFileResult, IFileUploadService} from "../../../commonModule/fileUpload/fileUpload.service";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {Enums, Models} from "../../../main";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../main/components/bp-artifact-picker";
import {IMessageService} from "../../../main/components/messages/message.svc";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {IArtifactAttachment, IArtifactDocRef, IStatefulArtifact, IStatefulItem, IStatefulSubArtifact} from "../../../managers/artifact-manager";
import {IDialogService, IDialogSettings} from "../../../shared";
import {Helper} from "../../../shared/utils/helper";
import {IUploadStatusDialogData} from "../../../shared/widgets";
import {BpFileUploadStatusController} from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import {ISession} from "../../../shell";
import {BPBaseUtilityPanelController} from "../bp-base-utility-panel";

export class BPAttachmentsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-attachments-panel.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPAttachmentsPanelController;
    public bindings = {
        context: "<"
    };
}

export class BPAttachmentsPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "$q",
        "localization",
        "session",
        "settings",
        "dialogService",
        "messageService",
        "fileUploadService"
    ];

    public attachmentsList: IArtifactAttachment[];
    public docRefList: IArtifactDocRef[];
    public item: IStatefulItem;
    public categoryFilter: number;
    public filesToUpload: any;

    private maxNumberAttachmentsDefault: number = 50;
    private subscribers: Rx.IDisposable[];

    constructor($q: ng.IQService,
                private localization: ILocalizationService,
                private session: ISession,
                private settingsService: ISettingsService,
                private dialogService: IDialogService,
                private messageService: IMessageService,
                private fileUploadService: IFileUploadService) {
        super($q);

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
            selectableItemTypes: [ItemTypePredefined.Document],
            isItemSelectable: (item: Models.IArtifact) => {
                // only select document reference that has been published at least once
                return item.version > 0;
            }
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
                    referencedDate: null
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

    private uploadFile = (file: File,
                       progressCallback: (event: ProgressEvent) => void,
                       cancelPromise: ng.IPromise<void>): ng.IPromise<IFileResult> => {
        const expiryDate = new Date();
        expiryDate.setDate(expiryDate.getDate() + 2);
        return this.fileUploadService.uploadToFileStore(file, expiryDate, progressCallback, cancelPromise);
    };

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
            let maxAttachmentFilesize: number = this.settingsService.getNumber("MaxAttachmentFilesize", Helper.maxAttachmentFilesizeDefault);
            let maxNumberAttachments: number = this.settingsService.getNumber("MaxNumberAttachments", this.maxNumberAttachmentsDefault);

            if (maxNumberAttachments < 0 || !Helper.isInt(maxNumberAttachments)) {
                maxNumberAttachments = this.maxNumberAttachmentsDefault;
            }
            if (maxAttachmentFilesize < 0 || !Helper.isInt(maxAttachmentFilesize)) {
                maxAttachmentFilesize = Helper.maxAttachmentFilesizeDefault;
            }

            const dialogData: IUploadStatusDialogData = {
                files: files,
                maxAttachmentFilesize: maxAttachmentFilesize,
                maxNumberAttachments: maxNumberAttachments - curNumOfAttachments,
                fileUploadAction: this.uploadFile
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
            okButton: this.localization.get("App_Button_Delete", "Delete"),
            header: this.localization.get("App_DialogTitle_Alert", "Warning"),
            message: this.localization.get("App_UP_Attachments_Delete_Confirm", "Please confirm the deletion of this attachment."),
            css: "modal-alert nova-messaging"
        };
        this.dialogService.open(dialogSettings).then(() => {
            this.item.attachments.remove([attachment]);
        });
    }

    public deleteDocRef(docRef: IArtifactDocRef) {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Delete", "Delete"),
            header: this.localization.get("App_DialogTitle_Alert", "Warning"),
            message: this.localization.get("App_UP_Attachments_Delete_Confirm", "Document Reference will be deleted. Continue?"),
            css: "modal-alert nova-messaging"
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

            const refresh = !this.item.attachments.changes() && !this.item.docRefs.changes();
            if (refresh) {
                this.item.attachments.refresh();
                this.item.docRefs.refresh();
            }

            const attachmentsSubscriber = this.item.attachments.getObservable().subscribe(this.attachmentsUpdated);
            const attachmentErrorSubscriber = this.item.attachments.errorObservable().subscribe(this.clearAttachmentList);
            const docRefsSubscriber = this.item.docRefs.getObservable().subscribe(this.docRefsUpdated);
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

    public canUpdateAttachments = (): boolean => {
        return !this.item.artifactState.readonly && !this.item.isReuseSettingSRO(Enums.ReuseSettings.Attachments);
    }

    public canUpdateDocRefs = (): boolean => {
        return !this.item.artifactState.readonly && !this.item.isReuseSettingSRO(Enums.ReuseSettings.DocumentReferences);
    }
}
