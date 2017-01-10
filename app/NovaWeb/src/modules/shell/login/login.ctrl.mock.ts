import {IDialogSettings} from "../../shared/widgets/bp-dialog/bp-dialog";

export class DataMock {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public dialogData = {};
}

export class DialogSettingsMock {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public dialogSettings: IDialogSettings = {};
}
