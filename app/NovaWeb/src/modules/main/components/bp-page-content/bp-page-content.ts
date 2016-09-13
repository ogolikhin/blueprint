import { Models} from "../../models";
import { IWindowManager } from "../../services";
import { IProjectManager, ISelectionManager, SelectionSource} from "../../../managers";
import { IStatefulArtifact } from "../../../managers/models";

import { IMessageService, IStateManager } from "../../../core";
import { IDiagramService } from "../../../editors/bp-diagram/diagram.svc";
import { IEditorContext } from "../../models/models";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");

    public controller: Function = PageContentCtrl;
    public controllerAs = "$content";
    public bindings: any = {
        viewState: "<"
    };
} 

class PageContentCtrl {
    private subscribers: Rx.IDisposable[];
    public static $inject: [string] = [
        "$state",
        "messageService",
        "projectManager",
        "diagramService",
        "selectionManager2",
        "stateManager",
        "windowManager"];
    constructor(private $state: ng.ui.IStateService,
                private messageService: IMessageService,
                private projectManager: IProjectManager,
                private diagramService: IDiagramService,
                private selectionManager: ISelectionManager,
                private stateManager: IStateManager,
                private windowManager: IWindowManager) {
    }
    public context: IEditorContext = null;

    public viewState: boolean;

    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.selectionManager.artifactObservable.subscribeOnNext(this.selectContext, this),
            this.windowManager.mainWindow.subscribeOnNext(this.onAvailableAreaResized, this)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private selectContext(artifact: IStatefulArtifact) {
        let _context: IEditorContext = {};
        try {
            if (!artifact) {
                this.$state.go("main");
                return;
            }

            // _context.artifact = artifact;
//             _context.type = this.projectManager.getArtifactType(artifact.id);
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
        // if ($event.target && $event.target["tagName"] !== "BUTTON") {
        //     if (this.context) {
        //         this.selectionManager.selection = { artifact: this.context.artifact, source: SelectionSource.Editor };
        //     } else {
        //         this.selectionManager.clearSelection();
        //     }
        // }
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
