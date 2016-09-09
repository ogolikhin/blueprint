import { IGlossaryService, IGlossaryTerm } from "./glossary.svc";
import { ILocalizationService, IMessageService, IStateManager } from "../../core";
import { ISelectionManager, ISelection, SelectionSource } from "../../main/services/selection-manager";
import { IEditorContext, IArtifact } from "../../main/models/models";
import { BpBaseEditor} from "../bp-base-editor";

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
        "stateManager"
    ];

    private _context: IEditorContext;
    private subscribers: Rx.IDisposable[];

    public glossary: IArtifact;
    public isLoading: boolean = true; 

    constructor(
        private $element: ng.IAugmentedJQuery,
        private $log: ng.ILogService,
        private localization: ILocalizationService, 
        private glossaryService: IGlossaryService,
        private selectionManager: ISelectionManager,
        private $sce: ng.ISCEService,
        messageService: IMessageService,
        stateManager: IStateManager) {
            super(messageService, stateManager);
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
            this._context = <IEditorContext>changesObj.context.currentValue;

            if (this._context && this._context.artifact) {
                this.isLoading = true;
                this.glossary = null;

                this.glossaryService.getGlossary(this._context.artifact.id).then((result: IArtifact) => {
                    result.subArtifacts = result.subArtifacts.map((term: IGlossaryTerm) => {
                        term.description = this.$sce.trustAsHtml(term.description);
                        return term;
                    });
                    this.stateManager.addChange(result);

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
        }
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
