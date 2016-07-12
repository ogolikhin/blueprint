import { IProjectManager, Models} from "../..";
import { IMessageService, Message } from "../../../shell";


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
    public static $inject: [string] = ["messageService", "projectManager"];
    constructor(private messageService: IMessageService, private projectManager: IProjectManager) {
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
            project : this.projectManager.currentProject.getValue()
        } 
    }

    private getContentType(artifact: Models.IArtifact): string {
        switch (artifact.predefinedType) {
            case Models.ItemTypePredefined.DomainDiagram:
            case Models.ItemTypePredefined.GenericDiagram:
                return "graphic";
            default:
                return "other";
        }
    }
}