import "angular";
import "angular-sanitize";
import { IStencilService } from "./impl/stencil.svc";
import { ILocalizationService, IStateManager, IMessageService } from "../../core";
import { Models } from "../../main";
import { IDiagramService, CancelationTokenConstant } from "./diagram.svc";
import { DiagramView } from "./impl/diagram-view";
import { ISelectionManager, ISelection } from "../../main/services/selection-manager";
import { IDiagramElement } from "./impl/models";
import { SafaryGestureHelper } from "./impl/utils/gesture-helper";
import { SelectionHelper } from "./impl/utils/selection-helper";
import { BpBaseEditor} from "../bp-base-editor";



export class BPDiagram implements ng.IComponentOptions {
    public template: string = require("./bp-diagram.html");
    public controller: Function = BPDiagramController;
    public bindings: any = {
        context: "<"
    };
}

export class BPDiagramController extends BpBaseEditor{

    public static $inject: [string] = [
        "messageService", 
        "stateManager", 
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
    private artifact: Models.IArtifact;

    constructor(
        public messageService: IMessageService,
        public stateManager: IStateManager,
        private $element: ng.IAugmentedJQuery,
        private $q: ng.IQService,
        private $sanitize: any,
        private stencilService: IStencilService,
        private diagramService: IDiagramService,
        private selectionManager: ISelectionManager,
        private localization: ILocalizationService,
        private $rootScope: ng.IRootScopeService,
        private $log: ng.ILogService) {
            super(messageService, stateManager);
            new SafaryGestureHelper().disableGestureSupport(this.$element);
    }

    public $onInit() {
        super.$onInit();

        //use context reference as the last parameter on subscribe...
        this.subscribers.push(
            //subscribe for current artifact change (need to distinct artifact)
            this.selectionManager.selectionObservable
                .filter(this.clearSelectionFilter)
                .subscribeOnNext(this.clearSelection, this)
        );
        this.$element.on("click", this.stopPropagation);
    }

    private clearSelectionFilter = (selection: ISelection) => {
        return selection != null
               && selection.artifact
               && selection.artifact.id === this.artifact.id
               && !selection.subArtifact;
    }



    public $onDestroy() {
        super.$onDestroy();
        this.$element.off("click", this.stopPropagation);
        if (this.diagramView) {
            this.diagramView.destroy();
        }
    }

    public onLoading(obj: any): boolean {
        this.isLoading = true;
        this.artifact = (obj.context.currentValue as Models.IEditorContext).artifact;
        return !!this.artifact;
    }

    public onUpdate(context: Models.IEditorContext) {
        this.$element.css("height", "100%");
        this.$element.css("width", "");
        this.$element.css("background-color", "transparent");
        if (this.diagramView) {
            this.diagramView.destroy();
        }
        if (this.cancelationToken) {
           this.cancelationToken.resolve();
        }
        if (this.artifact !== null && this.diagramService.isDiagram(this.artifact.predefinedType)) {
            this.cancelationToken = this.$q.defer();
            this.diagramService.getDiagram(this.artifact.id, this.artifact.predefinedType, this.cancelationToken.promise).then(diagram => {
                
                this.stateManager.addChange(diagram.data);

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
                    let viewContainer = this.getViewContainer(this.$element);
                    this.diagramView = new DiagramView(viewContainer, this.stencilService);
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


    // private onArtifactChanged = () => {
    // }

    private getViewContainer($element: ng.IAugmentedJQuery): HTMLElement {
        let htmlElement = null;
        let childElements = this.$element.find("div");

        for (let i = 0; i < childElements.length; i++) {
            if (childElements[i].className.match(/diagram-container/)) {
                htmlElement = childElements[i];
                break;
            }
        }

        return htmlElement;
    }

    private onSelectionChanged = (diagramType: string, elements: Array<IDiagramElement>) => {
        this.$rootScope.$applyAsync(() => {
            const selectionHelper = new SelectionHelper();
            this.selectionManager.selection = selectionHelper.getEffectiveSelection(
                this.artifact,
                elements,
                diagramType);
        });
    }

    private clearSelection(selection: ISelection) {
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
