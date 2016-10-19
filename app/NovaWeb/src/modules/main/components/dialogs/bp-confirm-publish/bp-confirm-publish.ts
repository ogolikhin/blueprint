import "./bp-confirm-publish.scss";

import { Helper, IBPTreeController, IDialogSettings, BaseDialogController, IDialogService, IDialogData } from "../../../../shared";
import { Models, Enums } from "../../../../main/models";

export interface IConfirmPublishController {
    errorMessage: string;
    hasError: boolean;
}

interface IArtifactWithProject extends Models.IArtifact {
    projectName?: string;
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
    private _sortedList: IArtifactWithProject[];
    private _selectedProject: number;
    private _currentProject: number;

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

        this._sortedList = [];
        dialogData.artifactList.forEach((artifact) => {
            let item = artifact as IArtifactWithProject;
            item.projectName = dialogData.projectList.filter((project) => {
                return project.id === artifact.projectId;
            })[0].name;
            this._sortedList.push(item);
        });
        this._sortedList.sort(this.sortList);
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
    public get sortedList(): IArtifactWithProject[]{
        return this._sortedList;
    }

    public itemLabel = (artifact: IArtifactWithProject): string => {
        return artifact.prefix + artifact.id + " - " + artifact.name;
    };

    public mustShowProject = (artifact: Models.IArtifact): boolean => {
        if (this._currentProject !== artifact.projectId) {
            this._currentProject = artifact.projectId;
            return true;
        }
        return false;
    };

    private sortList = (a, b) => {
        // put selected project first
        if (a.projectId === this._selectedProject && b.projectId !== this._selectedProject) {
            return -1;
        } else if (b.projectId === this._selectedProject && a.projectId !== this._selectedProject) {
            return 1;
        }

        // otherwise sort by project name
        if (a.projectName < b.projectName) {
            return -1;
        } else if (a.projectName > b.projectName) {
            return 1;
        } else {
            // then by artifact name
            if (a.name < b.name) {
                return -1;
            } else if (a.name > b.name) {
                return 1;
            } else {
                // and finally by artifact ID
                return a.id > b.id ? 1 : -1;
            }
        }
    };


}
