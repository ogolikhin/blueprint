import {IProjectExplorerService} from "./project-explorer.service";
import {IViewModel, IArtifact, IArtifactWithProject} from "../../models/models";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {ExplorerNodeVM} from "../../models/tree-node-vm-factory";
import {IInstanceItem} from "../../models/admin-store-models";
import {IItemInfoResult} from "../../../commonModule/itemInfo/itemInfo.service";
import {MoveCopyArtifactInsertMethod} from "../dialogs/move-copy-artifact/move-copy-artifact";
import {IChangeSet} from "../../../managers/artifact-manager/changeset/changeset";

export class ProjectExplorerServiceMock implements IProjectExplorerService {
    public projects: ExplorerNodeVM[];
    projectsChangeObservable: Rx.Observable<IChangeSet>;

    static $inject: string[] = [
        "$q"
    ];

    constructor(public $q: ng.IQService) {
        // this.projectCollection = new Rx.BehaviorSubject<IViewModel<IStatefulArtifact>[]>(undefined);
    }

    public setSelectionId(id: number) {
        //
    }

    public getSelectionId(): number {
        return 1;
    }

    public add(projectId: number): ng.IPromise<void> {
        return this.$q.resolve();
    }

    public openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void> {
        return this.$q.resolve();
    }

    public openProjectWithDialog() {
        return;
    }

    public remove(projectId: number) {
        return;
    }

    public removeAll() {
        return;
    }

    public refresh(projectId: number, expandToArtifact?: IArtifact): ng.IPromise<ExplorerNodeVM> {
        return this.$q.resolve({} as ExplorerNodeVM);
    }

    public refreshAll(): ng.IPromise<any> {
        return this.$q.resolve();
    }

    public getProject(id: number): ExplorerNodeVM {
        return undefined;
    }

    public getDescendantsToBeDeleted(artifact: IStatefulArtifact): ng.IPromise<IArtifactWithProject[]> {
        return this.$q.resolve([]);
    }

    public calculateOrderIndex(insertMethod: MoveCopyArtifactInsertMethod, selectedArtifact: IArtifact): ng.IPromise<number> {
        return this.$q.resolve(0);
    }

    public openProject(projectId: IInstanceItem | IItemInfoResult): ng.IPromise<void> {
        return this.$q.resolve();
    }
}
