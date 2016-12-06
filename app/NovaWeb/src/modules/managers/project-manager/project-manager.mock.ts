import {IProjectManager} from "./project-manager";
import {IViewModel, IArtifact, IArtifactWithProject} from "../../main/models/models";
import {IInstanceItem} from "../../main/models/admin-store-models";
import {IStatefulArtifact} from "../artifact-manager/artifact/artifact";
import {MoveCopyArtifactInsertMethod} from "../../main/components/dialogs/move-copy-artifact/move-copy-artifact";

export class ProjectManagerMock implements IProjectManager {
    public projectCollection: Rx.BehaviorSubject<IViewModel<IStatefulArtifact>[]>;

    static $inject: string[] = [
        "$q"
    ];

    constructor(public $q: ng.IQService) {
        this.projectCollection = new Rx.BehaviorSubject<IViewModel<IStatefulArtifact>[]>(undefined);
    }

    // eventManager
    public initialize() {
        return;
    }

    public load(projectId: number): ng.IPromise<void> {
        return this.$q.resolve();
    }

    public add(project: IInstanceItem) {
        return;
    }

    public openProjectAndExpandToNode(projectId: number, artifactIdToExpand: number): ng.IPromise<void> {
        return this.$q.resolve();
    }

    public openProjectWithDialog(): void {
        return;
    }

    public remove(projectId: number): void {
        return;
    }

    public removeAll(): void {
        return;
    }

    public refresh(id: number, selectionId?: number, forceOpen?: boolean): ng.IPromise<void> {
        return this.$q.resolve();
    }

    public refreshCurrent(): ng.IPromise<void> {
        return this.$q.resolve();
    }

    public refreshAll(): ng.IPromise<void> {
        return this.$q.resolve();
    }

    public getProject(id: number): IViewModel<IStatefulArtifact> {
        return undefined;
    }

    public getSelectedProjectId(): number {
        return undefined;
    }

    public triggerProjectCollectionRefresh(): void {
        return;
    }

    public getDescendantsToBeDeleted(artifact: IStatefulArtifact): ng.IPromise<IArtifactWithProject[]> {
        return this.$q.resolve([]);
    }

    public calculateOrderIndex(insertMethod: MoveCopyArtifactInsertMethod, selectedArtifact: IArtifact): ng.IPromise<number> {
        return this.$q.resolve(0);
    }

    public dispose(): void {
        return;
    }
}
