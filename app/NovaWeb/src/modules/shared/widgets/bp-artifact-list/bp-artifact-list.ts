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
    public limitTo: number;
    public initialRows: number;

    private _sortedList: Models.IArtifactWithProject[];
    private initialLimit: number;

    public $onInit = () => {
        // limit set on the component
        if (!this.limit) {
            this.limit = 100; // if no limit is set, default to 100
        }
        this.initialLimit = 10; // limit of the initial list
        this.limitTo = this.initialLimit;

        this._sortedList = [];
        this.artifactList.forEach((artifact) => {
            let item = artifact as Models.IArtifactWithProject;
            if (this.projectList) {
                item.projectName = this.projectList.filter((project) => {
                    return project.id === artifact.projectId;
                })[0].name;
            }
            this._sortedList.push(item);
        });
        this._sortedList.sort(this.sortList);

        let projectId: number;
        let numberOfProject = 0;
        for (let index = 0; index < this._sortedList.length && index < this.initialLimit; index++) {
            if (projectId !== this._sortedList[index].projectId) {
                numberOfProject++;
                projectId = this._sortedList[index].projectId;
            }
        }
        this.initialRows = numberOfProject + (this._sortedList.length < this.initialLimit ? this._sortedList.length : this.initialLimit);
    };

    public get sortedList(): Models.IArtifactWithProject[]{
        return this._sortedList;
    }

    public loadMore = () => {
        this.limitTo = this.limit;
    };

    public itemLabel = (artifact: Models.IArtifactWithProject): string => {
        return artifact.prefix + artifact.id + " - " + artifact.name;
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
