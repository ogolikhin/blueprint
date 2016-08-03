import { IProject, IArtifact, ISubArtifact, IItem } from "./../models/models";

export interface ISelectionManager {
    selectedProjectObservable: Rx.Observable<IProject>;
    selectedArtifactObservable: Rx.Observable<IArtifact>;
    selectedSubArtifactObservable: Rx.Observable<ISubArtifact>;
    selectedItemObservable: Rx.Observable<IItem>;

    selectionObservable: Rx.Observable<ISelection>;
    selection: ISelection;
    clearSelection();
}

export enum SelectionSource {
    Explorer = 1,
    Editor = 2
}

export interface ISelection {
    source: SelectionSource;
    artifact: IArtifact;
    project: IProject;
    subArtifact?: ISubArtifact;
}

/**
 * Use SelectionManager to get or set current selection
 */
export class SelectionManager implements ISelectionManager {

    private selectionSubject: Rx.BehaviorSubject<ISelection>;

    constructor() {
        this.selectionSubject = new Rx.BehaviorSubject<ISelection>(null);
    }

    public get selectedProjectObservable() {
        return this.selectionSubject
            .filter(s => s != null)
            .map(s => s.project)
            .distinctUntilChanged(this.distinctById).asObservable();
    }

    public get selectedArtifactObservable() {
        return this.selectionSubject
            .filter(s => s != null)
            .map(s => s.artifact)
            .distinctUntilChanged(this.distinctById).asObservable();
    }

    public get selectedSubArtifactObservable() {
        return this.selectionSubject
            .filter(s => s != null)
            .map(s => s.subArtifact)
            .distinctUntilChanged(this.distinctById).asObservable();
    }

    public get selectedItemObservable() {
        return this.selectionSubject
            .filter(s => s != null)
            .map(s => this.getSelectedItem(s))
            .distinctUntilChanged(this.distinctById).asObservable();
    }

    private getSelectedItem(selection: ISelection): IItem {
        if (selection) {
            if (selection.subArtifact) {
                return selection.subArtifact;
            }
            if (selection.artifact) {
                return selection.artifact;
            }
            return selection.project;
        }
        return null;
    }

    private distinctById(item: IItem) {
        return item ? item.id : -1;
    }

    public get selection() {
        return this.selectionSubject.getValue();
    }

    public set selection(value: ISelection) {
        this.selectionSubject.onNext(value);
    }

    public get selectionObservable() {
        return this.selectionSubject.asObservable();
    }

    public clearSelection() {
        this.selectionSubject.onNext({ project: null, artifact: null, source: SelectionSource.Explorer });
    }
}