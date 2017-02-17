import {ISelectionManager, ISelection} from "./selection-manager";
import {IStatefulArtifact, IStatefulSubArtifact} from "./../../managers/artifact-manager";
import {IItem} from "../../main/models/models";

export class SelectionManagerMock implements ISelectionManager {
    public static $inject = ["$q"];

    private selectionSubject: Rx.BehaviorSubject<ISelection>;
    private explorerArtifactSelectionSubject: Rx.BehaviorSubject<IStatefulArtifact>;

    constructor(private $q) {
        const selection = <ISelection>{
            artifact: undefined,
            subArtifact: undefined,
            multiSelect: undefined
        };
        this.selectionSubject = new Rx.BehaviorSubject<ISelection>(selection);
        this.explorerArtifactSelectionSubject = new Rx.BehaviorSubject<IStatefulArtifact>(null);
    }

    public dispose() {
        this.clearAll();
    }

    public get artifactObservable() {
        return undefined;
    }

    public get explorerArtifactObservable() {
        return undefined;
    }

    //ToDo: should be mocked individually in unit tests
    public get subArtifactObservable() {
        return this.selectionSubject
            .filter(s => s != null)
            .map(s => s.subArtifact)
            .distinctUntilChanged(this.distinctById).asObservable();
    }

    /**
     * Observable that always corresponds to the currently selected artifact's observable.
     */
    public get currentlySelectedArtifactObservable() {
        return this.selectionSubject
           .filter(selection => !!(selection && selection.artifact))
           .flatMap(selection => selection.artifact.getObservable())
           .asObservable();
    }

    public get selectionObservable() {
        return this.selectionSubject.asObservable();
    }

    public getArtifact(): IStatefulArtifact {
        return this.selectionSubject.getValue().artifact;
    }

    //ToDo: should be mocked individually in unit tests
    public setArtifact(artifact: IStatefulArtifact) {

        const selection = <ISelection>{
            artifact: artifact,
            subArtifact: undefined,
            multiSelect: undefined
        };

        this.setSelectionSubject(selection);
    }

    //ToDo: should be mocked individually in unit tests
    public getSubArtifact(): IStatefulSubArtifact {
        const val = this.selectionSubject.getValue();
        if (val && val.subArtifact) {
            return val.subArtifact;
        }

        return null;
    }

    //ToDo: should be mocked individually in unit tests
    public setSubArtifact(subArtifact: IStatefulSubArtifact) {
        const val = this.selectionSubject.getValue();
        const selection = <ISelection>{
            artifact: val.artifact,
            subArtifact: subArtifact,
            multiSelect: undefined
        };

        this.setSelectionSubject(selection);
    }

    public getExplorerArtifact() {
        return this.explorerArtifactSelectionSubject.getValue();
    }

    public getArtifactProjectId(): number {
        return 1;
    }

    public setExplorerArtifact(artifact: IStatefulArtifact) {
        return undefined;
    }

    public clearAll() {
        return undefined;
    }

    public clearSubArtifact() {
        return undefined;
    }

    private distinctById(item: IItem) {
        return item ? item.id : -1;
    }

    private setSelectionSubject(selection: ISelection) {
        this.selectionSubject.onNext(selection);
    }

    public autosave(): ng.IPromise<any> {
        return this.$q.resolve();
    }
}
