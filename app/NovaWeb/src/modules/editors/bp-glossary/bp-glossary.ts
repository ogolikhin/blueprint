import { IGlossaryService, IGlossaryTerm } from "./glossary.svc";
import { ILocalizationService, IMessageService } from "../../core";

import { ISelectionManager, ISelection, SelectionSource } from "../../main/services/selection-manager";
import { Models } from "../../main/models";

import { 
    IArtifactManager, 
    BpBaseEditor 
} from "../bp-base-editor";



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
        "selectionManager",
        "$sce",
        "messageService",
        "artifactManager"
    ];


    public glossary: Models.IArtifact;
    public isLoading: boolean = true; 

    constructor(
        private $element: ng.IAugmentedJQuery,
        private $log: ng.ILogService,
        private localization: ILocalizationService, 
        private glossaryService: IGlossaryService,
        private selectionManager: ISelectionManager,
        private $sce: ng.ISCEService,
        messageService: IMessageService,
        public artifactManager: IArtifactManager) {
            super(messageService, artifactManager);
    }

    public $onInit() {
        super.$onInit();
        this.subscribers.push(this.selectionManager.selectedSubArtifactObservable.filter(s => s == null).subscribeOnNext(this.clearSelection, this));
        this.$element.on("click", this.stopPropagation);
    }

    public $onDestroy() {
        super.$onDestroy();
        this.$element.off("click", this.stopPropagation);
    }

    public onLoad() {
        this.glossary = null;

        this.glossaryService.getGlossary(this.artifact.id).then((result: Models.IArtifact) => {
            result.subArtifacts = result.subArtifacts.map((term: IGlossaryTerm) => {
                term.description = this.$sce.trustAsHtml(term.description);
                return term;
            });

            this.glossary = result;

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
        if (this.glossary) {
            this.glossary.subArtifacts = this.glossary.subArtifacts.map((t: IGlossaryTerm) => {
                t.selected = false;
                return t;
            });
        }
    }

    public selectTerm(term: IGlossaryTerm) {
        if (term.selected) {
            return;
        }
        this.glossary.subArtifacts = this.glossary.subArtifacts.map((t: IGlossaryTerm) => {
            t.selected = t === term;
            return t;
        });
        const oldSelection = this.selectionManager.selection;
        const selection: ISelection = {
            source: SelectionSource.Editor,
            artifact: oldSelection.artifact,
            subArtifact: term
        };
        this.selectionManager.selection = selection;
    }
    private stopPropagation(event: JQueryEventObject) {
        if (event.target.tagName !== "TH") {
            event.stopPropagation();
        }
    }
}
