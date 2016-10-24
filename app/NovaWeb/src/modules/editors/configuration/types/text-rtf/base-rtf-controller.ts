import { INavigationService } from "../../../../core/navigation";

export interface IBPFieldBaseRTFController {
    editorBody: HTMLElement;
    observer: MutationObserver;
    handleLinks(nodeList: Node[] | NodeList, remove: boolean): void;
    handleMutation(mutation: MutationRecord): void;
    removeObserver(): void;
}

export class BPFieldBaseRTFController implements IBPFieldBaseRTFController {
    constructor( private navigationService: INavigationService ) {

    }

    public editorBody: HTMLElement;
    public observer: MutationObserver;

    private getHandleClick = () => {
         const navigationService = this.navigationService;
         return function (event) {
            event.stopPropagation();
            event.preventDefault();
            const itemId = Number(this.getAttribute("subartifactid")) || Number(this.getAttribute("artifactid"));
            if (itemId) {
                 navigationService.navigateTo(itemId);
            } else {
                window.open(this.href, "_blank");
            }
        };
    }

    public handleLinks = (nodeList: Node[] | NodeList, remove: boolean = false) => {
        if (nodeList.length === 0) {
            return;
        }
        for (let i = 0; i < nodeList.length; i++) {
            let element = nodeList[i] as HTMLElement;

            if (!remove) {
                // IE doesn't show the pointer cursor over links nested in a element with contenteditable=true
                // We need to remove and add back that attribute on mouseover/out!!
                if (document.body.classList.contains("is-msie")) {
                    element.addEventListener("mouseover", this.disableEditability);
                    element.addEventListener("mouseout", this.enableEditability);
                }
                element.addEventListener("click", this.getHandleClick());
            } else {
                if (document.body.classList.contains("is-msie")) {
                    element.removeEventListener("mouseover", this.disableEditability);
                    element.removeEventListener("mouseout", this.enableEditability);
                }
                element.removeEventListener("click", this.getHandleClick());
            }
        }
    };

    public handleMutation = (mutation: MutationRecord) => {
        let addedNodes = mutation.addedNodes;
        let removedNodes = mutation.removedNodes;
        if (addedNodes) {
            for (let i = 0; i < addedNodes.length; i++) {
                let node = addedNodes[i];
                if (node.nodeType === 1) { // ELEMENT_NODE
                    if (node.nodeName.toUpperCase() === "A") {
                        this.handleLinks([node]);
                    } else {
                        let element = node as HTMLElement;
                        this.handleLinks(element.querySelectorAll("a"));
                    }
                }
            }
        }
        if (removedNodes) {
            for (let i = 0; i < removedNodes.length; i++) {
                let node = removedNodes[i];
                if (node.nodeType === 1) { // ELEMENT_NODE
                    if (node.nodeName.toUpperCase() === "A") {
                        this.handleLinks([node], true);
                    } else {
                        let element = node as HTMLElement;
                        this.handleLinks(element.querySelectorAll("a"), true);
                    }
                }
            }
        }
    };

    public disableEditability = (e) => {
        if (this.editorBody) {
            this.editorBody.setAttribute("contentEditable", "false");
        }
    };

    public enableEditability = (e) => {
        if (this.editorBody) {
            this.editorBody.setAttribute("contentEditable", "true");
        }
    };

    public removeObserver = () => {
        if (this.observer) {
            this.observer.disconnect();
        }
    };
}
