import "angular";
import { ILocalizationService, IFileUploadService, IFileResult } from "../../../core";
import { IDialogSettings, BaseDialogController, IDialogService } from "../../../shared/";

export interface IBpFileUploadStatusController {
    // propertyMap: any;
    // selectedItem?: any;
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
        "$scope", 
        "localization",
        "fileUploadService",
        "$uibModalInstance", 
        "dialogService", 
        "dialogSettings",
        "dialogData"
    ];

    public files: IFileUploadStatus[];
    public totalFailedFiles: number = 0;

    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private fileUploadService: IFileUploadService,
        $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private dialogService: IDialogService,
        dialogSettings: IDialogSettings,
        dialogData: File[]) {

        super($uibModalInstance, dialogSettings);
        
        this.initFilesList(dialogData);
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

            const maxNumAttachments = 3;
            if (index > maxNumAttachments - 1) {
                file.isFailed = true;
                file.errorMessage = "You cannot attach more than " + maxNumAttachments + " files";
            
            } else if (this.isFileValid(file)) {
                this.uploadFile(file);
            }
            
            return file;
        });
    }

    private isFileValid(f: IFileUploadStatus) {
        const maxFilesize = 0.5 * 1024 * 1024;
        if (f.file.size > maxFilesize) {
            f.isFailed = true;
            f.errorMessage = "File exceeds 10 MB";

            return false;
        }
        return true;
    }

    private uploadFile(f: IFileUploadStatus) {
        f.isUploading = true;

        this.fileUploadService.uploadToFileStore(f.file, new Date(), (event: ProgressEvent) => {
            f.progress = Math.floor((event.loaded / event.total) * 100);
            console.log("loaded so far: loaded " + event.loaded + ", total: " + event.total + ", progress: " + f.progress);
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
