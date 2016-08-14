import { IProjectManager, Models, ISidebarToggle } from "../..";
import { ISelectionManager, SelectionSource } from "./../../services/selection-manager";
import { IMessageService } from "../../../core";
import { IDiagramService } from "../../../editors/bp-diagram/diagram.svc";
import { IEditorContext } from "../../models/models";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./pagecontent.html");

    public controller: Function = PageContentCtrl;
    public controllerAs = "$content";
    public bindings: any = {
        viewState: "<"
    };
} 

class PageContentCtrl {
    private subscribers: Rx.IDisposable[];
    public static $inject: [string] = ["$state", "messageService", "projectManager", "diagramService", "selectionManager", "sidebarToggle"];
    constructor(private $state: ng.ui.IStateService,
                private messageService: IMessageService,
                private projectManager: IProjectManager,
                private diagramService: IDiagramService,
                private selectionManager: ISelectionManager,
                private sidebarToggle: ISidebarToggle) {
    }
    public context: IEditorContext = null;

    public viewState: boolean;

    public scrollOptions = {
        minScrollbarLength: 20,
        scrollXMarginOffset: 4,
        scrollYMarginOffset: 4
    };

    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.getSelectedArtifactObservable().subscribeOnNext(this.selectContext, this),
            this.sidebarToggle.getAvailableArea.subscribeOnNext(this.onAvailableAreaResized, this)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private selectContext(artifact: Models.IArtifact) {
        let _context: IEditorContext = {};
        try {
            if (!artifact) {
                this.$state.go("main");
                return;
            }

            _context.artifact = artifact;
            _context.type = this.projectManager.getArtifactType(_context.artifact);
            this.$state.go("main.artifact", { id: artifact.id });

        } catch (ex) {
            this.messageService.addError(ex.message);
        }
        this.context = _context;
    }

    private getSelectedArtifactObservable() {
        return this.selectionManager.selectionObservable
            .filter(s => s != null && s.source === SelectionSource.Explorer)
            .map(s => s.artifact)
            .distinctUntilChanged(a => a ? a.id : -1).asObservable();
    }

    public onContentSelected($event: MouseEvent) {
        if ($event.target && $event.target["tagName"] !== "BUTTON") {
            if (this.context) {
                this.selectionManager.selection = { artifact: this.context.artifact, source: SelectionSource.Editor };
            } else {
                this.selectionManager.clearSelection();
            }
        }
    }

    private onAvailableAreaResized() {
        let scrollableElem = document.querySelector(".page-body-wrapper.ps-container") as HTMLElement;
        if (scrollableElem) {
            setTimeout(() => {
                (<any>window).PerfectScrollbar.update(scrollableElem);
            }, 500);
        }
    }
}
