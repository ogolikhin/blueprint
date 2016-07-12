import "angular";
import "angular-sanitize";
import {IStencilService} from "./impl/stencil.svc";
import {IDiagramService} from "./diagram.svc";
import {DiagramView} from "./impl/diagram-view";
import {IProjectManager, Models} from "../../../../main";

export class BPDiagram implements ng.IComponentOptions {
    public controller: Function = BPDiagramController;
}

export class BPDiagramController {
    public static $inject: [string] = [
        "$element",
        "$sanitize",
        "stencilService", 
        "diagramService",
        "projectManager"
    ];

    private subscribers: Rx.IDisposable[];
    private diagramView: DiagramView;

    constructor(
        private $element: ng.IAugmentedJQuery,
        private $sanitize: any,
        private stencilService: IStencilService,
        private diagramService: IDiagramService,
        private projectManager: IProjectManager) {
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
            this.diagramView = new DiagramView(this.$element[0], this.stencilService);
            this.diagramView.sanitize = this.$sanitize;

            this.diagramService.getDiagram(artifact.id, artifact.predefinedType).then(diagram => {
                    this.diagramView.drawDiagram(diagram);
                });
        }
    }
}
