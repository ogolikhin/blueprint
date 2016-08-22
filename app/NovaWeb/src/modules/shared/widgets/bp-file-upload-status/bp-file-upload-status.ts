import "angular";
import { ILocalizationService, IFileUploadService, IFileResult } from "../../../core";
import { IDialogSettings, BaseDialogController, IDialogService } from "../../../shared/";

export interface IBpFileUploadStatusController {
    // propertyMap: any;
    // selectedItem?: any;
}

export interface IUploadStatusDialogData {
    files: File[];
    maxNumberAttachments: number;
    maxAttachmentFilesize: number;
}

interface IFileUploadStatus {
    file: File;
    isComplete: boolean;
    isFailed: boolean;
    isUploading: boolean;
    cancelDeferred: ng.IDeferred<any>;
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
        "dialogService",
        "dialogSettings",
        "dialogData"
    ];

    public files: IFileUploadStatus[];
    public totalFailedFiles: number = 0;



    constructor(
        private $q: ng.IQService,
        private localization: ILocalizationService,
        private fileUploadService: IFileUploadService,
        private $filter: ng.IFilterService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private dialogService: IDialogService,
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
                    cancelDeferred: this.$q.defer(),
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
                file.errorMessage = "You have exceeded the maximum allowed number of attachments";
            
            } else if (this.isFileValid(file)) {
                this.uploadFile(file);
            }
            
            return file;
        });
    }

    private isFileValid(f: IFileUploadStatus) {
        const filesizeFilter: Function = this.$filter("BpFilesize") as Function;

        if (f.file.size > this.dialogData.maxAttachmentFilesize) {
            f.isFailed = true;
            f.errorMessage = "File exceeds " + filesizeFilter(this.dialogData.maxAttachmentFilesize);

            return false;
        }
        return true;
    }

    private uploadFile(f: IFileUploadStatus) {
        f.isUploading = true;

        this.fileUploadService.uploadToFileStore(f.file, new Date(), (event: ProgressEvent) => {
            f.progress = Math.floor((event.loaded / event.total) * 100);
        }, f.cancelDeferred.promise).then(
            (result: IFileResult) => {
                f.guid = result.guid;
                f.filepath = result.uriToFile;
                f.isComplete = true;
                f.isFailed = false;

                return result;
            },
            (error: any) => {
                if (error.code === -1) {
                    f.errorMessage = "Cancelled upload";
                }
                console.log("error uploading a file: " + error.message);
                f.errorMessage = error.message;
                f.isFailed = true;
                f.isComplete = false;
            }
        ).finally(() => {
            f.isUploading = false;
            this.updateTotalFailedFiles();
        });
    }

    private updateTotalFailedFiles() {
        this.totalFailedFiles = this.files.filter((file: IFileUploadStatus) => file.isFailed).length;
    }

    public cancelUpload(file: IFileUploadStatus) {
        const index = this.files.indexOf(file);
        if (index > -1) {
            this.files[index].cancelDeferred.resolve();
            this.files.splice(index, 1);
        }
    }

    private cancelAllActiveFileUploads() {
        this.files
            .filter((file: IFileUploadStatus) => file.isUploading)
            .map((file: IFileUploadStatus) => {
                file.cancelDeferred.resolve();
            });
    }

    // Dialog return value
    public get returnValue(): any {
        return this.files
            .filter((f: IFileUploadStatus) => f.isComplete)
            .map((f: IFileUploadStatus) => f.guid);
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
