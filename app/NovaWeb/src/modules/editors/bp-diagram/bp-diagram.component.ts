import "angular";
import "angular-sanitize";
import { IStencilService } from "./impl/stencil.svc";
import { IDiagramService, CancelationTokenConstant } from "./diagram.svc";
import { DiagramView } from "./impl/diagram-view";
import { IProjectManager, Models } from "../../main";
import { ISelectionManager, SelectionSource } from "../../main/services/selection-manager";
import { IDiagramElement } from "./impl/models";
import { ILocalizationService } from "../../core";
import { SafaryGestureHelper } from "./impl/utils/gesture-helper";

export class BPDiagram implements ng.IComponentOptions {
    public template: string = require("./bp-diagram.html");
    public controller: Function = BPDiagramController;
}

export class BPDiagramController {
    public static $inject: [string] = [
        "$element",
        "$q",
        "$sanitize",
        "stencilService", 
        "diagramService",
        "projectManager",
        "selectionManager",
        "localization",
        "$log"
    ];

    public isLoading: boolean = true;

    private subscribers: Rx.IDisposable[];
    private diagramView: DiagramView;
    private cancelationToken: ng.IDeferred<any>;
    public isBrokenOrOld: boolean = false;
    public errorMsg: string;

    constructor(
        private $element: ng.IAugmentedJQuery,
        private $q: ng.IQService,
        private $sanitize: any,
        private stencilService: IStencilService,
        private diagramService: IDiagramService,
        private projectManager: IProjectManager,
        private selectionManager: ISelectionManager,
        private localization: ILocalizationService,
        private $log: ng.ILogService) {
            new SafaryGestureHelper().disableGestureSupport(this.$element);
    }

        //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
        const selectedArtifactSubscriber: Rx.IDisposable = this.projectManager.currentArtifact.subscribe(this.setArtifactId);

        this.subscribers = [ selectedArtifactSubscriber ];
    }

    public $onDestroy() {
        if (this.diagramView) {
            this.diagramView.destroy();
        }
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }
    
    private setArtifactId = (artifact: Models.IArtifact) => {
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
                    this.diagramView.addSelectionListener(this.onSelectionChanged);
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

    private onSelectionChanged = (elements: Array<IDiagramElement>) => {
        const selection = angular.copy(this.selectionManager.selection);
        if (elements && elements.length > 0) {
            selection.subArtifact = elements[0];
        } else {
            selection.subArtifact = null;
        }
        selection.source = SelectionSource.Editor;
        this.selectionManager.selection = selection;
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
}
