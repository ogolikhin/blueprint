import * as angular from "angular";
import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import { BPFieldBaseRTFController } from "./base-rtf-controller";
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

export class BpFieldTextRTFInlineController extends BPFieldBaseRTFController {
    static $inject: [string] = ["$scope"];

    constructor(private $scope: AngularFormly.ITemplateScope) {
        super();

        let onChange = ($scope.to.onChange as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.
        $scope.to.onChange = () => { };

        const allowedFonts = ["Open Sans", "Arial", "Cambria", "Calibri", "Courier New", "Times New Roman", "Trebuchet MS", "Verdana"];
        let fontFormats = "";
        allowedFonts.forEach(function (font) {
            fontFormats += `${font}=` + (font.indexOf(" ") !== -1 ? `"${font}";` : `${font};`);
        });
        const bogusRegEx = /<br data-mce-bogus="1">/gi;

        let to: AngularFormly.ITemplateOptions = {
            tinymceOptions: { // this will go to ui-tinymce directive
                inline: true,
                fixed_toolbar_container: ".tinymce-toolbar-" + $scope.options["key"],
                menubar: false,
                toolbar: "fontselect fontsize bold italic underline forecolor format link",
                statusbar: false,
                valid_elements: "span[*],a[*],strong/b,em/i,u,sup,sub",
                extended_valid_elements: "a[href|type|title|linkassemblyqualifiedname|text|canclick|isvalid|mentionid|isgroup|email|" +
                "class|linkfontsize|linkfontfamily|linkfontstyle|linkfontweight|linktextdecoration|linkforeground|style|target|artifactid]",
                //invalid_elements: "p,br,hr,img,frame,iframe,script,table,thead,tbody,tr,td,ul,ol,li,dd,dt,dl,div,input,select,textarea",
                invalid_styles: {
                    "*": "background-image display margin padding float"
                },
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
                    Helper.autoLinkURLText(args.node);
                    Helper.setFontFamilyOrOpenSans(args.node, allowedFonts);
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
                    Helper.setFontFamilyOrOpenSans(editorBody, allowedFonts);
                    this.handleLinks(editorBody.querySelectorAll("a"));

                    // MutationObserver
                    const mutationObserver = window["MutationObserver"] || window["WebKitMutationObserver"] || window["MozMutationObserver"];
                    if (!angular.isUndefined(mutationObserver)) {
                        // create an observer instance
                        this.observer = new MutationObserver((mutations) => {
                            mutations.forEach(this.handleMutation);
                        });

                        const observerConfig = { attributes: false, childList: true, characterData: false, subtree: true };
                        this.observer.observe(editorBody, observerConfig);
                    }

                    let contentBody = editorBody.innerHTML.replace(bogusRegEx, "");

                    editor.on("Change", (e) => {
                        const _contentBody = editorBody.innerHTML.replace(bogusRegEx, "");
                        if (contentBody !== _contentBody) {
                            contentBody = _contentBody;
                            onChange(contentBody, $scope.options, $scope);
                        }
                    });

                    editor.on("Focus", (e) => {
                        if (editorBody.parentElement) {
                            editorBody.parentElement.classList.remove("tinymce-toolbar-hidden");
                        }
                    });

                    editor.on("Blur", (e) => {
                        if (editorBody.parentElement) {
                            editorBody.parentElement.classList.add("tinymce-toolbar-hidden");
                        }
                    });
                },
                setup: function (editor) {
                    editor.addButton("format", {
                        title: "Format",
                        type: "menubutton",
                        text: "",
                        icon: "format",
                        menu: [
                            {
                                icon: "strikethrough",
                                text: " Strikethrough",
                                onclick: function () { editor.editorCommands.execCommand("Strikethrough"); }
                            },
                            { text: "-" },
                            {
                                icon: "removeformat",
                                text: " Clear formatting",
                                onclick: function () { editor.editorCommands.execCommand("RemoveFormat"); }
                            }
                        ]
                    });
                    editor.addButton("fontsize", {
                        title: "Font Size",
                        type: "menubutton", // https://www.tinymce.com/docs/demo/custom-toolbar-menu-button/
                        text: "",
                        icon: "font-size",
                        menu: [
                            {
                                text: "8",
                                onclick: function () { editor.formatter.apply("font8"); }
                            },
                            {
                                text: "9",
                                onclick: function () { editor.formatter.apply("font9"); }
                            },
                            {
                                text: "10",
                                onclick: function () { editor.formatter.apply("font10"); }
                            },
                            {
                                text: "11",
                                onclick: function () { editor.formatter.apply("font11"); }
                            },
                            {
                                text: "12",
                                onclick: function () { editor.formatter.apply("font12"); }
                            },
                            {
                                text: "14",
                                onclick: function () { editor.formatter.apply("font14"); }
                            },
                            {
                                text: "16",
                                onclick: function () { editor.formatter.apply("font16"); }
                            },
                            {
                                text: "18",
                                onclick: function () { editor.formatter.apply("font18"); }
                            },
                            {
                                text: "20",
                                onclick: function () { editor.formatter.apply("font20"); }
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
                        isEmpty = !Helper.tagsContainText($modelValue);
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
}