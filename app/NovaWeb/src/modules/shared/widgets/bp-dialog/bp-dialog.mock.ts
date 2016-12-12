import {IDialogService, IDialogSettings, DialogTypeEnum} from "./bp-dialog";

export class DialogServiceMock implements IDialogService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public open(dialogSettings: IDialogSettings): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }

    public alert(message: string, header?: string): ng.IPromise<any> {
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
    };
}
