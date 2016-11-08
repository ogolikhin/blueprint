import { IDialogSettings, BaseDialogController, IDialogData } from "../../../../shared";
import { Models } from "../../../../main/models";

export interface IConfirmPublishController {
    errorMessage: string;
    hasError: boolean;
}

export interface IConfirmPublishDialogData extends IDialogData {
    artifactList: Models.IArtifact[];
    projectList: Models.IItem[];
    selectedProject?: number;
}

export class ConfirmPublishController extends BaseDialogController implements IConfirmPublishController {
    private _errorMessage: string;
    private _artifactList: Models.IArtifact[];
    private _projectList: Models.IItem[];
    private _selectedProject: number;
    private _header: string;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData"
    ];

    constructor(
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        public dialogData: IConfirmPublishDialogData) {
        super($instance, dialogSettings);
        this._artifactList = dialogData.artifactList;
        this._projectList = dialogData.projectList;
        this._selectedProject = dialogData.selectedProject;
        this._header = dialogSettings.header;
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
    public get selectedProject(): number{
        return this._selectedProject;
    }
    public get header(): string{
        return this._header;
    }


}
