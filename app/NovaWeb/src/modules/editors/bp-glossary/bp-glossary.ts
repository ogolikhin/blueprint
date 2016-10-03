import { IGlossaryService } from "./glossary.svc";
import { ILocalizationService, IMessageService } from "../../core";
import { IArtifactManager, IStatefulSubArtifact, IStatefulArtifactFactory } from "../../managers/artifact-manager";
import { Models } from "../../main/models";
import { BpBaseEditor } from "../bp-base-editor";


export class BpGlossary implements ng.IComponentOptions {
    public template: string = require("./bp-glossary.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpGlossaryController;
    public bindings: any = {
        context: "<"
    };
}

export class BpGlossaryController extends BpBaseEditor {
    public static $inject: [string] = [
        "$element",
        "$log",
        "localization",
        "glossaryService",
        "$sce",
        "messageService",
        "artifactManager",
        "statefulArtifactFactory"
    ];

    public isLoading: boolean = true;
    public terms: IStatefulSubArtifact[];
    public selectedTerm: IStatefulSubArtifact;

    constructor(
        private $element: ng.IAugmentedJQuery,
        private $log: ng.ILogService,
        private localization: ILocalizationService, 
        private glossaryService: IGlossaryService,
        private $sce: ng.ISCEService,
        public messageService: IMessageService,
        public artifactManager: IArtifactManager,
        private statefulArtifactFactory: IStatefulArtifactFactory) {

            super(messageService, artifactManager);
    }

    public $onInit() {
        super.$onInit();
        this.subscribers.push(this.artifactManager.selection.subArtifactObservable.filter(s => s == null).subscribeOnNext(this.clearSelection, this));
        this.$element.on("click", this.stopPropagation);
        this.terms = [];
    }

    public $onDestroy() {
        super.$onDestroy();
        this.$element.off("click", this.stopPropagation);
    }

    public onArtifactReady() {
        super.onArtifactReady();
        
        // TODO: move this to sub-artifact
        let statefulSubartifacts = [];
        this.glossaryService.getGlossary(this.artifact.id).then((result: Models.IArtifact) => {
            if (this.isDestroyed) {
                return;
            }
            result.subArtifacts = result.subArtifacts.map((term: Models.ISubArtifact) => {

                // TODO: should not same $sce wrapper in StatefulSubArtifact model (after MVP)
                term.description = this.$sce.trustAsHtml(term.description);

                const stateful = this.statefulArtifactFactory.createStatefulSubArtifact(this.artifact, term);
                statefulSubartifacts.push(stateful);

                return term;
            });
            this.artifact.subArtifactCollection.initialise(statefulSubartifacts);
            this.terms = this.artifact.subArtifactCollection.list();

        }).catch((error: any) => {
            //ignore authentication errors here
            if (error) {
                this.messageService.addError(error["message"] || "Artifact_NotFound");
            }
        }).finally(() => {
            this.isLoading = false;
        });
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

    private stopPropagation(event: JQueryEventObject) {
        if (event.target.tagName !== "TH") {
            event.stopPropagation();
        }
    }
}
