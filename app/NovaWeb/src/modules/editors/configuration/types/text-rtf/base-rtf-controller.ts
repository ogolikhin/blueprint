import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {Helper} from "../../../../shared/utils/helper";
import {IDialogSettings, IDialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {
    ArtifactPickerDialogController,
    IArtifactPickerOptions
} from "../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog";
import {Models} from "../../../../main/models";

export interface IBPFieldBaseRTFController {
    editorBody: HTMLElement;
    observer: MutationObserver;
    handleClick(event: Event): void;
    handleLinks(nodeList: Node[] | NodeList, remove: boolean): void;
    handleMutation(mutation: MutationRecord): void;
}

export class BPFieldBaseRTFController implements IBPFieldBaseRTFController {
    static $inject: [string] = [
        "$scope",
        "navigationService",
        "validationService",
        "localization",
        "dialogService"
    ];

    public editorBody: HTMLElement;
    public observer: MutationObserver;

    protected contentBuffer: string;
    protected mceEditor: TinyMceEditor;
    protected onChange: AngularFormly.IExpressionFunction;
    protected allowedFonts: string[];

    constructor(public $scope: AngularFormly.ITemplateScope,
                public navigationService: INavigationService,
                public validationService: IValidationService,
                public localization: ILocalizationService,
                public dialogService: IDialogService) {
        this.contentBuffer = undefined;

        // the onChange event has to be called from the custom validator (!) as otherwise it will fire before the actual validation takes place
        this.onChange = ($scope.to.onChange as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.
        //we override the default onChange as we need to deal with changes differently when using tinymce
        $scope.to.onChange = undefined;

        $scope["$on"]("$destroy", () => {
            $scope.to.onChange = this.onChange;

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

    protected handleValidation = () => {
        const $scope = this.$scope;
        $scope["$applyAsync"](() => {
            const content = this.contentBuffer || "";
            const isValid = this.validationService.textRtfValidation.hasValueIfRequired($scope.to.required, content, content);
            const formControl = $scope.fc as ng.IFormController;
            if (formControl) {
                formControl.$setValidity("requiredCustom", isValid, formControl);
                $scope.to["isInvalid"] = !isValid;
            }
        });
    };

    protected fontFormats(): string {
        let fontFormats = "";
        if (_.isArray(this.allowedFonts) && this.allowedFonts.length) {
            this.allowedFonts.forEach(function (font) {
                fontFormats += `${font}=` + (font.indexOf(" ") !== -1 ? `"${font}";` : `${font};`);
            });
        }
        return fontFormats;
    }

    protected triggerChange = (newContent?: string) => {
        if (_.isUndefined(newContent) && this.mceEditor) {
            newContent = this.mceEditor.getContent();
        }
        this.contentBuffer = newContent || "";

        this.handleValidation();

        const $scope = this.$scope;
        if (typeof this.onChange === "function") {
            this.onChange(newContent, $scope.options, $scope);
        }
    };

    protected prepRTF = (hasTables: boolean = false) => {
        this.editorBody = this.mceEditor.getBody() as HTMLElement;
        this.normalizeHtml(this.editorBody, hasTables);
        this.contentBuffer = this.mceEditor.getContent();
        this.handleValidation();
        this.$scope.options["data"].isFresh = false;
    };

    protected normalizeHtml(body: Node, hasTables: boolean = false) {
        Helper.autoLinkURLText(body);
        if (hasTables) {
            Helper.addTableBorders(body);
        }
        if (_.isArray(this.allowedFonts) && this.allowedFonts.length) {
            Helper.setFontFamilyOrOpenSans(body, this.allowedFonts);
        }
    };

    public openArtifactPicker = () => {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Add"),
            template: require("../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get("Property_RTF_Add_InlineTrace")
        };

        const dialogOption: IArtifactPickerOptions = {
            showSubArtifacts: true
        };

        this.dialogService.open(dialogSettings, dialogOption).then((items: Models.IItem[]) => {
            if (items.length === 1) {
                const artifactId: number = items[0].id;
                const artifactName: string = items[0].name;
                const artifactPrefix: string = items[0].prefix;
                /* tslint:disable:max-line-length */
                const inlineTrace: string = `<a linkassemblyqualifiedname="BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, ` +
                    `BluePrintSys.RC.Client.SL.RichText, Version=7.4.0.0, Culture=neutral, PublicKeyToken=null" ` +
                    `canclick="True" isvalid="True" href="/?ArtifactId=${artifactId}" target="_blank" artifactid="${artifactId}">` +
                    `<span>${artifactPrefix}${artifactId}: ${artifactName}</span></a>`;
                /* tslint:enable:max-line-length */

                this.mceEditor["selection"].setContent(inlineTrace);
                this.triggerChange();
            }
        });
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
        const addedNodes = mutation.addedNodes;
        const removedNodes = mutation.removedNodes;
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
