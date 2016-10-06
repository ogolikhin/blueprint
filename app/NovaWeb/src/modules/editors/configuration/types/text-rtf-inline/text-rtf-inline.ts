import * as angular from "angular";
import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import { Helper } from "../../../../shared";

export class BPFieldTextRTFInline implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTFInline";
    public template: string = require("./text-rtf-inline.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
            ($scope["options"] as AngularFormly.IFieldConfigurationObject).validation.show = ($scope["fc"] as ng.IFormController).$invalid;

            let tinymceBody = $element[0].querySelector(".tinymce-body");
            if (tinymceBody) {
                $scope["tinymceBody"] = tinymceBody;
            }
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextRTFInlineController;

    constructor() {
    }
}

export class BpFieldTextRTFInlineController {
    static $inject: [string] = ["$scope"];

    private observer: MutationObserver;

    constructor(private $scope: AngularFormly.ITemplateScope) {
        let to: AngularFormly.ITemplateOptions = {
            tinymceOptions: { // this will go to ui-tinymce directive
                inline: true,
                fixed_toolbar_container: ".tinymce-toolbar-" + $scope.options["key"],
                menubar: false,
                toolbar: "fontselect fontsize | bold italic underline | forecolor format | link table",
                statusbar: false,
                invalid_elements: "img,frame,iframe,script",
                invalid_styles: {
                    "*": "background-image"
                },
                extended_valid_elements: "a[href|type|title|linkassemblyqualifiedname|text|canclick|isvalid|mentionid|isgroup|email|" +
                "class|linkfontsize|linkfontfamily|linkfontstyle|linkfontweight|linktextdecoration|linkforeground|style|target|artifactid]",
                fontsize_formats: "9pt",
                // https://www.tinymce.com/docs/configure/content-formatting/#font_formats
                font_formats: "Open Sans=Open Sans,Portable User Interface,sans-serif;" +
                "Arial=Arial,Helvetica,sans-serif;" +
                "Cambria=Cambria,Georgia,serif;" +
                "Calibri=Calibri,Candara,Segoe,Segoe UI,Optima,sans-serif;" +
                "Courier New=Courier New,courier,monospace;" +
                "Times New Roman=Times New Roman,Times,Baskerville,Georgia,serif;" +
                "Trebuchet MS=Trebuchet MS,Lucida Grande,Lucida Sans Unicode,Lucida Sans,Tahoma,sans-serif;" +
                "Verdana=Verdana,Geneva,sans-serif;",
                paste_webkit_styles: "none", // https://www.tinymce.com/docs/plugins/paste/#paste_webkit_styles
                paste_remove_styles_if_webkit: true, // https://www.tinymce.com/docs/plugins/paste/#paste_remove_styles_if_webkit
                // https://www.tinymce.com/docs/plugins/paste/#paste_retain_style_properties
                paste_retain_style_properties: "background background-color color " +
                "font font-family font-size font-style font-weight line-height " +
                "margin margin-bottom margin-left margin-right margin-top " +
                "padding padding-bottom padding-left padding-right padding-top " +
                "border border-collapse border-color border-style border-width" +
                "text-align text-decoration vertical-align" +
                "height width",
                table_toolbar: "", // https://www.tinymce.com/docs/plugins/table/#table_toolbar
                table_default_styles: { // https://www.tinymce.com/docs/plugins/table/#table_default_styles
                    background: "white",
                    borderColor: "#000",
                    borderCollapse: "collapse",
                    borderWidth: "1px"
                },
                table_default_attributes: { // https://www.tinymce.com/docs/plugins/table/#table_default_attributes
                    border: "1",
                    width: "95%"
                },
                // we don't need the autoresize plugin when using the inline version of tinyMCE as the height will
                // be controlled using CSS (max-height, min-height)
                plugins: "paste textcolor table noneditable autolink link",
                // plugins: "contextmenu", // https://www.tinymce.com/docs/plugins/contextmenu/
                // contextmenu: "bold italic underline strikethrough | link inserttable | cell row column deletetable",
                // paste_preprocess: function (plugin, args) { // https://www.tinymce.com/docs/plugins/paste/#paste_preprocess
                // },
                paste_postprocess: (plugin, args) => { // https://www.tinymce.com/docs/plugins/paste/#paste_postprocess
                    Helper.autoLinkURLText(args.node);
                },
                init_instance_callback: (editor) => {
                    editor.formatter.register("font8", {
                        inline: "span",
                        styles: { "font-size": "8pt" }
                    });
                    editor.formatter.register("font9", { // default font, equivalent to 12px
                        inline: "span",
                        styles: { "font-size": "9pt" }
                    });
                    editor.formatter.register("font10", {
                        inline: "span",
                        styles: { "font-size": "10pt" }
                    });
                    editor.formatter.register("font11", {
                        inline: "span",
                        styles: { "font-size": "11pt" }
                    });
                    editor.formatter.register("font12", {
                        inline: "span",
                        styles: { "font-size": "12pt" }
                    });
                    editor.formatter.register("font14", {
                        inline: "span",
                        styles: { "font-size": "14pt" }
                    });
                    editor.formatter.register("font16", {
                        inline: "span",
                        styles: { "font-size": "16pt" }
                    });
                    editor.formatter.register("font18", {
                        inline: "span",
                        styles: { "font-size": "18pt" }
                    });
                    editor.formatter.register("font20", {
                        inline: "span",
                        styles: { "font-size": "20pt" }
                    });

                    let editorBody = editor.getBody();
                    Helper.autoLinkURLText(editorBody);
                    this.handleLinks(editorBody.querySelectorAll("a"));

                    // MutationObserver
                    const MutationObserver = window["MutationObserver"] || window["WebKitMutationObserver"] || window["MozMutationObserver"];
                    if (!angular.isUndefined(MutationObserver)) {
                        // create an observer instance
                        this.observer = new MutationObserver((mutations) => {
                            mutations.forEach(this.handleMutation);
                        });

                        const observerConfig = { attributes: false, childList: true, characterData: false, subtree: true };
                        this.observer.observe(editor.getBody(), observerConfig);
                    }
                },
                setup: function (editor) {
                    editor.addButton("format", {
                        title: "Format",
                        type: "menubutton",
                        text: "",
                        icon: "format",
                        menu: [
                            { icon: "strikethrough", text: " Strikethrough", onclick: function () { tinymce.execCommand("strikethrough"); } },
                            { icon: "bullist", text: " Bulleted list", onclick: function () { tinymce.execCommand("InsertUnorderedList"); } },
                            { icon: "numlist", text: " Numeric list", onclick: function () { tinymce.execCommand("InsertOrderedList"); } },
                            { icon: "outdent", text: " Outdent", onclick: function () { tinymce.execCommand("Outdent"); } },
                            { icon: "indent", text: " Indent", onclick: function () { tinymce.execCommand("Indent"); } },
                            { text: "-" },
                            { icon: "removeformat", text: " Clear formatting", onclick: function () { tinymce.execCommand("RemoveFormat"); } }
                        ]
                    });
                    editor.addButton("fontsize", {
                        type: "menubutton", // https://www.tinymce.com/docs/demo/custom-toolbar-menu-button/
                        text: "",
                        icon: "font-size",
                        menu: [
                            {
                                text: "8",
                                onclick: function () {
                                    editor.formatter.apply("font8");
                                }
                            },
                            {
                                text: "9",
                                onclick: function () {
                                    editor.formatter.apply("font9");
                                }
                            },
                            {
                                text: "10",
                                onclick: function () {
                                    editor.formatter.apply("font10");
                                }
                            },
                            {
                                text: "11",
                                onclick: function () {
                                    editor.formatter.apply("font11");
                                }
                            },
                            {
                                text: "12",
                                onclick: function () {
                                    editor.formatter.apply("font12");
                                }
                            },
                            {
                                text: "14",
                                onclick: function () {
                                    editor.formatter.apply("font14");
                                }
                            },
                            {
                                text: "16",
                                onclick: function () {
                                    editor.formatter.apply("font16");
                                }
                            },
                            {
                                text: "18",
                                onclick: function () {
                                    editor.formatter.apply("font18");
                                }
                            },
                            {
                                text: "20",
                                onclick: function () {
                                    editor.formatter.apply("font20");
                                }
                            }
                        ]
                    });
                }
            }
        };
        angular.merge($scope.to, to);

        let validators = {
            // tinyMCE may leave empty tags that cause the value to appear not empty
            requiredCustom: {
                expression: function ($viewValue, $modelValue, scope) {
                    let isEmpty = false;
                    if (scope.to && scope.to.required) {
                        let div = document.createElement("div");
                        div.innerHTML = ($modelValue || "").toString();
                        isEmpty = div.innerText.trim() === "";
                    }
                    scope.options.validation.show = isEmpty;
                    return !isEmpty;
                }
            }
        };
        $scope.options["validators"] = validators;

        $scope["$on"]("$destroy", () => {
            this.removeObserver();
            this.handleLinks(this.$scope["tinymceBody"].querySelectorAll("a"), true);
        });
    }

