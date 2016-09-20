import { IWindowManager } from "../../services";
import { IArtifactManager, SelectionSource } from "../../../managers";
import { IStatefulArtifact } from "../../../managers/models";
import { IMessageService, INavigationService } from "../../../core";
import { IDiagramService } from "../../../editors/bp-diagram/diagram.svc";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");
    public controller: Function = PageContentCtrl;
    public controllerAs = "$content";
} 

class PageContentCtrl {
    private subscribers: Rx.IDisposable[];

    public static $inject: [string] = [
        "messageService",
        "artifactManager",
        "diagramService",
        "windowManager",
        "navigationService"
    ];

    constructor(
        private messageService: IMessageService,
        private artifactManager: IArtifactManager,
        private diagramService: IDiagramService,
        private windowManager: IWindowManager,
        private navigationService: INavigationService
    ) {
    }

    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.artifactManager.selection.artifactObservable.filter(this.selectedInExplorer).subscribeOnNext(this.selectContext, this),
            this.windowManager.mainWindow.subscribeOnNext(this.onAvailableAreaResized, this)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private selectContext(artifact: IStatefulArtifact) {
        if (!artifact) {
            this.navigationService.navigateToMain();
            return;
        }

        this.navigationService.navigateToArtifact(artifact.id);
    }

    private selectedInExplorer = (artifact: IStatefulArtifact) => {
        const selectedInExplorer = this.artifactManager.selection.getArtifact(SelectionSource.Explorer);
        return selectedInExplorer && artifact && selectedInExplorer.id === artifact.id;
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
