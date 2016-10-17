import * as angular from "angular";
import { ILocalizationService } from "../../../../core";
import { Helper, IBPTreeController, IDialogSettings, BaseDialogController, IDialogService, IDialogData } from "../../../../shared";
import { Models, Enums } from "../../../../main/models";

export interface IConfirmPublishController {
    errorMessage: string;
    hasError: boolean;
}

export interface IConfirmPublishDialogData extends IDialogData {
    artifactList: Models.IArtifact[];
    projectList: Models.IItem[];
}

export class ConfirmPublishController extends BaseDialogController implements IConfirmPublishController {
    private _errorMessage: string;
    private _artifactList: Models.IArtifact[];
    private _projectList: Models.IItem[];

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
        
        //console.log("constructing ConfirmPublishController");
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
    public get projectList(): Models.IArtifact[]{
        return this._projectList;
    }

  


}