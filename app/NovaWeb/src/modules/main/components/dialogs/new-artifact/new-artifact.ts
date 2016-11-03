import { IDialogSettings, BaseDialogController, IDialogData } from "../../../../shared";
import { IProjectService } from "../../../../managers/project-manager";
import { Models, Enums } from "../../../../main/models";

export interface ICreateNewArtifactController {
    errorMessage: string;
    hasError: boolean;
}

export interface ICreateNewArtifactDialogData extends IDialogData {
    projectId: number;
    parentId: number;
    parentPredefinedType: number;
}

export class CreateNewArtifactController extends BaseDialogController implements ICreateNewArtifactController {
    private _errorMessage: string;

    private _projectId: number;
    private _parentId: number;
    private _parentType: number;

    private _projectMeta;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "projectService",
        "dialogData"
    ];

    constructor (
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        projectService: IProjectService,
        public dialogData: ICreateNewArtifactDialogData
    ) {
        super($instance, dialogSettings);

        if (!dialogData) {
            throw new Error("Parent data not provided");
        }

        if (!_.isNumber(dialogData.projectId) || dialogData.projectId <= 0) {
            throw new Error("Project not provided");
        }
        this._projectId = dialogData.projectId;

        if (!_.isNumber(dialogData.parentId) || dialogData.parentId <= 0 || !Enums.ItemTypePredefined[dialogData.parentPredefinedType]) {
            // something is wrong with the parent data, we will create the artifact as child of the project
            this._parentId = dialogData.projectId;
            this._parentType = Enums.ItemTypePredefined.Project;
        } else {
            this._parentId = dialogData.parentId;
            this._parentType = dialogData.parentPredefinedType;
        }

        projectService.getProjectMeta(this._projectId).then(
            (projectMeta) => {
                this._projectMeta = projectMeta;
            },
            (error) => {
                throw new Error("project meta erro");
            }
        );
    }

    public get hasError(): boolean {
        return Boolean(this._errorMessage);
    }
    public get errorMessage(): string {
        return this._errorMessage;
    }
}
