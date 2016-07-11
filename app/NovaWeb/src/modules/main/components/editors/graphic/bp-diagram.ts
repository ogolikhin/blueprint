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

    private _subscribers: Rx.IDisposable[];
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

        this._subscribers = [ selectedArtifactSubscriber ];

        this.diagramView = new DiagramView(this.$element[0], this.stencilService);
        this.diagramView.sanitize = this.$sanitize;
    }

    public $onDestroy() {
        if (this.diagramView != null) {
            this.diagramView.destroy();
        }
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }
    
    private setArtifactId = (artifact: Models.IArtifact) => {
        if (artifact !== null && this.isDiagram(artifact)) {
            this.diagramService.getDiagram(artifact.id).then((diagram) => {
                this.diagramView.drawDiagram(diagram);
            });
        }
    }

    private isDiagram(artifact: Models.IArtifact): boolean {
        switch (artifact.predefinedType) {
            case <Models.ArtifactTypeEnum>4108:
            case <Models.ArtifactTypeEnum>4112:
            case Models.ArtifactTypeEnum.GenericDiagram:
            case Models.ArtifactTypeEnum.UseCaseDiagram:
            case Models.ArtifactTypeEnum.Storyboard:
            case Models.ArtifactTypeEnum.UseCase:
                return true;
            default:
                return false;
        }
    }
}
