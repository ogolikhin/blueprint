import {IDialogSettings} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ICreateNewArtifactDialogData} from "./new-artifact";

export class DataMock implements ICreateNewArtifactDialogData {
    public projectId: number = 1;
    public parentId: number = 1;
    public parentPredefinedType: number = -1;
}

export class DialogSettingsMock {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public dialogSettings: IDialogSettings = {};
}
