import { IDialogSettings, IDialogService } from "../../.";
import { DialogTypeEnum } from "../../../shared/widgets/bp-dialog/bp-dialog";
import { Models } from "../../.";
import { IUploadStatusDialogData } from "../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";

// interface IUploadStatusDialogData {
//     files: File[];
//     maxNumberAttachments: number;
//     maxAttachmentFilesize: number;
//     allowedExtentions?: string[];
// }

export class ActorImagePickerDialogServiceMock implements IDialogService {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) { }

    public open(dialogSettings: IDialogSettings, dialogData: IUploadStatusDialogData): ng.IPromise<any> {

        const deferred = this.$q.defer<any>();

        let uploadList = dialogData.files;

        deferred.resolve(uploadList);
        return deferred.promise;
    }
    public alert(message: string, header?: string): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }

    public then(): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }

    public confirm(message: string, header?: string): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }
    public dialogSettings: IDialogSettings = {
        type: DialogTypeEnum.Base,
        header: "test",
        message: "test",
        cancelButton: "test",
        okButton: "test",
        template: "test",
        controller: null,
        css: null,
        backdrop: false
    }
}
