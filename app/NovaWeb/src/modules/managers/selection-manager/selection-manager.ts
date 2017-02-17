import {IItem} from "../../main/models/models";
import {IDialogSettings} from "../../shared";
import {IDispose} from "../models";
import {IStatefulArtifact, IStatefulSubArtifact} from "./../../managers/artifact-manager";

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
    setSubArtifact(subArtifact: IStatefulSubArtifact, multiSelect?: boolean, isDeleted?: boolean);
    autosave(showConfirm?: boolean): ng.IPromise<any>;
    clearAll();
    clearSubArtifact();

    getArtifactProjectId(): number;
}

export interface ISelection {
    artifact?: IStatefulArtifact;
    subArtifact?: IStatefulSubArtifact;
    multiSelect?: boolean;
    isDeleted?: boolean;
}

export class SelectionManager implements ISelectionManager {

    private selectionSubject: Rx.BehaviorSubject<ISelection>;
    private explorerArtifactSelectionSubject: Rx.BehaviorSubject<IStatefulArtifact>;

    static $inject: [string] = [
        "$q",
        "dialogService",
        "loadingOverlayService"
    ];

    constructor(private $q, private dialogService, private loadingOverlayService) {
        const selection = <ISelection>{
            artifact: undefined,
            subArtifact: undefined,
            multiSelect: undefined,
            isDeleted: undefined
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
            .asObservable();
    }

    public get explorerArtifactObservable() {
        return this.explorerArtifactSelectionSubject
            //.distinctUntilChanged(this.distinctById)
            .asObservable();
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

    public getArtifactProjectId(): number {
        const artifact = this.selectionSubject.getValue().artifact;
        if (!artifact) {
            return null;
        }

        return artifact.projectId;
    }

    public setArtifact(artifact: IStatefulArtifact) {

        const selection = <ISelection>{
            artifact: artifact,
            subArtifact: undefined,
            multiSelect: undefined
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

    public setSubArtifact(subArtifact: IStatefulSubArtifact,
        multiSelect: boolean = false, isDeleted: boolean = false) {
        const val = this.selectionSubject.getValue();
        const selection = <ISelection>{
            artifact: val.artifact,
            subArtifact: subArtifact,
            multiSelect: multiSelect,
            isDeleted: isDeleted
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
            subArtifact: undefined,
            multiSelect: undefined,
            isDeleted: undefined
        };
        this.setExplorerArtifact(undefined);
        this.setSelectionSubject(emptyselection);
    }

    public clearSubArtifact() {
        const val = this.selectionSubject.getValue();
        const selection = <ISelection>{
            artifact: val.artifact,
            subArtifact: undefined,
            multiSelect: undefined,
            isDeleted: undefined 
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
                prevSelection.artifact.unload();

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

    public autosave(showConfirm: boolean = true): ng.IPromise<any> {
        const artifact = this.getArtifact();
        if (artifact) {
            let autosaveId = this.loadingOverlayService.beginLoading();
            return artifact.save(true).catch((error) => {
                if (showConfirm) {
                    return this.dialogService.open(<IDialogSettings>{
                        okButton: "App_Button_Proceed",
                        message: "App_Save_Auto_Confirm",
                        header: "App_DialogTitle_Alert",
                        css: "modal-alert nova-messaging"
                    }).then(() => {
                        artifact.discard();
                    });
                } else {
                    return this.$q.reject(error);
                }
            }).finally(() => this.loadingOverlayService.endLoading(autosaveId));
        }
        return this.$q.resolve();
    }
}
