import {IAppicationError, HttpStatusCode} from "./../../core";
import {IItem} from "./../../main/models/models";
import {IStatefulArtifact, IStatefulSubArtifact} from "./../../managers/artifact-manager";
import {IDispose} from "./../../managers/models";
import { INavigationService } from "../../core/navigation";


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
    static $inject: [string] = [
        "navigationService"
    ];
    
    private selectionSubject: Rx.BehaviorSubject<ISelection>;
    private explorerArtifactSelectionSubject: Rx.BehaviorSubject<IStatefulArtifact>;
    private editorArtifact: IStatefulArtifact;
    private errorObserver: Rx.IDisposable;
    constructor(private navigationService: INavigationService) {
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

        if (artifact) {
            if (!this.editorArtifact || this.editorArtifact.id !== artifact.id) {
                if (this.errorObserver) {
                    this.errorObserver.dispose();
                }
                this.errorObserver = artifact.errorObservable().subscribeOnNext(this.onArtifactError);        
            }
        }

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

    private onArtifactError = (error: IAppicationError) => {
        if (error.statusCode === HttpStatusCode.Forbidden || 
            error.statusCode === HttpStatusCode.ServerError ||
            error.statusCode === HttpStatusCode.Unauthorized
            ) {
            this.navigationService.navigateToMain();
        } 
    }
    
}
