import "angular";
import "angular-sanitize";
import { IStencilService } from "./impl/stencil.svc";
import { IDiagramService, CancelationTokenConstant } from "./diagram.svc";
import { DiagramView } from "./impl/diagram-view";
import { Models } from "../../main";
import { ISelectionManager } from "../../main/services/selection-manager";
import { IDiagramElement } from "./impl/models";
import { ILocalizationService } from "../../core";
import { SafaryGestureHelper } from "./impl/utils/gesture-helper";
import { SelectionHelper } from "./impl/utils/selection-helper";

export class BPDiagram implements ng.IComponentOptions {
    public template: string = require("./bp-diagram.html");
    public controller: Function = BPDiagramController;
    public bindings: any = {
        context: "<"
    };
}

export class BPDiagramController {
    public static $inject: [string] = [
        "$element",
        "$q",
        "$sanitize",
        "stencilService",
        "diagramService",
        "selectionManager",
        "localization",
        "$rootScope",
        "$log"
    ];

    public isLoading: boolean = true;
    public isBrokenOrOld: boolean = false;
    public errorMsg: string;

    private diagramView: DiagramView;
    private cancelationToken: ng.IDeferred<any>;
    private subscribers: Rx.IDisposable[];

    constructor(
        private $element: ng.IAugmentedJQuery,
        private $q: ng.IQService,
        private $sanitize: any,
        private stencilService: IStencilService,
        private diagramService: IDiagramService,
        private selectionManager: ISelectionManager,
        private localization: ILocalizationService,
        private $rootScope: ng.IRootScopeService,
        private $log: ng.ILogService) {
            new SafaryGestureHelper().disableGestureSupport(this.$element);
    }

    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.selectionManager.selectedSubArtifactObservable.filter(s => s == null).subscribeOnNext(this.clearSelection, this),
        ];
        this.$element.on("click", this.stopPropagation);
    }

    public $onChanges(changesObj) {
        if (changesObj.context) {
            const artifact = changesObj.context.currentValue as Models.IArtifact;
            if (artifact) {
                this.onArtifactChanged(artifact);
            }
        }
    }

    public $onDestroy() {
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this.$element.off("click", this.stopPropagation);
        if (this.diagramView) {
            this.diagramView.destroy();
        }
    }
    
    private onArtifactChanged = (artifact: Models.IArtifact) => {
        this.$element.css("height", "100%");
        this.$element.css("width", "");
        this.$element.css("background-color", "transparent");
        if (this.diagramView) {
            this.diagramView.destroy();
        }
        if (this.cancelationToken) {
           this.cancelationToken.resolve();
        }
        this.isLoading = true;
        if (artifact !== null && this.diagramService.isDiagram(artifact.predefinedType)) {
            this.cancelationToken = this.$q.defer();
            this.diagramService.getDiagram(artifact.id, artifact.predefinedType, this.cancelationToken.promise).then(diagram => {

                if (diagram.libraryVersion === 0 && diagram.shapes && diagram.shapes.length > 0) {
                    this.isBrokenOrOld = true;
                    this.errorMsg = this.localization.get("Diagram_OldFormat_Message");                 
                    this.$log.error("Old diagram, libraryVersion is 0");
                } else {
                    this.isBrokenOrOld = false;
                    if (this.diagramView) {
                        this.diagramView.destroy();
                        this.$element.css("width", "");
                        this.$element.css("overflow", "");
                    }
                    this.diagramView = new DiagramView(this.$element[0], this.stencilService);
                    this.diagramView.addSelectionListener((elements) => this.onSelectionChanged(diagram.diagramType, elements));
                    this.stylizeSvg(this.$element, diagram.width, diagram.height);
                    this.diagramView.drawDiagram(diagram);
                }

            }).catch((error: any) => {              
                if (error !== CancelationTokenConstant.cancelationToken) {
                    this.isBrokenOrOld = true;
                    this.errorMsg = error.message;
                    this.$log.error(error.message);
                }               
            }).finally(() => {
                this.cancelationToken = null;
                this.isLoading = false;
            });
        }
    }

    private onSelectionChanged = (diagramType: string, elements: Array<IDiagramElement>) => {
        this.$rootScope.$applyAsync(() => {
            const selectionHelper = new SelectionHelper();
                this.selectionManager.selection = selectionHelper.getEffectiveSelection(
                this.selectionManager.selection,
                elements,
                diagramType);
        });
    }

    private clearSelection() {
        if (this.diagramView) {
            this.diagramView.clearSelection();
        }
    }

    private stylizeSvg($element: ng.IAugmentedJQuery, width: number, height: number) {
        var w = width + "px";
        var h = height + "px";
        var svg = $element.find("svg");

        svg.css("width", w);
        svg.css("height", h);
        svg.css("min-width", w);
        svg.css("min-height", h);
        svg.css("max-width", w);
        svg.css("max-height", h);

        $element.css("width", w);
        $element.css("height", h);
        $element.css("overflow", "hidden");
        $element.css("background-color", "");
    }

    private stopPropagation(eventObject: JQueryEventObject) {
        eventObject.stopPropagation();
    }
}
