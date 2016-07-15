import "angular";
import "angular-sanitize";
import {IStencilService} from "./impl/stencil.svc";
import {IDiagramService} from "./diagram.svc";
import {DiagramView} from "./impl/diagram-view";
import {IProjectManager, Models} from "../../../../main";
import {ILocalizationService } from "../../../../core";
import {IMessageService} from "../../../../shell";
import {SafaryGestureHelper} from "./impl/utils/gesture-helper";

export class BPDiagram implements ng.IComponentOptions {
    public controller: Function = BPDiagramController;
}

export class BPDiagramController {
    public static $inject: [string] = [
        "$element",
        "$sanitize",
        "stencilService", 
        "diagramService",
        "projectManager",
        "localization",
        "messageService"
    ];

    private subscribers: Rx.IDisposable[];
    private diagramView: DiagramView;

    constructor(
        private $element: ng.IAugmentedJQuery,
        private $sanitize: any,
        private stencilService: IStencilService,
        private diagramService: IDiagramService,
        private projectManager: IProjectManager,
        private localization: ILocalizationService,
        private messageService: IMessageService) {
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
        if (this.diagramView) {
            this.diagramView.destroy();
        }

        if (artifact !== null && this.diagramService.isDiagram(artifact.predefinedType)) {
            this.diagramService.getDiagram(artifact.id, artifact.predefinedType).then(diagram => {
                if (diagram.libraryVersion === 0) {
                    const message = this.localization.get("Diagram_OldFormat_Message");
                    this.messageService.addError(message);
                } else {
                    this.diagramView = new DiagramView(this.$element[0], this.stencilService);
                    this.diagramView.sanitize = this.$sanitize;
                    this.stylizeSvg(this.$element, diagram.width, diagram.height);
                    this.diagramView.drawDiagram(diagram);
                }
            });
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
    }
}
