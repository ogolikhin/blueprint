import { IDialogSettings, BaseDialogController, IDialogData } from "../../../../shared";
import { Models } from "../../../../main/models";

export interface IConfirmDeleteController {
    errorMessage: string;
    hasError: boolean;
}

export class ConfirmDeleteController extends BaseDialogController implements IConfirmDeleteController {
    private _errorMessage: string;
    private _artifactList: Models.IArtifact[];
    private _projectList: Models.IItem[];
    private _selectedProject: number;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData"
    ];

    constructor(
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        public dialogData: Models.IProject) {
        super($instance, dialogSettings);
        this._projectList = [dialogData];
        this._artifactList = dialogData.children;
    }

    public get hasError(): boolean {
        return Boolean(this._errorMessage);
    }

    public get errorMessage(): string {
        return this._errorMessage;
    }
    public get artifactList(): Models.IArtifact[]{
        return this._artifactList;
    }
    public get projectList(): Models.IItem[]{
        return this._projectList;
    }


}
