import {IProjectManager, Models} from "../..";
import {IMessageService} from "../../../core";
import {IDiagramService} from "../../../editors/bp-diagram/diagram.svc";
import {ItemTypePredefined} from "../../models/enums";


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
    public static $inject: [string] = ["$state", "messageService", "projectManager", "diagramService"];
    constructor(private $state: any,
                private messageService: IMessageService,
                private projectManager: IProjectManager,
                private diagramService: IDiagramService) {
    }
    //TODO remove after testing
    public addMsg() {
        //temporary removed to toolbar component under "Refresh" button
    }

    public context: any = null;
        
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

        } catch (ex) {
            this.messageService.addError(ex.message);
        }
        this.context = _context;
    }
    
}