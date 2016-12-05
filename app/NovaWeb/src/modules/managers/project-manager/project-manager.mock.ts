import {IProjectManager} from "./project-manager";
<<<<<<< HEAD
import {Models, AdminStoreModels} from "../../main/models";
import {IStatefulArtifact} from "../artifact-manager/artifact";
import {MoveArtifactInsertMethod} from "../../main/components/dialogs/move-artifact/move-artifact";

export class ProjectManagerMock implements IProjectManager {
    private _projectCollection: Rx.BehaviorSubject<Models.IViewModel<IStatefulArtifact>[]>;

    static $inject: [string] = [
        "$q"
    ];

    constructor(private $q: ng.IQService) {

    }

    public get projectCollection(): Rx.BehaviorSubject<Models.IViewModel<IStatefulArtifact>[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<Models.IViewModel<IStatefulArtifact>[]>([]));
    }

    public initialize() {
        //
    }
    public load(projectId: number): ng.IPromise<void> {
        return this.$q.resolve();
    }
    public add(project: AdminStoreModels.IInstanceItem) {
        //
    }
    public remove(projectId: number): void {
        //
    }
    public removeAll(): void {
        //
    }
    public refresh(id: number, forceOpen?: boolean): ng.IPromise<void> {
        return this.$q.resolve();
    }
    public refreshCurrent(): ng.IPromise<void> {
        return this.$q.resolve();
    }
    public refreshAll(): ng.IPromise<void> {
        return this.$q.resolve();
    }
    public getProject(id: number): Models.IViewModel<IStatefulArtifact> {
        return null;
    }
    public getSelectedProjectId(): number {
        return 0;
    }
    public triggerProjectCollectionRefresh() {
        //
    }
    public getDescendantsToBeDeleted(artifact: IStatefulArtifact): ng.IPromise<Models.IArtifactWithProject[]> {
        return null;
    }
    public calculateOrderIndex(insertMethod: MoveArtifactInsertMethod, selectedArtifact: Models.IArtifact): ng.IPromise<number> {
        return this.$q.resolve(0);
    }
    public dispose() {
        //
    }
}
=======
import {IViewModel, IArtifact, IArtifactWithProject} from "../../main/models/models";
import {IInstanceItem} from "../../main/models/admin-store-models";
import {IStatefulArtifact} from "../artifact-manager/artifact/artifact";
import {MoveArtifactInsertMethod} from "../../main/components/dialogs/move-artifact/move-artifact";

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

    public refresh(id: number, forceOpen?: boolean): ng.IPromise<void> {
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

    public calculateOrderIndex(insertMethod: MoveArtifactInsertMethod, selectedArtifact: IArtifact): ng.IPromise<number> {
        return this.$q.resolve(0);
    }

    public dispose(): void {
        return;
    }
}
>>>>>>> origin/develop
