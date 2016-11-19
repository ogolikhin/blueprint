import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {BPFieldBaseRTFController} from "./base-rtf-controller";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {IDialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {IArtifactRelationships} from "../../../../managers/artifact-manager/relationships/relationships";
import {IMessageService} from "../../../../core/messages/message.svc";

export class BPFieldTextRTFInline implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTFInline";
    public template: string = require("./text-rtf-inline.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            $scope["fc"].$setTouched();
            ($scope["options"] as AngularFormly.IFieldConfigurationObject).validation.show = ($scope["fc"] as ng.IFormController).$invalid;
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextRTFInlineController;
}

export class BpFieldTextRTFInlineController extends BPFieldBaseRTFController {
    static $inject: [string] = [
        "$scope",
        "navigationService",
        "validationService",
        "messageService",
        "localization",
        "dialogService",
        "selectionManager",
        "artifactRelationships"
    ];

    constructor($scope: AngularFormly.ITemplateScope,
                navigationService: INavigationService,
                validationService: IValidationService,
                messageService: IMessageService,
                localization: ILocalizationService,
                dialogService: IDialogService,
                selectionManager: ISelectionManager,
                artifactRelationships: IArtifactRelationships) {
        super($scope, navigationService, validationService, messageService, localization, dialogService, selectionManager, artifactRelationships);

        this.allowedFonts = ["Open Sans", "Arial", "Cambria", "Calibri", "Courier New", "Times New Roman", "Trebuchet MS", "Verdana"];

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
                font_formats: this.fontFormats(),
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
                    this.normalizeHtml(args.node);
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

                    this.prepRTF();

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
                                this.prepRTF();
                            }
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
                setup: (editor) => {
                    editor.addButton("fontsize", {
                        title: "Font Size",
                        type: "menubutton", // https://www.tinymce.com/docs/demo/custom-toolbar-menu-button/
                        text: "",
                        icon: "font-size",
                        menu: [
                            {
                                text: "8",
                                onclick: () => {
                                    editor.formatter.apply("font8");
                                    this.triggerChange();
                                }
                            },
                            {
                                text: "9",
                                onclick: () => {
                                    editor.formatter.apply("font9");
                                    this.triggerChange();
                                }
                            },
                            {
                                text: "10",
                                onclick: () => {
                                    editor.formatter.apply("font10");
                                    this.triggerChange();
                                }
                            },
                            {
                                text: "11",
                                onclick: () => {
                                    editor.formatter.apply("font11");
                                    this.triggerChange();
                                }
                            },
                            {
                                text: "12",
                                onclick: () => {
                                    editor.formatter.apply("font12");
                                    this.triggerChange();
                                }
                            },
                            {
                                text: "14",
                                onclick: () => {
                                    editor.formatter.apply("font14");
                                    this.triggerChange();
                                }
                            },
                            {
                                text: "16",
                                onclick: () => {
                                    editor.formatter.apply("font16");
                                    this.triggerChange();
                                }
                            },
                            {
                                text: "18",
                                onclick: () => {
                                    editor.formatter.apply("font18");
                                    this.triggerChange();
                                }
                            },
                            {
                                text: "20",
                                onclick: () => {
                                    editor.formatter.apply("font20");
                                    this.triggerChange();
                                }
                            }
                        ]
                    });
                    editor.addButton("linkstraces", {
                        title: "Links",
                        type: "menubutton",
                        text: "",
                        icon: "link",
                        menu: [
                            {
                                icon: "link",
                                text: " Links",
                                onclick: () => {
                                    editor.editorCommands.execCommand("mceLink");
                                }
                            },
                            {
                                icon: "numlist",
                                text: " Inline traces",
                                onclick: () => {
                                    this.openArtifactPicker();
                                }
                            }
                        ]
                    });
                }
            }
        };
        _.assign($scope.to, to);
    }
}
