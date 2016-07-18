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
        if (!artifact) {
            return;
        }
        this.contentType = this.getContentType(artifact);

        this.context = {
            artifact: angular.copy(artifact),
            project: this.projectManager.currentProject.getValue(),
            propertyTypes: this.projectManager.getArtifactPropertyTypes(artifact)
        };
    }

    private getContentType(artifact: Models.IArtifact): string {
        if (this.diagramService.isDiagram(artifact.predefinedType)) {
            return "diagram";
        }
        return "other";
    }
}