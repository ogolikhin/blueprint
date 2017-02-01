import {ILocalizationService} from "../../commonModule/localization/localization.service";
import {IStatefulSubArtifact} from "../../managers/artifact-manager";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {BpBaseEditor} from "../bp-base-editor";

export class BpGlossaryController extends BpBaseEditor {
    public static $inject: [string] = [
        "$log",
        "localization",
        "$sce",
        "selectionManager"
    ];

    public terms: IStatefulSubArtifact[];
    public selectedTerm: IStatefulSubArtifact;

    constructor(private $log: ng.ILogService,
                public localization: ILocalizationService,
                public $sce: ng.ISCEService,
                public selectionManager: ISelectionManager) {
        super(selectionManager);
    }

    public $onInit() {
        super.$onInit();
        this.subscribers.push(this.selectionManager.subArtifactObservable.filter(s => s == null).subscribeOnNext(this.clearSelection, this));
        this.terms = [];
    }

    protected destroy(): void {
        this.terms = undefined;
        this.selectedTerm = undefined;

        super.destroy();
    }

    protected onArtifactReady() {
        super.onArtifactReady();
        this.terms = this.artifact.subArtifactCollection.list();
    }

    private clearSelection() {
        this.selectedTerm = null;
        this.selectionManager.clearSubArtifact();
    }

    public selectTerm(term: IStatefulSubArtifact) {
        if (term !== this.selectedTerm) {
            this.selectedTerm = term;
            this.selectionManager.setSubArtifact(this.selectedTerm);
        }
    }
}
