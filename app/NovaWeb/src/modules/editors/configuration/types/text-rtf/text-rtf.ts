import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {BPFieldBaseRTFController} from "./base-rtf-controller";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {IDialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {IArtifactService} from "../../../../managers/artifact-manager/artifact/artifact.svc";
import {IArtifactRelationships} from "../../../../managers/artifact-manager/relationships/relationships";
import {IMessageService} from "../../../../core/messages/message.svc";

export class BPFieldTextRTF implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTF";
    public template: string = require("./text-rtf.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
            ($scope["options"] as AngularFormly.IFieldConfigurationObject).validation.show = ($scope["fc"] as ng.IFormController).$invalid;
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextRTFController;
}

export class BpFieldTextRTFController extends BPFieldBaseRTFController {
    static $inject: [string] = [
        "$q",
        "$scope",
        "$window",
        "navigationService",
        "validationService",
        "messageService",
        "localization",
        "dialogService",
        "selectionManager",
        "artifactService",
        "artifactRelationships"
    ];

    constructor($q: ng.IQService,
                $scope: AngularFormly.ITemplateScope,
                $window: ng.IWindowService,
                navigationService: INavigationService,
                validationService: IValidationService,
                messageService: IMessageService,
                localization: ILocalizationService,
                dialogService: IDialogService,
                selectionManager: ISelectionManager,
                artifactService: IArtifactService,
                artifactRelationships: IArtifactRelationships) {
        super($q, $scope, $window, navigationService, validationService, messageService,
            localization, dialogService, selectionManager, artifactService, artifactRelationships);

        const inlineBgColor = "#e8e8e8"; // this is $gray-lightest as defined in _colors.scss
        const bodyBgColor = "#fbf8e7"; // this is $yellow-pale as defined in _colors.scss
        /* tslint:disable:max-line-length */
        // pencil icon
        const bodyBgImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA8AAAAQCAYAAADJViUEAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuNi1jMTExIDc5LjE1ODMyNSwgMjAxNS8wOS8xMC0wMToxMDoyMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENDIDIwMTUgKFdpbmRvd3MpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOkNBRUY4MjFGMTJFNzExRTY5QUM2QjQ5OUFFNTcxMDE1IiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOkNBRUY4MjIwMTJFNzExRTY5QUM2QjQ5OUFFNTcxMDE1Ij4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6Q0FFRjgyMUQxMkU3MTFFNjlBQzZCNDk5QUU1NzEwMTUiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6Q0FFRjgyMUUxMkU3MTFFNjlBQzZCNDk5QUU1NzEwMTUiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz6zEGidAAABYElEQVR42oyST0rDUBDGk9coIog0rt31Bq5Cb+HCI1gQAikeQhDUTYSS7Ny5rhTBC2ThQg/gxn2i/ZP6mprEb2BGQmg0Ax95meQ338wkHaNFDIfDI8dxdBRFuppXLcBTXJ6hNyrSGvY87wSXAZ1N0+xCT67r9v6FAQ7w8n1RFO9lWb5yugv1hDP/AEfsaGRZ9qiUOkShF9/3z5BeQrm5YUZqc1TPx3F8cYegIzSDMlV3rILkSrFYLMbgxtyuko7VplYlMKuRpulDGIaXuNWsDCqqsCJQa32O81Rc4TgJguCKoRnri+al2tLGFgGWZR0nSXJL5/l8PoHjTQ1MobU4WwxvkxPgvm3bfXIEeM1b/eRuUm45/90JgdAedADtQ7v8TDP0QTvje3IsBba4gHyynGdas9O0CRSYEt/QqlJIM7xsAqXtDi9sh4sZPFtWWU656U8UJ8VFpP1CPkcTSPEjwADmppjiAB7dnwAAAABJRU5ErkJggg==";
        /* tslint:enable:max-line-length */

        this.allowedFonts = ["Open Sans", "Arial", "Cambria", "Calibri", "Courier New", "Times New Roman", "Trebuchet MS", "Verdana"];

        const to: AngularFormly.ITemplateOptions = {
            tinymceOptions: { // this will go to ui-tinymce directive
                menubar: false,
                toolbar: "bold italic underline strikethrough | fontsize fontselect forecolor format | linkstraces table",
                statusbar: false,
                content_style: `html { height: 100%; overflow: auto !important; }
                body.mce-content-body { background: transparent; font-family: 'Open Sans', sans-serif; font-size: 9pt; min-height: 100px;
                margin: 8px 20px 8px 8px; overflow: visible !important; padding-bottom: 0 !important; }
                html:hover, html:focus { background: ${bodyBgColor} url(${bodyBgImage}) no-repeat right 4px top 6px; background-attachment: fixed; }
                body.mce-content-body *[contentEditable=false] *[contentEditable=true]:focus,
                body.mce-content-body *[contentEditable=false] *[contentEditable=true]:hover,
                body.mce-content-body *[contentEditable=false][data-mce-selected] {background: ${inlineBgColor}; outline: none !important; }
                a:hover { cursor: pointer !important; }
                p { margin: 0 0 8px; }`,
                extended_valid_elements: "a[href|type|title|linkassemblyqualifiedname|text|canclick|isvalid|mentionid|isgroup|email|" +
                "class|linkfontsize|linkfontfamily|linkfontstyle|linkfontweight|linktextdecoration|linkforeground|style|target|artifactid]",
                invalid_elements: "img,frame,iframe,script",
                invalid_styles: {
                    "*": "background-image"
                },
                // https://www.tinymce.com/docs/configure/content-formatting/#font_formats
                font_formats: this.fontFormats(),
                convert_urls: false, // https://www.tinymce.com/docs/configure/url-handling/#convert_urls
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
                paste_word_valid_elements: "-strong/b,-em/i,-u,-span,-p,-ol,-ul,-li,-h1,-h2,-h3,-h4,-h5,-h6," +
                "-p/div[align],-a[href|name],sub,sup,strike,br,del,table[align|width],tr," +
                "td[colspan|rowspan|width|align|valign],th[colspan|rowspan|width],thead,tfoot,tbody",
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
                autoresize_on_init: true,
                autoresize_min_height: 150,
                autoresize_max_height: 350,
                autoresize_overflow_padding: 0,
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
                    this.normalizeHtml(args.node, true);
                },
                init_instance_callback: (editor) => {
                    this.mceEditor = editor;

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

                    this.prepRTF(true);

                    // MutationObserver
                    const mutationObserver = window["MutationObserver"] || window["WebKitMutationObserver"] || window["MozMutationObserver"];
                    if (!_.isUndefined(mutationObserver)) {
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

                    editor.on("Change", (e) => {
                        const currentContent = editor.getContent();
                        if (currentContent !== this.contentBuffer) {
                            if (!$scope.options["data"].isFresh) {
                                this.triggerChange(currentContent);
                            } else { // this will get called when refreshing the artifact
                                this.prepRTF(true);
                            }
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
                setup: (editor) => {
                    editor.addButton("format", {
                        title: "Format",
                        type: "menubutton",
                        text: "",
                        icon: "format",
                        menu: [
                            {
                                icon: "bullist",
                                text: " Bulleted list",
                                onclick: () => {
                                    editor.editorCommands.execCommand("InsertUnorderedList");
                                }
                            },
                            {
                                icon: "numlist",
                                text: " Numeric list",
                                onclick: () => {
                                    editor.editorCommands.execCommand("InsertOrderedList");
                                }
                            },
                            {
                                icon: "outdent",
                                text: " Outdent",
                                onclick: () => {
                                    editor.editorCommands.execCommand("Outdent");
                                }
                            },
                            {
                                icon: "indent",
                                text: " Indent",
                                onclick: () => {
                                    editor.editorCommands.execCommand("Indent");
                                }
                            },
                            {text: "-"},
                            {
                                icon: "removeformat",
                                text: " Clear formatting",
                                onclick: () => {
                                    editor.editorCommands.execCommand("RemoveFormat");
                                    this.triggerChange();
                                }
                            }
                        ]
                    });
                    editor.addButton("fontsize", {
                        title: "Font Size",
                        type: "menubutton", // https://www.tinymce.com/docs/demo/custom-toolbar-menu-button/
                        text: "",
                        icon: "font-size",
                        menu: this.fontSizeMenu(editor)
                    });
                    editor.addButton("linkstraces", {
                        title: "Links",
                        type: "menubutton",
                        text: "",
                        icon: "link",
                        menu: this.linksMenu(editor)
                    });
                }
            }
        };
        _.assign($scope.to, to);
    }
}
