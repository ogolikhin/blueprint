import "angular";
import {ILocalizationService, IFileUploadService, IFileResult} from "../../../core";
import {IDialogSettings, BaseDialogController} from "../bp-dialog";
import {FiletypeParser} from "../../../shared/utils/filetypeParser";

export interface IBpFileUploadStatusController {
    // propertyMap: any;
    // selectedItem?: any;
}

export interface IUploadStatusDialogData {
    files: File[];
    maxNumberAttachments: number;
    maxAttachmentFilesize: number;
    allowedExtentions?: string[];
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

export class BpFileUploadStatusController extends BaseDialogController implements IBpFileUploadStatusController {
    static $inject = [
        "$q",
        "localization",
        "fileUploadService",
        "$filter",
        "$uibModalInstance",
        "dialogSettings",
        "dialogData",
        "$timeout"
    ];

    public files: IFileUploadStatus[];
    public totalFailedFiles: number = 0;

    public isUploading: boolean = true;

    constructor(private $q: ng.IQService,
                private localization: ILocalizationService,
                private fileUploadService: IFileUploadService,
                private $filter: ng.IFilterService,
                $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                private dialogData: IUploadStatusDialogData,
                private $timeout: ng.ITimeoutService
                ) {

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

                file.errorMessage =
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
        let expiryDate = new Date();
        expiryDate.setDate(expiryDate.getDate() + 2);

        this.fileUploadService.uploadToFileStore(f.file, expiryDate, (event: ProgressEvent) => {
            f.progress = Math.floor((event.loaded / event.total) * 100);
        }, f.timeout.promise).then(
            (result: IFileResult) => {
                f.progress = 100;
                f.guid = result.guid;
                f.filepath = result.uriToFile;
                f.isComplete = true;
                f.isFailed = false;

                return result;
            },
            (error: any) => {
                f.errorMessage = error && error.message || this.localization.get("App_UP_Attachments_Upload_Error", "Upload error.");
                f.progress = 0;
                f.isFailed = true;
                f.isComplete = false;
            }
        ).finally(() => {
            f.isUploading = false;
            this.updateTotalFailedFiles();
            if (this.files.filter(a => a.isUploading === true).length === 0) {
                if (this.files.filter(a => a.isFailed === true).length === 0) {
                    this.$timeout(() => {
                        super.ok();
                    }, 500);
                } else {
                    this.isUploading = false;
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
