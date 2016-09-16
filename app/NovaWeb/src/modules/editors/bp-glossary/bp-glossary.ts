import { IGlossaryService, IGlossaryTerm } from "./glossary.svc";
import { ILocalizationService, IMessageService } from "../../core";

// import { ISelectionManager, ISelection, SelectionSource } from "../../main/services/selection-manager";
import { IArtifactManager, IStatefulArtifact, IStatefulSubArtifact, IStatefulArtifactFactory } from "../../managers/artifact-manager";
import { ISelectionManager, ISelection, SelectionSource } from "../../managers/selection-manager";
import { Models } from "../../main/models";
import { BpBaseEditor } from "../bp-base-editor";


export class BpGlossary implements ng.IComponentOptions {
    public template: string = require("./bp-glossary.html");
    public controller: Function = BpGlossaryController;
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

    public onLoad() {
        // TODO: move this to sub-artifact
        this.glossaryService.getGlossary(this.artifact.id).then((result: Models.IArtifact) => {
            result.subArtifacts = result.subArtifacts.map((term: IGlossaryTerm) => {
                term.description = this.$sce.trustAsHtml(term.description);

                const stateful = this.statefulArtifactFactory.createStatefulArtifact(term);
                this.artifact.subArtifactCollection.add(stateful);

                return term;
            });

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
        // if (this.glossary) {
        //     this.glossary.subArtifacts = this.glossary.subArtifacts.map((t: IGlossaryTerm) => {
        //         t.selected = false;
        //         return t;
        //     });
        // }
    }

    /*public selectTerm(term: IGlossaryTerm) {
        if (term.selected) {
            return;
        }
        this.glossary.subArtifacts = this.glossary.subArtifacts.map((t: IGlossaryTerm) => {
            t.selected = t === term;
            return t;
        });
        // const oldSelection = this.selectionManager.selection;
        // const selection: ISelection = {
        //     source: SelectionSource.Editor,
        //     artifact: oldSelection.artifact,
        //     subArtifact: term
        // };
        // this.selectionManager.selection = selection;
        const subArtifact = this.statefulArtifactFactory.createStatefulSubArtifact(this.glossary, term);
        this.artifactManager.selection.setSubArtifact(this.artifact);
    }*/

    private stopPropagation(event: JQueryEventObject) {
        if (event.target.tagName !== "TH") {
            event.stopPropagation();
        }
    }
}
