import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {IFormlyScope} from "../../formly-config";
import {BPFieldBaseRTFController} from "./base-rtf-controller";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {IDialogService, IDialogSettings} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {IArtifactService} from "../../../../managers/artifact-manager/artifact/artifact.svc";
import {IArtifactRelationships} from "../../../../managers/artifact-manager/relationships/relationships";
import {
    BpFileUploadStatusController,
    IUploadStatusDialogData, IUploadStatusResult
} from "../../../../shared/widgets/bp-file-upload-status/bp-file-upload-status";
import {ISettingsService} from "../../../../commonModule/configuration/settings.service";
import {IFileUploadService, IFileResult} from "../../../../commonModule/fileUpload/fileUpload.service";
import {Helper} from "../../../../shared/utils/helper";
import {IMessageService} from "../../../../main/components/messages/message.svc";

export class BPFieldTextRTF implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldTextRTF";
    public template: string = require("./text-rtf.html");
    public wrapper: string[] = ["bpFieldLabel", "bootstrapHasError"];
    public link: ng.IDirectiveLinkFn = function ($scope: IFormlyScope) {
        $scope.$applyAsync(() => {
            $scope.fc.$setTouched();
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldTextRTFController;
}

interface ISize {
    width: number;
    height: number;
}

export class BpFieldTextRTFController extends BPFieldBaseRTFController {
    static $inject: [string] = [
        "$q",
        "$log",
        "$scope",
        "$window",
        "$filter",
        "navigationService",
        "validationService",
        "messageService",
        "localization",
        "dialogService",
        "selectionManager",
        "artifactService",
        "fileUploadService",
        "settings",
        "artifactRelationships"
    ];

    constructor(protected $q: ng.IQService,
                private $log: ng.ILogService,
                $scope: IFormlyScope,
                protected $window: ng.IWindowService,
                private $filter: ng.IFilterService,
                navigationService: INavigationService,
                validationService: IValidationService,
                messageService: IMessageService,
                localization: ILocalizationService,
                dialogService: IDialogService,
                selectionManager: ISelectionManager,
                artifactService: IArtifactService,
                private fileUploadService: IFileUploadService,
                private settingsService: ISettingsService,
                artifactRelationships: IArtifactRelationships) {
        super($q, $scope, $window, navigationService, validationService, messageService,
            localization, dialogService, selectionManager, artifactService, artifactRelationships);

        this.allowedFonts = ["Open Sans", "Arial", "Cambria", "Calibri", "Courier New", "Times New Roman", "Trebuchet MS", "Verdana"];

        const addImagesToolbar = $scope.options["data"]["allowAddImages"] ? " uploadimage" : "";

        const to: AngularFormly.ITemplateOptions = {
            tinymceOptions: { // this will go to ui-tinymce directive
                menubar: false,
                toolbar: "bold italic underline strikethrough | fontsize fontselect forecolor format | linkstraces table" + addImagesToolbar,
                statusbar: false,
                body_class: this.$window.document.body.getAttribute("class"),
                content_css: "/novaweb/libs/tinymce/content.css", // https://www.tinymce.com/docs/configure/content-appearance/#content_css
                extended_valid_elements: "a[href|type|title|linkassemblyqualifiedname|text|canclick|isvalid|mentionid|isgroup|email|" +
                "class|linkfontsize|linkfontfamily|linkfontstyle|linkfontweight|linktextdecoration|linkforeground|style|target|artifactid]",
                invalid_elements: "frame,iframe,script",
                invalid_styles: {
                    "*": "background-image"
                },
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
                paste_preprocess: this.pastePreProcess,
                paste_postprocess: this.pastePostProcess,
                init_instance_callback: this.initInstanceCallback,
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

                    editor.addButton("uploadimage", {
                        title: "Insert Image",
                        text: "",
                        icon: "image",
                        onclick: () => {
                            const input = angular.element(`<input type="file" name="image_file"
                                    accept=".jpeg,.jpg,.png" style="display: none">`);

                            input.one("change", (event: Event) => {
                                const inputElement = <HTMLInputElement>event.currentTarget;
                                const imageFile = inputElement.files[0];
                                const numberOfExistingImages = this.getNumberOfImagesInContent(editor.getContent());
                                let dimensions: ISize;

                                this.uploadImage(imageFile, numberOfExistingImages).then((uploadedImageUrl: string) => {
                                    this.getImageDimensions(imageFile)
                                        .then(dim => {
                                            dimensions = this.getScaledDimensions(dim);
                                        })
                                        .finally(() => {
                                            const imageContent = editor.dom.createHTML("img", {
                                                src: uploadedImageUrl,
                                                width: dimensions.width,
                                                height: dimensions.height
                                            });
                                            editor.insertContent(`<span>${imageContent}</span>`);
                                            this.addEmbeddedImageToList(uploadedImageUrl);
                                        });
                                });
                            });

                            input[0].click();
                        }
                    });
                }
            }
        };
        _.assign($scope.to, to);
    }

    private getNumberOfImagesInContent(content: string): number {
        return _.isEmpty(content) ? 0 : angular.element(content).find("img").length;
    }

    private getScaledDimensions(dimensions: ISize): ISize {
        const max = 400;
        if (dimensions.width && dimensions.height && Math.max(dimensions.width, dimensions.height) > max) {
            const ratio = Math.min(max / dimensions.width, max / dimensions.height);
            return {width: Math.round(dimensions.width * ratio), height: Math.round(dimensions.height * ratio)};

        } else {
            return dimensions;
        }
    }

    private getImageDimensions(imageFile: File): ng.IPromise<ISize> {
        const deferred = this.$q.defer<{width: number, height: number}>();

        const tempImage: any = new Image();
        tempImage.onload = function(this: HTMLImageElement, event: Event) {
            deferred.resolve({width: this.naturalWidth, height: this.naturalHeight});
        };
        tempImage.onerror = function() {
            deferred.reject();
        };
        tempImage.src = this.$window.URL.createObjectURL(imageFile);

        return deferred.promise;
    }

    private uploadImage(file: File, numberOfExistingImages: number): ng.IPromise<string> {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Ok", "OK"),
            template: require("../../../../shared/widgets/bp-file-upload-status/bp-file-upload-status.html"),
            controller: BpFileUploadStatusController,
            css: "nova-file-upload-status",
            header: this.localization.get("App_UP_Attachments_Upload_Dialog_Header", "File Upload"),
            backdrop: false
        };

        const uploadFile = (file: File,
                            progressCallback: (event: ProgressEvent) => void,
                            cancelPromise: ng.IPromise<void>): ng.IPromise<IFileResult> => {

            return this.fileUploadService.uploadImageToFileStore(file, progressCallback, cancelPromise);
        };

        let filesize = this.settingsService.getNumber("MaxEmbeddedImageFileSize", Helper.defaultMaxEmbeddedImageFileSize, 1);
        if (!_.isFinite(filesize) || filesize < 0 || filesize > Helper.defaultMaxEmbeddedImageFileSize) {
            filesize = Helper.defaultMaxEmbeddedImageFileSize;
        }

        let maxNumOfImages = this.settingsService.getNumber("MaxEmbeddedImagesNumber", Helper.defaultMaxEmbeddedImageNumber);
        maxNumOfImages = _.inRange(maxNumOfImages, 0, Infinity) ? maxNumOfImages : Helper.defaultMaxEmbeddedImageNumber;
        const localeFormatFilter = this.$filter("bpFormat") as Function;
        const localeMessage = this.localization.get("Property_Max_Images_Error", "This property exceeds the maximum number of images ({0}).");
        const dialogData: IUploadStatusDialogData = {
            files: [file],
            maxAttachmentFilesize: filesize,
            minAttachmentFilesize: 1,
            maxNumberAttachments: maxNumOfImages - numberOfExistingImages,
            maxNumberAttachmentsError: localeFormatFilter(localeMessage, maxNumOfImages),
            allowedExtentions: ["png", "jpeg", "jpg"],
            fileUploadAction: uploadFile
        };

        return this.dialogService.open(dialogSettings, dialogData).then((uploadList: IUploadStatusResult[]) => {
            if (uploadList && uploadList.length > 0) {
                const uploadedFile = uploadList[0];
                return `/${uploadedFile.url}`;
            }
        });
    }
}
