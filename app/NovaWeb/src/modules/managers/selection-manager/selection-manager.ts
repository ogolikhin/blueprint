import {IItem} from "./../../main/models/models";
import {IStatefulArtifact, IStatefulSubArtifact} from "./../../managers/artifact-manager";
import {IDispose} from "./../../managers/models";

export interface ISelectionManager extends IDispose {
    artifactObservable: Rx.Observable<IStatefulArtifact>;
    explorerArtifactObservable: Rx.Observable<IStatefulArtifact>;
    subArtifactObservable: Rx.Observable<IStatefulSubArtifact>;
    currentlySelectedArtifactObservable: Rx.Observable<IStatefulArtifact>;
    selectionObservable: Rx.Observable<ISelection>;

    getArtifact(): IStatefulArtifact;
    setArtifact(artifact: IStatefulArtifact);
    getExplorerArtifact();
    setExplorerArtifact(artifact: IStatefulArtifact);

    getSubArtifact(): IStatefulSubArtifact;
    setSubArtifact(subArtifact: IStatefulSubArtifact);

    clearAll();
    clearSubArtifact();
}

export interface ISelection {
    artifact?: IStatefulArtifact;
    subArtifact?: IStatefulSubArtifact;
}

export class SelectionManager implements ISelectionManager {
    private selectionSubject: Rx.BehaviorSubject<ISelection>;
    private explorerArtifactSelectionSubject: Rx.BehaviorSubject<IStatefulArtifact>;
    private editorArtifact: IStatefulArtifact;

    constructor() {
        const selection = <ISelection>{
            artifact: null,
            subArtifact: null
        };
        this.selectionSubject = new Rx.BehaviorSubject<ISelection>(selection);
        this.explorerArtifactSelectionSubject = new Rx.BehaviorSubject<IStatefulArtifact>(null);
    }

    public dispose() {
        this.clearAll();
    }

    public get artifactObservable() {
        return this.selectionSubject
            .filter(s => s != null)
            .map(s => s.artifact)
            .distinctUntilChanged(this.distinctById).asObservable();
    }

    public get explorerArtifactObservable() {
        return this.explorerArtifactSelectionSubject
            .filter(s => s != null)
            .distinctUntilChanged(this.distinctById).asObservable();
    }

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
           .filter(selection => selection != null && selection.artifact != null)
           .flatMap(selection => selection.artifact.getObservable())
           //.distinctUntilChanged(this.distinctById) -Don't re-enable without testing refreshing a deleted artifact; we need every artifact event.
           .asObservable();
   }

    public get selectionObservable() {
        return this.selectionSubject.asObservable();
    }

    public getArtifact(): IStatefulArtifact {
        return this.editorArtifact;
    }

    public setArtifact(artifact: IStatefulArtifact) {
        const selection = <ISelection>{
            artifact: artifact,
            subArtifact: null
        };
        this.editorArtifact = artifact;
        this.setSelectionSubject(selection);
    }

    public getSubArtifact(): IStatefulSubArtifact {
        const val = this.selectionSubject.getValue();
        if (val && val.subArtifact) {
            return val.subArtifact;
        }

        return null;
    }

    public setSubArtifact(subArtifact: IStatefulSubArtifact) {
        const val = this.selectionSubject.getValue();
        const selection = <ISelection>{
            artifact: val.artifact,
            subArtifact: subArtifact
        };

        this.setSelectionSubject(selection);
    }

    public getExplorerArtifact() {
        return this.explorerArtifactSelectionSubject.getValue();
    }

    public setExplorerArtifact(artifact: IStatefulArtifact) {
        this.explorerArtifactSelectionSubject.onNext(artifact);
    }

    public clearAll() {
        const selection = <ISelection>{
            artifact: null,
            subArtifact: null
        };
        this.setExplorerArtifact(null);
        this.setSelectionSubject(selection);
    }

    public clearSubArtifact() {
        const val = this.selectionSubject.getValue();
        const selection = <ISelection>{
            artifact: val.artifact,
            subArtifact: null
        };

        this.setSelectionSubject(selection);
    }

    private distinctById(item: IItem) {
        return item ? item.id : -1;
    }

    private setSelectionSubject(selection: ISelection) {
        this.selectionSubject.onNext(selection);
    }
}
