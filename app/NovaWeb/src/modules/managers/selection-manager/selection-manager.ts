import { IItem } from "./../../main/models/models";
import { IStatefulArtifact, IStatefulSubArtifact, IDispose } from "./../../managers/models";

export interface ISelectionManager extends IDispose {
    artifactObservable: Rx.Observable<IStatefulArtifact>;
    subArtifactObservable: Rx.Observable<IStatefulSubArtifact>;
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

export enum SelectionSource {
    None = 0,
    Explorer = 1,
    Editor = 2
}

export interface ISelection {
    artifact?: IStatefulArtifact;
    subArtifact?: IStatefulSubArtifact;
}

export class SelectionManager implements ISelectionManager {
    private selectionSubject: Rx.BehaviorSubject<ISelection>;
    private explorerArtifact: IStatefulArtifact;
    private editorArtifact: IStatefulArtifact;

    constructor() {
        const selection = <ISelection>{
            artifact: null,
            subArtifact: null,
            source: null
        };
        this.selectionSubject = new Rx.BehaviorSubject<ISelection>(selection);
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

    public get subArtifactObservable() {
        return this.selectionSubject
            .filter(s => s != null)
            .map(s => s.subArtifact)
            .distinctUntilChanged(this.distinctById).asObservable();
    }

    public get selectionObservable() {
        return this.selectionSubject.asObservable();
    }

    public getArtifact(source?: SelectionSource): IStatefulArtifact {
        if (source === SelectionSource.Explorer) {
            return this.explorerArtifact;
        } else if (source === SelectionSource.Editor) {
            return this.editorArtifact;
        }

        const val = this.selectionSubject.getValue();
        if (val && val.artifact) {
            return val.artifact;
        }
        return null;
    }

    public setArtifact(artifact: IStatefulArtifact) {
        const selection = <ISelection>{
            artifact: artifact,
            subArtifact: null,
        };
        this.editorArtifact = artifact;
        this.setSubject(selection);
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
            subArtifact: subArtifact,
        };

        this.setSubject(selection);
    }

    public getExplorerArtifact() {
        return this.explorerArtifact;
    }

    public setExplorerArtifact(artifact: IStatefulArtifact) {
        this.explorerArtifact = artifact;
    }

    public clearAll() {
        const selection = <ISelection>{
            artifact: null,
            subArtifact: null,
            source: null
        };
        this.setSubject(selection);
    }

    public clearSubArtifact() {
        const val = this.selectionSubject.getValue();
        const selection = <ISelection>{
            artifact: val.artifact,
            subArtifact: null,
        };

        this.setSubject(selection);
    }

    private distinctById(item: IItem) {
        return item ? item.id : -1;
    }

    private setSubject(selection: ISelection) {
        this.selectionSubject.onNext(selection);
    }
}
