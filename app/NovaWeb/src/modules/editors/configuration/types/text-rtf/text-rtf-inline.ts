import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {BPFieldBaseRTFController} from "./base-rtf-controller";
import {Helper} from "../../../../shared";
import {INavigationService} from "../../../../core/navigation/navigation.svc";

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
}

export class BpFieldTextRTFInlineController extends BPFieldBaseRTFController {
    static $inject: [string] = ["$scope", "navigationService"];

    constructor(private $scope: AngularFormly.ITemplateScope, navigationService: INavigationService) {
        super(navigationService);

        let contentBuffer: string = undefined;
        let mceEditor: TinyMceEditor;

        // the onChange event has to be called from the custom validator (!) as otherwise it will fire before the actual validation takes place
        const onChange = ($scope.to.onChange as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.
        //we override the default onChange as we need to deal with changes differently when using tinymce
        $scope.to.onChange = undefined;

        const allowedFonts = ["Open Sans", "Arial", "Cambria", "Calibri", "Courier New", "Times New Roman", "Trebuchet MS", "Verdana"];
        let fontFormats = "";
        allowedFonts.forEach(function (font) {
            fontFormats += `${font}=` + (font.indexOf(" ") !== -1 ? `"${font}";` : `${font};`);
        });

        const to: AngularFormly.ITemplateOptions = {
            tinymceOptions: { // this will go to ui-tinymce directive
                inline: true,
                fixed_toolbar_container: ".tinymce-toolbar-" + $scope.options["key"],
                menubar: false,
                toolbar: "bold italic underline strikethrough | fontselect forecolor | link | removeformat",
                statusbar: false,
                valid_elements: "span[*],a[*],strong/b,em/i,u,sup,sub",
                extended_valid_elements: "a[href|type|title|linkassemblyqualifiedname|text|canclick|isvalid|mentionid|isgroup|email|" +
                "class|linkfontsize|linkfontfamily|linkfontstyle|linkfontweight|linktextdecoration|linkforeground|style|target|artifactid]",
                //invalid_elements: "p,br,hr,img,frame,iframe,script,table,thead,tbody,tr,td,ul,ol,li,dd,dt,dl,div,input,select,textarea",
                invalid_styles: {
                    "*": "background-image display margin padding float white-space"
                },
                object_resizing: false, // https://www.tinymce.com/docs/configure/advanced-editing-behavior/#object_resizing
                // https://www.tinymce.com/docs/configure/content-formatting/#font_formats
                font_formats: fontFormats,
                // paste_enable_default_filters: false, // https://www.tinymce.com/docs/plugins/paste/#paste_enable_default_filters
                paste_webkit_styles: "none", // https://www.tinymce.com/docs/plugins/paste/#paste_webkit_styles
                paste_remove_styles_if_webkit: true, // https://www.tinymce.com/docs/plugins/paste/#paste_remove_styles_if_webkit
                // https://www.tinymce.com/docs/plugins/paste/#paste_retain_style_properties
                paste_retain_style_properties: "background background-color color " +
                "font font-family font-size font-style font-weight " +
                "text-decoration",
                paste_filter_drop: false,
                // we don't need the autoresize plugin when using the inline version of tinyMCE as the height will
                // be controlled using CSS (max-height, min-height)
                plugins: "paste textcolor noneditable autolink link",
                // plugins: "contextmenu", // https://www.tinymce.com/docs/plugins/contextmenu/
                // contextmenu: "bold italic underline strikethrough | link inserttable | cell row column deletetable",
                paste_preprocess: function (plugin, args) { // https://www.tinymce.com/docs/plugins/paste/#paste_preprocess
                    // remove generic font family
                    let content = args.content;
                    content = content.replace(/, ?sans-serif([;'"])/gi, "$1");
                    content = content.replace(/, ?serif([;'"])/gi, "$1");
                    content = content.replace(/, ?monospace([;'"])/gi, "$1");
                    args.content = content;
                },
                paste_postprocess: (plugin, args) => { // https://www.tinymce.com/docs/plugins/paste/#paste_postprocess
                    prepBody(args.node);
                },
                init_instance_callback: (editor) => {
                    mceEditor = editor;

                    editor.formatter.register("font8", {
                        inline: "span",
                        styles: {"font-size": "8pt"}
                    });
                    editor.formatter.register("font9", { // default font, equivalent to 12px
                        inline: "span",
                        styles: {"font-size": "9pt"}
                    });
                    editor.formatter.register("font10", {
                        inline: "span",
                        styles: {"font-size": "10pt"}
                    });
                    editor.formatter.register("font11", {
                        inline: "span",
                        styles: {"font-size": "11pt"}
                    });
                    editor.formatter.register("font12", {
                        inline: "span",
                        styles: {"font-size": "12pt"}
                    });
                    editor.formatter.register("font14", {
                        inline: "span",
                        styles: {"font-size": "14pt"}
                    });
                    editor.formatter.register("font16", {
                        inline: "span",
                        styles: {"font-size": "16pt"}
                    });
                    editor.formatter.register("font18", {
                        inline: "span",
                        styles: {"font-size": "18pt"}
                    });
                    editor.formatter.register("font20", {
                        inline: "span",
                        styles: {"font-size": "20pt"}
                    });

                    this.editorBody = editor.getBody();
                    prepBody(this.editorBody);
                    updateModel();

                    // MutationObserver
                    const mutationObserver = window["MutationObserver"] || window["WebKitMutationObserver"] || window["MozMutationObserver"];
                    if (!angular.isUndefined(mutationObserver)) {
                        // create an observer instance
                        this.observer = new MutationObserver((mutations) => {
                            mutations.forEach(this.handleMutation);
                        });

                        const observerConfig = {
                            attributes: false,
                            childList: true,
                            characterData: false,
                            subtree: true
                        };
                        this.observer.observe(this.editorBody, observerConfig);
                    }

                    editor.on("Dirty", (e) => {
                        if (!$scope.options["data"].isFresh) {
                            const value = editor.getContent();
                            if (contentBuffer !== value) {
                                triggerChange(value);
                            }
                        }
                    });

                    editor.on("Change", (e) => {
                        if (!$scope.options["data"].isFresh) {
                            const value = editor.getContent();
                            if (contentBuffer !== value) {
                                triggerChange(value);
                            }
                        } else { // this will get called when refreshing the artifact
                            prepBody(editor.getBody());
                            updateModel();
                        }
                    });

                    editor.on("Focus", (e) => {
                        if (this.editorBody.parentElement && this.editorBody.parentElement.parentElement) {
                            this.editorBody.parentElement.parentElement.classList.remove("tinymce-toolbar-hidden");
                        }
                    });

                    editor.on("Blur", (e) => {
                        if (this.editorBody.parentElement && this.editorBody.parentElement.parentElement) {
                            this.editorBody.parentElement.parentElement.classList.add("tinymce-toolbar-hidden");
                        }
                    });
                },
                setup: function (editor) {
                    editor.addButton("fontsize", {
                        title: "Font Size",
                        type: "menubutton", // https://www.tinymce.com/docs/demo/custom-toolbar-menu-button/
                        text: "",
                        icon: "font-size",
                        menu: [
                            {
                                text: "8",
                                onclick: function () {
                                    editor.formatter.apply("font8");
                                    triggerChange(editor.getContent());
                                }
                            },
                            {
                                text: "9",
                                onclick: function () {
                                    editor.formatter.apply("font9");
                                    triggerChange(editor.getContent());
                                }
                            },
                            {
                                text: "10",
                                onclick: function () {
                                    editor.formatter.apply("font10");
                                    triggerChange(editor.getContent());
                                }
                            },
                            {
                                text: "11",
                                onclick: function () {
                                    editor.formatter.apply("font11");
                                    triggerChange(editor.getContent());
                                }
                            },
                            {
                                text: "12",
                                onclick: function () {
                                    editor.formatter.apply("font12");
                                    triggerChange(editor.getContent());
                                }
                            },
                            {
                                text: "14",
                                onclick: function () {
                                    editor.formatter.apply("font14");
                                    triggerChange(editor.getContent());
                                }
                            },
                            {
                                text: "16",
                                onclick: function () {
                                    editor.formatter.apply("font16");
                                    triggerChange(editor.getContent());
                                }
                            },
                            {
                                text: "18",
                                onclick: function () {
                                    editor.formatter.apply("font18");
                                    triggerChange(editor.getContent());
                                }
                            },
                            {
                                text: "20",
                                onclick: function () {
                                    editor.formatter.apply("font20");
                                    triggerChange(editor.getContent());
                                }
                            }
                        ]
                    });
                }
            }
        };
        angular.merge($scope.to, to);

        $scope.options["validators"] = {
            // tinyMCE may leave empty tags that cause the value to appear not empty
            requiredCustom: {
                expression: ($viewValue, $modelValue, scope) => {
                    let value = mceEditor ? mceEditor.getContent() : $modelValue;
                    if (scope.options && scope.options.data && scope.options.data.isFresh) {
                        contentBuffer = value;
                        scope.options.data.isFresh = false;
                    }

                    if (contentBuffer !== value) {
                        triggerChange(value);
                    }

                    let isEmpty = false;
                    if (scope.to && scope.to.required) {
                        isEmpty = !Helper.tagsContainText($modelValue);
                    }
                    scope.options.validation.show = isEmpty;
                    return !isEmpty;
                }
            }
        };

        $scope["$on"]("$destroy", () => {
            this.removeObserver();
            this.handleLinks(this.$scope["tinymceBody"].querySelectorAll("a"), true);
        });

        function triggerChange(newContent: string) {
            contentBuffer = newContent;
            if (typeof onChange === "function") {
                onChange(newContent, $scope.options, $scope);
            }
        }

        function prepBody(body: Node) {
            Helper.autoLinkURLText(body);
            Helper.setFontFamilyOrOpenSans(body, allowedFonts);
        }

        function updateModel() {
            if (mceEditor) {
                contentBuffer = mceEditor.getContent();
                $scope.model[$scope.options["key"]] = contentBuffer;
                $scope.options["data"].isFresh = false;
            }
        }
    }
}
