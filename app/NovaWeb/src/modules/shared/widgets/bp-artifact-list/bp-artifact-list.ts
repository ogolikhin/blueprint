import { Models } from "../../../main/models";

export class BPArtifactListComponent implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-list.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPArtifactListController;
    public bindings: any = {
        artifactList: "<",
        projectList: "<?",
        selectedProject: "<?",
        limit: "<?"
    };
}

interface IArtifactWithProject extends Models.IArtifact {
    projectName?: string;
}

export interface IBPArtifactListController {
    artifactList: Models.IArtifact[];
    projectList?: Models.IItem[];
    selectedProject?: number;
    limit?: number;
}

export class BPArtifactListController implements IBPArtifactListController {
    public artifactList: Models.IArtifact[];
    public projectList: Models.IItem[];
    public selectedProject: number;
    public limit: number;

    private _sortedList: IArtifactWithProject[];
    private _currentProject: number;

    public $onInit = () => {
        if (!this.limit) {
            this.limit = 100;
        }

        this._sortedList = [];
        this.artifactList.forEach((artifact) => {
            let item = artifact as IArtifactWithProject;
            if (this.projectList) {
                item.projectName = this.projectList.filter((project) => {
                    return project.id === artifact.projectId;
                })[0].name;
            }
            this._sortedList.push(item);
        });
        this._sortedList.sort(this.sortList);
    };

    public get sortedList(): IArtifactWithProject[]{
        return this._sortedList;
    }

    public itemLabel = (artifact: IArtifactWithProject): string => {
        return artifact.prefix + artifact.id + " - " + artifact.name;
    };

    public mustShowProject = (artifact: IArtifactWithProject): boolean => {
        if (artifact.projectName && this._currentProject !== artifact.projectId) {
            this._currentProject = artifact.projectId;
            return true;
        }
        return false;
    };

    private sortList = (a, b) => {
        // put selected project first
        if (a.projectId === this.selectedProject && b.projectId !== this.selectedProject) {
            return -1;
        } else if (b.projectId === this.selectedProject && a.projectId !== this.selectedProject) {
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