    private disableEditability = (event) => {
        if (this.$scope["tinymceBody"] && !this.$scope["tinymceBody"].classList.contains("mce-edit-focus")) {
            angular.element(this.$scope["tinymceBody"]).attr("contentEditable", "false");
        }
    };

    private enableEditability = (event) => {
        angular.element(this.$scope["tinymceBody"]).attr("contentEditable", "true");
    };

    private handleClick = function(event) {
        console.log("handleClick");
        event.stopPropagation();
        event.preventDefault();

        const href = this.href;
        if (href.indexOf("?ArtifactId=") !== -1 && this.getAttribute("artifactid")) {
            const artifactId = parseInt(href.split("?ArtifactId=")[1], 10);
            if (artifactId === parseInt(this.getAttribute("artifactid"), 10)) {
                self.location.href = "/#/main/" + artifactId;
            }
        } else {
            window.open(href, "_blank");
        }
    };

    private handleLinks = (nodeList: Node[] | NodeList, remove: boolean = false) => {
        if (nodeList.length === 0) {
            return;
        }
        for (let i = 0; i < nodeList.length; i++) {
            let element = nodeList[i] as HTMLElement;

            element.removeEventListener("click", this.handleClick);

            if (!remove) {
                angular.element(element).attr("contentEditable", "false");
                angular.element(element).attr("data-mce-contenteditable", "false");

                element.addEventListener("mouseover", this.disableEditability);
                element.addEventListener("mouseout", this.enableEditability);
                element.addEventListener("click", this.handleClick);
            } else {
                element.removeEventListener("mouseover", this.disableEditability);
                element.removeEventListener("mouseout", this.enableEditability);
            }
        }
    };

    private handleMutation = (mutation: MutationRecord) => {
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

    private removeObserver = () => {
        if (this.observer) {
            this.observer.disconnect();
        }
    };
}