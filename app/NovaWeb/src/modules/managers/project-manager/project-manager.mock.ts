import { IProjectManager, IArtifactNode } from "./project-manager";
import { Models, AdminStoreModels, Enums } from "../../main/models";
import { IStatefulArtifact, StatefulArtifact } from "../artifact-manager/artifact";

export class ProjectManagerMock implements IProjectManager {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) { }


    private _projectCollection: Rx.BehaviorSubject<IArtifactNode[]>;

    get projectCollection(): Rx.BehaviorSubject<IArtifactNode[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<IArtifactNode[]>([]));
    }

    loadFolders = (id?: number): ng.IPromise<AdminStoreModels.IInstanceItem[]> => {
        const deferred = this.$q.defer<AdminStoreModels.IInstanceItem[]>();
        const items = [{ id: 1, name: "test", type: 1, parentFolderId: 0, hasChildren: false }];
        deferred.resolve(items);
        return deferred.promise;
    };
    getProject = (id: number) => { return null; };
    getArtifact = (artifactId: number): IStatefulArtifact => {
        let artifact: Models.IArtifact = { hasChildren: true, id: 1 };

        return new StatefulArtifact(artifact, null);
    };

    initialize() { /*do nothing*/ }
    removeAll(): void { /*do nothing*/ }
    refresh(id: number, forceOpen?: boolean): ng.IPromise<void> { return null; }
    refreshCurrent(): ng.IPromise<void> { return null; }
    refreshAll(): ng.IPromise<void> { return null; }
    loadArtifact(id: number): void { /*do nothing*/ }
    getArtifactNode(id: number): IArtifactNode { return null; }
    getSelectedProject(): IArtifactNode { return null; }
    triggerProjectCollectionRefresh() { /*do nothing*/ }
    getDescendantsToBeDeleted(artifact: IStatefulArtifact): ng.IPromise<Models.IArtifactWithProject[]> { return null; }

    add(project: AdminStoreModels.IInstanceItem) {
        return null;
    }

    remove(projectId: number): void {
        return null;
    }

    dispose() {
        /*do nothing*/
    }
}
