import { IDialogSettings, BaseDialogController, IDialogData } from "../../../../shared";
import { Models } from "../../../../main/models";

export interface IConfirmDeleteController {
    errorMessage: string;
    hasError: boolean;
}

export class ConfirmDeleteController extends BaseDialogController implements IConfirmDeleteController {
    private _errorMessage: string;
    private _artifactList: Models.IArtifactWithProject[];
    private _selectedProject: number;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData"
    ];

    constructor(
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        public dialogData: Models.IArtifactWithProject[]) {
        super($instance, dialogSettings);
        this._artifactList = dialogData;
    }

    public get hasError(): boolean {
        return Boolean(this._errorMessage);
    }

    public get errorMessage(): string {
        return this._errorMessage;
    }

    public get artifactList(): Models.IArtifactWithProject[]{
        return this._artifactList;
    }
}
