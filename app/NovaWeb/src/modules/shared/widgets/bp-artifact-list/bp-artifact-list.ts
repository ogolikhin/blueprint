import {Models} from "../../../main/models";

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

    public itemHeight: number;
    public listHeight: string;

    private _sortedList: Models.IArtifactWithProject[];
    private initialLimit: number;

    public $onInit = () => {
        this.itemHeight = 21;

        // limit set on the component
        if (!this.limit) {
            this.limit = 100; // if no limit is set, default to 100
        }
        this.initialLimit = 10; // limit of the initial list
        this.limitTo = this.initialLimit;

        this._sortedList = this.artifactList.map((artifact) => {
            const item = artifact as Models.IArtifactWithProject;
            if (this.projectList) {
                const project = _.find(this.projectList, (project) => {
                    return item.projectId === project.id;
                });
                item.projectName = project ? project.name : undefined;
            }
            return item;
        });
        this._sortedList.sort(this.sortList);

        const initialArtifacts = _.slice(this._sortedList, 0, this.initialLimit);
        const initialProjects = initialArtifacts.map(artifact => artifact.projectName);
        const numberOfDistinctProjects = Object.keys(_.countBy(initialProjects)).length;
        this.initialRows = numberOfDistinctProjects + (this._sortedList.length < this.initialLimit ? this._sortedList.length : this.initialLimit);

        this.listHeight = _.toString(this.initialRows * this.itemHeight) + "px";
    };

    public get sortedList(): Models.IArtifactWithProject[] {
        return this._sortedList;
    }

    public showOverflow = (): boolean => {
        return this.limitTo !== this.initialLimit;
    };

    public showProject = (artifact: Models.IArtifactWithProject, index?: number): boolean => {
        if (!artifact.projectName) {
            return false;
        }

        return index === 0
            || this._sortedList[index].projectName !== this._sortedList[index - 1].projectName;
    };

    public noMoreItems = (): boolean => {
        return this.sortedList.length <= this.limitTo;
    };

    public loadMore = () => {
        this.limitTo = this.limit;
    };

    public showLoadMore = (): boolean => {
        return this.limitTo === this.initialLimit && !this.noMoreItems();
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
