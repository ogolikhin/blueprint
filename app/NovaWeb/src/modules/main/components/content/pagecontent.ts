import {IProjectManager, Models} from "../..";
import {IMessageService} from "../../../shell";
import {IDiagramService} from "../editors/graphic/diagram.svc";


export class PageContent implements ng.IComponentOptions {
    public template: string = require("./pagecontent.html");

    public controller: Function = PageContentCtrl;
    public controllerAs = "$content";
    public bindings: any = {
        viewState: "<",
    };

} 

class PageContentCtrl {
    private subscribers: Rx.IDisposable[];
    public static $inject: [string] = ["messageService", "projectManager", "diagramService"];
    constructor(private messageService: IMessageService,
                private projectManager: IProjectManager,
                private diagramService: IDiagramService) {
    }
    //TODO remove after testing
    public addMsg() {
        //temporary removed to toolbar component under "Refresh" button
    }

    public context: any = null;

    public contentType: string = "details";
    
    public viewState: boolean;

    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.projectManager.currentArtifact.subscribeOnNext(this.selectContext, this),
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private selectContext(artifact: Models.IArtifact) {
        let _context: any = {};
        try {
            if (!artifact) {
                return;
            }

            _context.artifact = artifact;
            _context.project = this.projectManager.currentProject.getValue();
            _context.type = this.projectManager.getArtifactType(_context.artifact, _context.project);
            _context.propertyTypes = this.projectManager.getArtifactPropertyTypes(_context.artifact);
            this.contentType = this.getContentType(artifact);

        } catch (ex) {
            this.messageService.addError(ex.message);
        }
        this.context = _context;
    }

    private getContentType(artifact: Models.IArtifact): string {
        if (this.diagramService.isDiagram(artifact.predefinedType)) {
            return "diagram";
        } else if (Models.ItemTypePredefined.Project == artifact.predefinedType) {
            return "project";
        } else if (Models.ItemTypePredefined.CollectionFolder == artifact.predefinedType) {
            return "collection";
        }
        return "general";
    }
}