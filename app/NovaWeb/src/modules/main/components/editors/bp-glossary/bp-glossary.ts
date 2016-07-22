// import { Models } from "../../../";
import { IGlossaryDetals, IGlossaryService, IGlossaryTerm } from "./glossary.svc";
import { ILocalizationService } from "../../../../core";

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
        "$sce"
    ];

    private _context: number; // Models.IArtifact;

    public glossary: IGlossaryDetals;

    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService, 
        private glossaryService: IGlossaryService,
        private $sce: ng.ISCEService) {
    }

    public $onChanges(changesObj) {
        if (changesObj.context) {
            this._context = changesObj.context.currentValue;

            if (this._context) {
                this.glossaryService.getGlossary(this._context).then((result: IGlossaryDetals) => {
                    result.terms = result.terms.map((term: IGlossaryTerm) => {
                        term.definition = this.$sce.trustAsHtml(term.definition); 
                        return term;
                    });

                    this.glossary = result;
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
    }
}
