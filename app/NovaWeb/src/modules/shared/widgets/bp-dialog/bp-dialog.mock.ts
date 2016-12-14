import {IDialogService, IDialogSettings, DialogTypeEnum} from "./bp-dialog";

export class DialogServiceMock implements IDialogService {
    public static $inject = ["$q"];
    public alerts: string[] = [];

    constructor(private $q: ng.IQService) {
    }

    public open(dialogSettings: IDialogSettings): ng.IPromise<any> {
        return this.$q.resolve(true);
    }

    public alert(message: string, header?: string): ng.IPromise<any> {
        this.alerts.push(message);
        return this.$q.resolve(true);
    }

    public confirm(message: string, header?: string): ng.IPromise<any> {
        return this.$q.resolve(true);
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
