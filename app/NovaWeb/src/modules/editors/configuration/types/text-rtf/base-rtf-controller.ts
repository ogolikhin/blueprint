import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";

export interface IBPFieldBaseRTFController {
    editorBody: HTMLElement;
    observer: MutationObserver;
    handleClick(event: Event): void;
    handleLinks(nodeList: Node[] | NodeList, remove: boolean): void;
    handleMutation(mutation: MutationRecord): void;
}

export class BPFieldBaseRTFController implements IBPFieldBaseRTFController {
    static $inject: [string] = ["$scope", "navigationService", "validationService"];

    public editorBody: HTMLElement;
    public observer: MutationObserver;

    protected contentBuffer: string;
    protected mceEditor: TinyMceEditor;
    protected onChange: AngularFormly.IExpressionFunction;

    constructor(protected $scope: AngularFormly.ITemplateScope,
                protected navigationService: INavigationService,
                protected validationService: IValidationService) {
        this.contentBuffer = undefined;

        // the onChange event has to be called from the custom validator (!) as otherwise it will fire before the actual validation takes place
        this.onChange = ($scope.to.onChange as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.
        //we override the default onChange as we need to deal with changes differently when using tinymce
        $scope.to.onChange = undefined;

        $scope["$on"]("$destroy", () => {
            this.removeObserver();
            if (this.editorBody) {
                this.handleLinks(this.editorBody.querySelectorAll("a"), true);
            }
        });
    }

    private removeObserver = () => {
        if (this.observer) {
            this.observer.disconnect();
        }
    };

    protected fontFormats(allowedFonts: string[]): string {
        let fontFormats = "";
        if (_.isArray(allowedFonts) && allowedFonts.length) {
            allowedFonts.forEach(function (font) {
                fontFormats += `${font}=` + (font.indexOf(" ") !== -1 ? `"${font}";` : `${font};`);
            });
        }
        return fontFormats;
    }

    protected triggerChange = (newContent: string) => {
        const $scope = this.$scope;
        const isValid = this.validationService.textRtfValidation.hasValueIfRequired($scope.to.required, newContent, newContent);

        if ($scope.fc) {
            const fc = $scope.fc as ng.IFormController;
            fc.$setValidity("requiredCustom", isValid, fc);
        }

        this.contentBuffer = newContent;
        if (typeof this.onChange === "function") {
            this.onChange(newContent, $scope.options, $scope);
        }
    };

    public handleClick = (event: Event) => {
        const navigationService = this.navigationService;
        const target = event.currentTarget as HTMLElement;

        event.stopPropagation();
        event.preventDefault();
        const itemId = Number(target.getAttribute("subartifactid")) || Number(target.getAttribute("artifactid"));
        if (itemId) {
            navigationService.navigateTo({id: itemId});
        } else {
            window.open(target.getAttribute("href"), "_blank");
        }
    };

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
                element.addEventListener("click", this.handleClick);
            } else {
                if (document.body.classList.contains("is-msie")) {
                    element.removeEventListener("mouseover", this.disableEditability);
                    element.removeEventListener("mouseout", this.enableEditability);
                }
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
}
