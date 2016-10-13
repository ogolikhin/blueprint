import {IDialogSettings, IDialogData} from "../../../../shared/widgets/bp-dialog/bp-dialog";

export class DataMock {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }


    public dialogData: IDialogData = {
        manualTraces: [],
        artifactId: 1,
        isItemReadOnly: false
    };
}

export class DialogSettingsMock {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }


    public dialogSettings: IDialogSettings = {};
}
