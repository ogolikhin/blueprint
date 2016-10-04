import * as angular from "angular";
import "angular-formly";
import { Helper } from "../../../../shared";

export class BPFieldTextRTFInline implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTFInline";
    public template: string = require("./text-rtf-inline.template.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();

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
            tinymceOption: { // this will go to ui-tinymce directive
                inline: true,
                fixed_toolbar_container: ".tinymce-toolbar-" + $scope.options["key"],
                menubar: false,
                toolbar: "fontsize | bold italic underline | forecolor format | link table",
                statusbar: false,
                invalid_elements: "img,frame,iframe,script",
                invalid_styles: {
                    "*": "background background-image background-color"
                },
                paste_remove_styles_if_webkit: false, // https://www.tinymce.com/docs/plugins/paste/#paste_remove_styles_if_webkit
                // we don't need the autoresize plugin when using the inline version of tinyMCE as the height will
                // be controlled using CSS (max-height, min-height)
                plugins: "paste textcolor table noneditable autolink link", // contextmenu",
                contextmenu: "bold italic underline strikethrough | link inserttable | cell row column deletetable",
                // paste_preprocess: function (plugin, args) { // https://www.tinymce.com/docs/plugins/paste/#paste_preprocess
                // },
                paste_postprocess: (plugin, args) => { // https://www.tinymce.com/docs/plugins/paste/#paste_postprocess
                    Helper.autoLinkURLText(args.node);
                },
                init_instance_callback: (editor) => {
                    editor.formatter.register("font8px", {
                        inline: "span",
                        styles: { "font-size": "8px" }
                    });
                    editor.formatter.register("font10px", {
                        inline: "span",
                        styles: { "font-size": "10px" }
                    });
                    editor.formatter.register("font12px", {
                        inline: "span",
                        styles: { "font-size": "12px" }
                    });
                    editor.formatter.register("font15px", {
                        inline: "span",
                        styles: { "font-size": "15px" }
                    });
                    editor.formatter.register("font18px", {
                        inline: "span",
                        styles: { "font-size": "18px" }
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
                            { icon: "indent", text: " Indent", onclick: function () { tinymce.execCommand("Indent"); } }
                        ]
                    });
                    editor.addButton("fontsize", {
                        type: "menubutton", // https://www.tinymce.com/docs/demo/custom-toolbar-menu-button/
                        text: "",
                        icon: "font-size",
                        menu: [
                            {
                                text: "8px",
                                onclick: function () {
                                    editor.formatter.apply("font8px");
                                }
                            },
                            {
                                text: "10px",
                                onclick: function () {
                                    editor.formatter.apply("font10px");
                                }
                            }, {
                                text: "12px",
                                onclick: function () {
                                    editor.formatter.apply("font12px");
                                }
                            }, {
                                text: "15px",
                                onclick: function () {
                                    editor.formatter.apply("font15px");
                                }
                            }, {
                                text: "18px",
                                onclick: function () {
                                    editor.formatter.apply("font18px");
                                }
                            }]
                    });
                }
            }
        };
        angular.merge($scope.to, to);

        $scope["$on"]("$destroy", () => {
            this.removeObserver();
            this.handleLinks(this.$scope["tinymceBody"].querySelectorAll("a"), true);
        });
    }

    private disableEditability = (event) => {
        angular.element(this.$scope["tinymceBody"]).attr("contentEditable", "false");
    };

    private enableEditability = (event) => {
        angular.element(this.$scope["tinymceBody"]).attr("contentEditable", "true");
    };

    private handleClick = function(event) {
        event.stopPropagation();
        event.preventDefault();

        const href = this.href;
        if (href.indexOf("?ArtifactId=") !== -1) {
            const artifactId = parseInt(href.split("?ArtifactId=")[1], 10);
            self.location.replace("/#/main/" + artifactId);
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
        this.observer.disconnect();
    };
}