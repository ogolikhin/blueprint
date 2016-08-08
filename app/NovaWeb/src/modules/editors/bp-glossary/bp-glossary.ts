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
        "$log",
        "localization",
        "glossaryService",
        "selectionManager",
        "$sce"
    ];

    private _context: number; // Models.IArtifact;

    public glossary: IGlossaryDetails;
    public isLoading: boolean = true;

    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService, 
        private glossaryService: IGlossaryService,
        private selectionManager: ISelectionManager,
        private $sce: ng.ISCEService) {
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
}
