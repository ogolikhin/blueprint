import {IArtifactManager, IStatefulSubArtifact} from "../../managers/artifact-manager";
import {BpBaseEditor} from "../bp-base-editor";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";

export class BpGlossary implements ng.IComponentOptions {
    public template: string = require("./bp-glossary.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpGlossaryController;
}

export class BpGlossaryController extends BpBaseEditor {
    public static $inject: [string] = [
        "$log",
        "localization",
        "$sce",
        "messageService",
        "artifactManager"
    ];

    public terms: IStatefulSubArtifact[];
    public selectedTerm: IStatefulSubArtifact;

    constructor(private $log: ng.ILogService,
                public localization: ILocalizationService,
                public $sce: ng.ISCEService,
                public messageService: IMessageService,
                public artifactManager: IArtifactManager) {

        super(messageService, artifactManager);
    }

    public $onInit() {
        super.$onInit();
        this.subscribers.push(this.artifactManager.selection.subArtifactObservable.filter(s => s == null).subscribeOnNext(this.clearSelection, this));
        this.terms = [];
    }

    public $onDestroy() {
        super.$onDestroy();
        delete this.terms;
        delete this.selectedTerm;
    }

    public onArtifactReady() {
        super.onArtifactReady();
        this.terms = this.artifact.subArtifactCollection.list();
    }

    private clearSelection() {
        this.selectedTerm = null;
        this.artifactManager.selection.clearSubArtifact();
    }

    public selectTerm(term: IStatefulSubArtifact) {
        if (term !== this.selectedTerm) {
            this.selectedTerm = term;
            this.artifactManager.selection.setSubArtifact(this.selectedTerm);
        }
    }
}
