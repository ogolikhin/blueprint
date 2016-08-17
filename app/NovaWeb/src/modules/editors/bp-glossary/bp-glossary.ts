import { IGlossaryDetails, IGlossaryService, IGlossaryTerm } from "./glossary.svc";
import { ILocalizationService } from "../../core";
import { ISelectionManager, ISelection, SelectionSource } from "../../main/services/selection-manager";
import { ItemTypePredefined } from "./../../main/models/enums";

export class BpGlossary implements ng.IComponentOptions {
    public template: string = require("./bp-glossary.html");
    public controller: Function = BpGlossaryController;
    public bindings: any = {
        context: "<"
    };
}

export class BpGlossaryController {
    public static $inject: [string] = [
        "$element",
        "$log",
        "localization",
        "glossaryService",
        "selectionManager",
        "$sce"
    ];

    private _context: number; // Models.IArtifact;
    private subscribers: Rx.IDisposable[];

    public glossary: IGlossaryDetails;
    public isLoading: boolean = true;

    constructor(
        private $element: ng.IAugmentedJQuery,
        private $log: ng.ILogService,
        private localization: ILocalizationService, 
        private glossaryService: IGlossaryService,
        private selectionManager: ISelectionManager,
        private $sce: ng.ISCEService) {
    }

    public $onInit() {
        this.subscribers = [
            this.selectionManager.selectedSubArtifactObservable.filter(s => s == null).subscribeOnNext(this.clearSelection, this),
        ];
        this.$element.on("click", this.stopPropagation);
    }

    public $onDestroy() {
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this.$element.off("click", this.stopPropagation);
    }

    public $onChanges(changesObj) {
        if (changesObj.context) {
            this._context = changesObj.context.currentValue;

            if (this._context) {
                this.isLoading = true;
                this.glossary = null;

                this.glossaryService.getGlossary(this._context).then((result: IGlossaryDetails) => {
                    result.terms = result.terms.map((term: IGlossaryTerm) => {
                        term.definition = this.$sce.trustAsHtml(term.definition); 
                        return term;
                    });

                    this.glossary = result;

                }).finally(() => {
                    this.isLoading = false;
                });
            }
        }
    }

    private clearSelection() {
        if (this.glossary) {
            this.glossary.terms = this.glossary.terms.map((t: IGlossaryTerm) => {
                t.selected = false;
                return t;
            });
        }
    }

    public selectTerm(term: IGlossaryTerm) {
        if (term.selected) {
            return;
        }
        this.glossary.terms = this.glossary.terms.map((t: IGlossaryTerm) => {
            t.selected = t === term;
            return t;
        });
        const oldSelection = this.selectionManager.selection;
        const selection: ISelection = {
            source: SelectionSource.Editor,
            artifact: oldSelection.artifact,
            subArtifact: {
                id: term.id,
                name: term.name,
                predefinedType: ItemTypePredefined.Term,
                prefix: "TR"
            }
        };
        this.selectionManager.selection = selection;
    }
    private stopPropagation(event: JQueryEventObject) {
        if (event.target.tagName !== "TH") {
            event.stopPropagation();
        }
    }
}
