import { IDialogSettings, BaseDialogController, IDialogData } from "../../../../shared";
import { Models } from "../../../../main/models";

export interface ICreateNewArtifactController {
    errorMessage: string;
    hasError: boolean;
}

export interface ICreateNewArtifactDialogData extends IDialogData {
    parentId: number;
    parentType: number;
}

export class CreateNewArtifactController extends BaseDialogController implements ICreateNewArtifactController {
    private _errorMessage: string;
    private _parentId: number;
    private _parentType: number;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData"
    ];

    constructor(
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        public dialogData: ICreateNewArtifactDialogData) {
        super($instance, dialogSettings);
        this._parentId = dialogData.parentId;
        this._parentType = dialogData.parentType;
    }

    public get hasError(): boolean {
        return Boolean(this._errorMessage);
    }
    public get errorMessage(): string {
        return this._errorMessage;
    }
}
