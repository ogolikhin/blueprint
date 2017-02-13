import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {IFormlyScope} from "../../formly-config";
import {BPFieldBaseRTFController} from "./base-rtf-controller";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {IDialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {IArtifactService} from "../../../../managers/artifact-manager/artifact/artifact.svc";
import {IArtifactRelationships} from "../../../../managers/artifact-manager/relationships/relationships";
import {IMessageService} from "../../../../main/components/messages/message.svc";

export class BPFieldTextRTFInline implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTFInline";
    public template: string = require("./text-rtf-inline.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope: IFormlyScope) {
        $scope.$applyAsync(() => {
            $scope.fc.$setTouched();
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextRTFInlineController;
}

export class BpFieldTextRTFInlineController extends BPFieldBaseRTFController {
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
                $scope: IFormlyScope,
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

        this.isSingleLine = true;

        this.allowedFonts = ["Open Sans", "Arial", "Cambria", "Calibri", "Courier New", "Times New Roman", "Trebuchet MS", "Verdana"];

        const to: AngularFormly.ITemplateOptions = {
            tinymceOptions: { // this will go to ui-tinymce directive
                inline: true,
                fixed_toolbar_container: ".tinymce-toolbar-" + $scope.options["key"],
                menubar: false,
                toolbar: "bold italic underline strikethrough | fontselect forecolor | linkstraces | removeformat",
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
                // https://www.tinymce.com/docs/configure/content-filtering/#force_hex_style_colors
                force_hex_style_colors: true,
                convert_urls: false, // https://www.tinymce.com/docs/configure/url-handling/#convert_urls
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
                paste_preprocess: this.pastePreProcess,
                paste_postprocess: this.pastePostProcess,
                init_instance_callback: this.initInstanceCallback,
                setup: (editor) => {
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
