import {IWindowManager} from "../../services";
import {IArtifactManager} from "../../../managers";
import {IMessageService, INavigationService} from "../../../core";
import {IDiagramService} from "../../../editors/bp-diagram/diagram.svc";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./bp-page-content.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = PageContentCtrl;
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

    constructor(private messageService: IMessageService,
                private artifactManager: IArtifactManager,
                private diagramService: IDiagramService,
                private windowManager: IWindowManager,
                private navigationService: INavigationService) {
    }

    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.windowManager.mainWindow.subscribeOnNext(this.onAvailableAreaResized, this)
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
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
