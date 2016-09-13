import { Models} from "../../main";
import { IStateManager, ItemState } from "../../core";
import { IBpAccordionPanelController } from "../../main/components/bp-accordion/bp-accordion";
import { IStatefulArtifact, IStatefulSubArtifact } from "../../managers/artifact-manager";
import { ISelectionManager } from "../../managers/selection-manager";

export class BPBaseUtilityPanelController {
    private _subscribers: Rx.IDisposable[];
    private timeout: ng.IDeferred<void>;
    protected itemState: ItemState;

    constructor(
        protected $q: ng.IQService,
        protected selectionManager: ISelectionManager, 
        protected stateManager: IStateManager,
        public bpAccordionPanel: IBpAccordionPanelController) {
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        const stateObservable = this.stateManager.stateChange.asObservable()
            .filter((is: ItemState) => {
                return !this.itemState || is.isReadonly !== this.itemState.isReadonly;
            })
            .subscribeOnNext(this.stateChanged, this);
        const selectionObservable = this.selectionManager.selectionObservable;
        const panelActiveObservable = this.bpAccordionPanel.isActiveObservable; 
        const artifactOrVisibilityChange: Rx.IDisposable = 
            Rx.Observable
                .combineLatest(selectionObservable, panelActiveObservable, 
                    (selection, isActive) => {
                        return { selection: selection, isActive: isActive };
                    })
                .filter(o => o.selection && o.isActive)
                .map(o => {
                    return { artifact: o.selection.artifact, subArtifact: o.selection.subArtifact };
                })
                .distinctUntilChanged()
                .subscribe(s => this.selectionChanged(s.artifact, s.subArtifact));
        
        this._subscribers = [ artifactOrVisibilityChange, stateObservable ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    protected stateChanged(state: ItemState) {
        this.itemState = state;
    }

    private selectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact) {
        if (this.timeout) {
            this.timeout.resolve();
        }

        this.timeout = this.$q.defer<any>();
        const selectionChangedResult = this.onSelectionChanged(artifact, subArtifact, this.timeout.promise);
        if (selectionChangedResult) {
            selectionChangedResult.then(() =>
                this.timeout = undefined
            );
        }
    }

    protected onSelectionChanged(artifact: Models.IArtifact, subArtifact: Models.ISubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        return this.$q.resolve();
    }
}
