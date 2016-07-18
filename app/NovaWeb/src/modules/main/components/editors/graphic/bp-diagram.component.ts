import "angular";
import "angular-sanitize";
import {IStencilService} from "./impl/stencil.svc";
import {IDiagram} from "./impl/models";
import {IDiagramService} from "./diagram.svc";
import {DiagramView} from "./impl/diagram-view";
import {IProjectManager, Models} from "../../../../main";
import {ILocalizationService } from "../../../../core";
import {IMessageService} from "../../../../shell";
import {SafaryGestureHelper} from "./impl/utils/gesture-helper";

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
        "localization",
        "messageService"
    ];

    public isLoading: boolean = true;

    private subscribers: Rx.IDisposable[];
    private diagramView: DiagramView;
    private cancelationToken: ng.IDeferred<any>;

    constructor(
        private $element: ng.IAugmentedJQuery,
        private $q: ng.IQService,
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
                if (diagram.libraryVersion === 0) {
                    const message = this.localization.get("Diagram_OldFormat_Message");
                    this.messageService.addError(message);
                } else {
                    if (this.diagramView) {
                        this.diagramView.destroy();
                        this.$element.css("width", "");
                        this.$element.css("overflow", "");
                    }
                    this.diagramView = new DiagramView(this.$element[0], this.stencilService);
                    this.diagramView.sanitize = this.$sanitize;
                    this.stylizeSvg(this.$element, diagram.width, diagram.height);
                    this.diagramView.drawDiagram(diagram);
                }
            }).finally(() => {
                this.cancelationToken = null;
                this.isLoading = false;
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
        $element.css("background-color", "");
    }
}
