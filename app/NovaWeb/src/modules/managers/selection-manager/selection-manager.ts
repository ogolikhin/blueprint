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

    constructor() {
        const selection = <ISelection>{
            artifact: undefined,
            subArtifact: undefined
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
           .filter(selection => !!(selection && selection.artifact))
           .flatMap(selection => selection.artifact.getObservable())
           //.distinctUntilChanged(this.distinctById) -Don't re-enable without testing refreshing a deleted artifact; we need every artifact event.
           .asObservable();
   }

    public get selectionObservable() {
        return this.selectionSubject.asObservable();
    }

    public getArtifact(): IStatefulArtifact {
        return this.selectionSubject.getValue().artifact;
    }

    public setArtifact(artifact: IStatefulArtifact) {

        const selection = <ISelection>{
            artifact: artifact,
            subArtifact: undefined
        };

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
        const emptyselection = <ISelection>{
            artifact: undefined,
            subArtifact: undefined
        };
        this.setExplorerArtifact(undefined);
        this.setSelectionSubject(emptyselection);
    }

    public clearSubArtifact() {
        const val = this.selectionSubject.getValue();
        const selection = <ISelection>{
            artifact: val.artifact,
            subArtifact: undefined
        };

        this.setSelectionSubject(selection);
    }

    private distinctById(item: IItem) {
        return item ? item.id : -1;
    }


    private unsubscribe(selection: ISelection) {
        const prevSelection = this.selectionSubject.getValue();

        if (prevSelection && selection) {
            if (prevSelection.artifact && !_.isEqual(prevSelection.artifact, selection.artifact)) {
                prevSelection.artifact.unsubscribe();
                if (prevSelection.subArtifact && !_.isEqual(prevSelection.subArtifact, selection.subArtifact)) {
                    prevSelection.subArtifact.unsubscribe();
                }
            }
        }
    }

    private setSelectionSubject(selection: ISelection) {
        this.unsubscribe(selection);
        this.selectionSubject.onNext(selection);
    }

}
