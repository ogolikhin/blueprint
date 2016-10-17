import * as angular from "angular";

export interface IBPFieldBaseRTFController {
    observer: MutationObserver;
    handleClick(event: Event): void;
    handleLinks(nodeList: Node[] | NodeList, remove: boolean): void;
    handleMutation(mutation: MutationRecord): void;
    removeObserver(): void;
}

export class BPFieldBaseRTFController implements IBPFieldBaseRTFController {
    public observer: MutationObserver;

    constructor() {
//fixme: empty constructors can be omitted
    }

    public handleClick = function (event) {
        event.stopPropagation();
        event.preventDefault();

        const href = this.href;
        if (href.indexOf("?ArtifactId=") !== -1 && this.getAttribute("artifactid")) {
            const artifactId = parseInt(href.split("?ArtifactId=")[1], 10);
            if (artifactId === parseInt(this.getAttribute("artifactid"), 10)) {
                console.log("Should GOTO " + artifactId);
            }
        } else {
            window.open(href, "_blank");
        }
    };

    public handleLinks = (nodeList: Node[] | NodeList, remove: boolean = false) => {
        if (nodeList.length === 0) {
            return;
        }
        for (let i = 0; i < nodeList.length; i++) {
            let element = nodeList[i] as HTMLElement;

            if (!remove) {
                angular.element(element).attr("contentEditable", "false");
                angular.element(element).attr("data-mce-contenteditable", "false");

                element.addEventListener("click", this.handleClick);
            } else {
                element.removeEventListener("click", this.handleClick);
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

    public removeObserver = () => {
        if (this.observer) {
            this.observer.disconnect();
        }
    };
}
