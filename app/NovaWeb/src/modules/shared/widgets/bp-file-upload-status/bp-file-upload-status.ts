import {IDialogSettings, BaseDialogController} from "../bp-dialog";
import {FiletypeParser} from "../../utils/filetypeParser";
import {IFileResult} from "../../../commonModule/fileUpload/fileUpload.service";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";

export interface IUploadStatusDialogData {
    files: File[];
    maxNumberAttachments: number;
    maxNumberAttachmentsError?: string;
    maxAttachmentFilesize: number;
    minAttachmentFilesize?: number;
    allowedExtentions?: string[];
    fileUploadAction: (file: File,
                       progressCallback: (event: ProgressEvent) => void,
                       cancelPromise: ng.IPromise<void>) => ng.IPromise<any>;
}

export interface IUploadStatusResult {
    guid: string;
    url: string;
    name: string;
    file: File;
}

interface IFileUploadStatus {
    file: File;
    isComplete: boolean;
    isFailed: boolean;
    isUploading: boolean;
    timeout: ng.IDeferred<any>;
    progress: number;
    guid: string;
    filepath: string;
    errorMessage: string;
}

export class BpFileUploadStatusController extends BaseDialogController {
    public files: IFileUploadStatus[];
    public totalFailedFiles: number = 0;
    public areFilesUploading: boolean = true;

    static $inject = [
        "$q",
        "localization",
        "$filter",
        "$uibModalInstance",
        "dialogSettings",
        "dialogData"
    ];

    constructor(private $q: ng.IQService,
                private localization: ILocalizationService,
                private $filter: ng.IFilterService,
                $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                private dialogData: IUploadStatusDialogData) {

        super($uibModalInstance, dialogSettings);

        this.initFilesList(dialogData.files);
        this.queueFilesToUpload();
        this.updateTotalFailedFiles();
    };

    private initFilesList(files: File[]) {
        if (files) {
            this.files = [];
            for (let i = 0; i < files.length; i++) {
                this.files.push({
                    file: files[i],
                    isComplete: false,
                    isFailed: false,
                    isUploading: false,
                    timeout: this.$q.defer(),
                    progress: 0,
                    guid: null,
                    filepath: null,
                    errorMessage: null
                });
            }
        }
    }

    private queueFilesToUpload() {
        this.files.map((file: IFileUploadStatus, index: number) => {
            if (index > this.dialogData.maxNumberAttachments - 1) {
                file.isFailed = true;

                file.errorMessage = this.dialogData.maxNumberAttachmentsError ||
                    this.localization.get("App_UP_Attachments_Upload_Max_Attachments_Error",
                        "The artifact has the maximum number of attachments.");
            } else if (this.dialogData.allowedExtentions && this.dialogData.allowedExtentions.length > 0 &&
                this.dialogData.allowedExtentions.indexOf(FiletypeParser.getFileExtension(file.file.name)) === -1) {
                file.isFailed = true;
                file.errorMessage
                    = this.localization.get("App_UP_Attachments_Have_Wrong_Type", "The attachment has wrong file type.");
            } else if (this.isFileValid(file)) {
                this.uploadFile(file);
            }

            return file;
        });
    }

    private isFileValid(f: IFileUploadStatus) {
        const filesizeFilter: Function = this.$filter("bpFilesize") as Function;

        if (!_.isNil(this.dialogData.minAttachmentFilesize) &&
            f.file.size < this.dialogData.minAttachmentFilesize) {

            f.isFailed = true;
            f.errorMessage =
                this.localization.get("App_UP_Attachments_Upload_Filesize_Zero_Error", "The file size is zero.");
            return false;
        }

        if (f.file.size > this.dialogData.maxAttachmentFilesize) {
            f.isFailed = true;
            f.errorMessage =
                this.localization.get("App_UP_Attachments_Upload_Max_Filesize_Error", "The file exceeds")
                + ` ${filesizeFilter(this.dialogData.maxAttachmentFilesize)}.`;

            return false;
        }
        return true;
    }

    private uploadFile(f: IFileUploadStatus) {
        f.isUploading = true;

        this.dialogData.fileUploadAction(f.file, (event: ProgressEvent) => {
            f.progress = Math.floor((event.loaded / event.total) * 100);
        }, f.timeout.promise)
            .then((result: IFileResult) => {
                f.progress = 100;
                f.guid = result.guid;
                f.filepath = result.uriToFile;
                f.isComplete = true;
                f.isFailed = false;

                return result;
            })
            .catch((error: any) => {
                if (error.statusCode === HttpStatusCode.BadRequest && error.errorCode === 131) {
                    f.errorMessage = this.localization.get("App_UP_Attachments_Image_Wrong_Type");    
                } else {
                    f.errorMessage = this.localization.get("App_UP_Attachments_Upload_Error", "Upload error.");
                }
                f.progress = 0;
                f.isFailed = true;
                f.isComplete = false;
            })
            .finally(() => {
                f.isUploading = false;
                this.updateTotalFailedFiles();
                if (this.files.filter(a => a.isUploading).length === 0) {
                    if (this.files.filter(a => a.isFailed).length === 0) {
                        super.ok();
                    } else {
                        this.areFilesUploading = false;
                    }
                }
            });
    }

    private updateTotalFailedFiles() {
        this.totalFailedFiles = this.files.filter((file: IFileUploadStatus) => file.isFailed).length;
    }

    public cancelUpload(file: IFileUploadStatus) {
        const index = this.files.indexOf(file);
        if (index > -1) {
            this.files[index].timeout.resolve();
            this.files.splice(index, 1);
        }
    }

    private cancelAllActiveFileUploads() {
        this.files
            .filter((file: IFileUploadStatus) => file.isUploading)
            .map((file: IFileUploadStatus) => {
                file.timeout.resolve();
            });
    }

    // Dialog return value
    public get returnValue(): IUploadStatusResult[] {
        return this.files
            .filter((f: IFileUploadStatus) => f.isComplete)
            .map((f: IFileUploadStatus) => {
                return {
                    guid: f.guid,
                    url: f.filepath,
                    name: f.file.name,
                    file: f.file
                };
            });
    };

    public cancel() {
        this.cancelAllActiveFileUploads();
        super.cancel();
    };

    public ok() {
        this.cancelAllActiveFileUploads();
        super.ok();
    };
}
