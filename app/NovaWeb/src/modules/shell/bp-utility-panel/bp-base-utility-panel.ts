import { IProjectManager, Models} from "../../main";
import { IBpAccordionPanelController } from "../../main/components/bp-accordion/bp-accordion";

export class BPBaseUtilityPanelController {
    private _subscribers: Rx.IDisposable[];

    constructor(
        protected projectManager: IProjectManager, 
        public bpAccordionPanel: IBpAccordionPanelController) {
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        const selectedArtifact: Rx.Observable<Models.IArtifact> = this.projectManager.currentArtifact.asObservable();
        const panelVisibility: Rx.Observable<boolean> = this.bpAccordionPanel.isOpenObservable; 
        const artifactOrVisibilityChange: Rx.IDisposable = 
            Rx.Observable
                .combineLatest(selectedArtifact, panelVisibility, 
                    (artifact, visibility) => {
                        return { artifact: artifact, isVisible: visibility };
                    })
                .filter(o => o.isVisible)
                .map(o => o.artifact)
                .distinctUntilChanged(o => o && o.id)
                .subscribe(this.setArtifactId);
        
        this._subscribers = [ artifactOrVisibilityChange ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    protected setArtifactId = (artifact: Models.IArtifact) => {}
}
