import { IItem } from "./../../main/models/models";
import { IStatefulArtifact, IStatefulSubArtifact } from "./../../managers/models";

export interface ISelectionManager {
    artifactObservable: Rx.Observable<IStatefulArtifact>;
    subArtifactObservable: Rx.Observable<IStatefulSubArtifact>;
    selectionObservable: Rx.Observable<ISelection>;

    getArtifact(source?: SelectionSource): IStatefulArtifact;
    setArtifact(artifact: IStatefulArtifact, source?: SelectionSource);

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
    source: SelectionSource;
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

    public setArtifact(artifact: IStatefulArtifact, source?: SelectionSource) {
        const selection = <ISelection>{
            artifact: artifact,
            subArtifact: null,
            source: source
        };

        if (source === SelectionSource.Explorer) {
            this.explorerArtifact = artifact;
        } else if (source === SelectionSource.Editor) {
            this.editorArtifact = artifact;
        }

        this.selectionSubject.onNext(selection);
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
            source: val.source
        };

        this.selectionSubject.onNext(selection);
    }

    public clearAll() {
        const selection = <ISelection>{
            artifact: null,
            subArtifact: null,
            source: null
        };
        this.selectionSubject.onNext(selection);
    }

    public clearSubArtifact() {
        const val = this.selectionSubject.getValue();
        const selection = <ISelection>{
            artifact: val.artifact,
            subArtifact: null,
            source: val.source
        };

        this.selectionSubject.onNext(selection);
    }

    private distinctById(item: IItem) {
        return item ? item.id : -1;
    }
}
