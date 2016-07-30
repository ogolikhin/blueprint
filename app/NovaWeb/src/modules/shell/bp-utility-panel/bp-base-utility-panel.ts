import { ISelectionManager, Models} from "../../main";
import { IBpAccordionPanelController } from "../../main/components/bp-accordion/bp-accordion";

export class BPBaseUtilityPanelController {
    private _subscribers: Rx.IDisposable[];

    constructor(
        protected selectionManager: ISelectionManager, 
        public bpAccordionPanel: IBpAccordionPanelController) {
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        const selectionObservable = this.selectionManager.selectionObservable;
        const panelVisibility = this.bpAccordionPanel.isOpenObservable; 
        const artifactOrVisibilityChange: Rx.IDisposable = 
            Rx.Observable
                .combineLatest(selectionObservable, panelVisibility, 
                    (selection, visibility) => {
                        return { selection: selection, isVisible: visibility };
                    })
                .filter(o => o.selection && o.isVisible)
                .map(o => o.selection)
                .distinctUntilChanged()
                .subscribe(s => this.onSelectionChanged(s.artifact, s.subArtifact));
        
        this._subscribers = [ artifactOrVisibilityChange ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    protected onSelectionChanged = (artifact: Models.IArtifact, subArtifact: Models.ISubArtifact) => {

    }
}
