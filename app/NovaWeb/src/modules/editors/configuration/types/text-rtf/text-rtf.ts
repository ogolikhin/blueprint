import * as angular from "angular";
import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {BPFieldBaseRTFController} from "./base-rtf-controller";
import {Helper} from "../../../../shared";

export class BPFieldTextRTF implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTF";
    public template: string = require("./text-rtf.template.html");
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
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextRTFController;

    constructor() {
    }
}

export class BpFieldTextRTFController extends BPFieldBaseRTFController {
    static $inject: [string] = ["$scope"];

    constructor(private $scope: AngularFormly.ITemplateScope) {
        super();

        let onChange = ($scope.to.onChange as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.
        $scope.to.onChange = () => {
        };

        const allowedFonts = ["Open Sans", "Arial", "Cambria", "Calibri", "Courier New", "Times New Roman", "Trebuchet MS", "Verdana"];
        let fontFormats = "";
        allowedFonts.forEach(function (font) {
            fontFormats += `${font}=` + (font.indexOf(" ") !== -1 ? `"${font}";` : `${font};`);
        });

        /* tslint:disable:max-line-length */
        const bodyBgColor = "#fbf8e7"; // this is $yellow-pale as defined in styles/modules/_variables.scss
        // pencil icon
        const bodyBgImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA8AAAAQCAYAAADJViUEAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTExIDc5LjE1ODMyNSwgMjAxNS8wOS8xMC0wMToxMDoyMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIDIwMTUgKFdpbmRvd3MpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOkNBRUY4MjFGMTJFNzExRTY5QUM2QjQ5OUFFNTcxMDE1IiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOkNBRUY4MjIwMTJFNzExRTY5QUM2QjQ5OUFFNTcxMDE1Ij4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6Q0FFRjgyMUQxMkU3MTFFNjlBQzZCNDk5QUU1NzEwMTUiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6Q0FFRjgyMUUxMkU3MTFFNjlBQzZCNDk5QUU1NzEwMTUiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz6zEGidAAABYElEQVR42oyST0rDUBDGk9coIog0rt31Bq5Cb+HCI1gQAikeQhDUTYSS7Ny5rhTBC2ThQg/gxn2i/ZP6mprEb2BGQmg0Ax95meQ338wkHaNFDIfDI8dxdBRFuppXLcBTXJ6hNyrSGvY87wSXAZ1N0+xCT67r9v6FAQ7w8n1RFO9lWb5yugv1hDP/AEfsaGRZ9qiUOkShF9/3z5BeQrm5YUZqc1TPx3F8cYegIzSDMlV3rILkSrFYLMbgxtyuko7VplYlMKuRpulDGIaXuNWsDCqqsCJQa32O81Rc4TgJguCKoRnri+al2tLGFgGWZR0nSXJL5/l8PoHjTQ1MobU4WwxvkxPgvm3bfXIEeM1b/eRuUm45/90JgdAedADtQ7v8TDP0QTvje3IsBba4gHyynGdas9O0CRSYEt/QqlJIM7xsAqXtDi9sh4sZPFtWWU656U8UJ8VFpP1CPkcTSPEjwADmppjiAB7dnwAAAABJRU5ErkJggg==";
        /* tslint:enable:max-line-length */

        let to: AngularFormly.ITemplateOptions = {
            tinymceOptions: { // this will go to ui-tinymce directive
                menubar: false,
                toolbar: "fontselect fontsize | bold italic underline | forecolor format | link table",
                statusbar: false,
                content_style: `html { overflow: auto !important; }
                body.mce-content-body { font-family: 'Open Sans', sans-serif; font-size: 9pt; overflow: visible !important; }
                body:hover, body:focus { background: ${bodyBgColor} url(${bodyBgImage}) no-repeat right 4px top 6px; background-attachment: fixed; }
                a:hover { cursor: pointer !important; }`,
                invalid_elements: "img,frame,iframe,script",
                invalid_styles: {
                    "*": "background-image"
                },
                extended_valid_elements: "a[href|type|title|linkassemblyqualifiedname|text|canclick|isvalid|mentionid|isgroup|email|" +
                "class|linkfontsize|linkfontfamily|linkfontstyle|linkfontweight|linktextdecoration|linkforeground|style|target|artifactid]",
                // https://www.tinymce.com/docs/configure/content-formatting/#font_formats
                font_formats: fontFormats,
                // paste_enable_default_filters: false, // https://www.tinymce.com/docs/plugins/paste/#paste_enable_default_filters
                paste_webkit_styles: "none", // https://www.tinymce.com/docs/plugins/paste/#paste_webkit_styles
                paste_remove_styles_if_webkit: true, // https://www.tinymce.com/docs/plugins/paste/#paste_remove_styles_if_webkit
                // https://www.tinymce.com/docs/plugins/paste/#paste_retain_style_properties
                paste_retain_style_properties: "background background-color color " +
                "font font-family font-size font-style font-weight line-height " +
                "margin margin-bottom margin-left margin-right margin-top " +
                "padding padding-bottom padding-left padding-right padding-top " +
                "border-collapse border-color border-style border-width " +
                "text-align text-decoration vertical-align " +
                "height width",
                paste_filter_drop: false,
                table_toolbar: "", // https://www.tinymce.com/docs/plugins/table/#table_toolbar
                table_default_styles: { // https://www.tinymce.com/docs/plugins/table/#table_default_styles
                    borderColor: "#000",
                    borderCollapse: "collapse",
                    borderWidth: "1px"
                },
                table_default_attributes: { // https://www.tinymce.com/docs/plugins/table/#table_default_attributes
                    border: "1",
                    width: "95%"
                },
                plugins: "paste textcolor table noneditable autolink link autoresize",
                autoresize_min_height: 100,
                autoresize_max_height: 400,
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
                    Helper.addTableBorders(args.node);
                    Helper.setFontFamilyOrOpenSans(args.node, allowedFonts);
                },
                init_instance_callback: (editor) => {
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

                    let editorBody = editor.getBody();
                    Helper.autoLinkURLText(editorBody);
                    Helper.addTableBorders(editorBody);
                    Helper.setFontFamilyOrOpenSans(editorBody, allowedFonts);
                    this.handleLinks(editorBody.querySelectorAll("a"));

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
                        this.observer.observe(editorBody, observerConfig);
                    }

                    let contentBody = editorBody.innerHTML;

                    editor.on("Change", (e) => {
                        if (contentBody !== editorBody.innerHTML) {
                            contentBody = editorBody.innerHTML;
                            onChange(contentBody, $scope.options, $scope);
                        }
                    });

                    editor.on("Focus", (e) => {
                        if (editor.editorContainer) {
                            editor.editorContainer.parentElement.classList.remove("tinymce-toolbar-hidden");
                        }
                    });

                    editor.on("Blur", (e) => {
                        if (editor.editorContainer) {
                            editor.editorContainer.parentElement.classList.add("tinymce-toolbar-hidden");
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
                                onclick: function () {
                                    editor.editorCommands.execCommand("Strikethrough");
                                }
                            },
                            {
                                icon: "bullist",
                                text: " Bulleted list",
                                onclick: function () {
                                    editor.editorCommands.execCommand("InsertUnorderedList");
                                }
                            },
                            {
                                icon: "numlist",
                                text: " Numeric list",
                                onclick: function () {
                                    editor.editorCommands.execCommand("InsertOrderedList");
                                }
                            },
                            {
                                icon: "outdent",
                                text: " Outdent",
                                onclick: function () {
                                    editor.editorCommands.execCommand("Outdent");
                                }
                            },
                            {
                                icon: "indent",
                                text: " Indent",
                                onclick: function () {
                                    editor.editorCommands.execCommand("Indent");
                                }
                            },
                            {text: "-"},
                            {
                                icon: "removeformat",
                                text: " Clear formatting",
                                onclick: function () {
                                    editor.editorCommands.execCommand("RemoveFormat");
                                }
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
                        isEmpty = !Helper.tagsContainText($modelValue);
                    }
                    scope.to["isInvalid"] = isEmpty;
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
