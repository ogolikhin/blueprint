import "angular-ui-tinymce";
import {IMentionService, MentionService} from "./mention.svc";
import {Helper} from "../../../../shared/utils/helper";

export class BPCommentEdit implements ng.IComponentOptions {
    public template: string = require("./bp-comment-edit.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPCommentEditController;
    public bindings: any = {
        addButtonText: "@",
        cancelButtonText: "@",
        commentPlaceHolderText: "@",
        cancelComment: "&",
        postComment: "&",
        commentText: "@",
        emailDiscussionsEnabled: "="
    };
}

export class BPCommentEditController {
    static $inject: [string] = [
        "$q",
        "mentionService",
        "$window",
        "$scope"
    ];

    constructor(
        private $q: ng.IQService,
        private mentionService: IMentionService,
        private $window: ng.IWindowService,
        private $scope: ng.IScope) {
    }

    private commentEditor: TinyMceEditor;
    private editorBody: HTMLElement;
    public cancelComment: Function;
    public postComment: Function;
    public addButtonText: string;
    public cancelButtonText: string;
    public commentPlaceHolderText: string;
    public commentText: string;
    public isWaiting: boolean = false;
    public emailDiscussionsEnabled: boolean;
    public tinymceOptions = {
        plugins: "textcolor table noneditable autolink link autoresize mention paste",
        autoresize_bottom_margin: 0,
        toolbar: "bold italic underline strikethrough | fontsize fontselect forecolor format | link",
        convert_urls: false,
        relative_urls: false,
        remove_script_host: false,
        statusbar: false,
        content_style: `body.mce-content-body { background: transparent; font-family: 'Open Sans', sans-serif; font-size: 12pt; min-height: 100px;}`,
        menubar: false,
        extended_valid_elements: MentionService.requiredAttributes,
        invalid_elements: "img,frame,iframe,script",
        invalid_styles: {
            "*": "background-image"
        },
        // https://www.tinymce.com/docs/configure/content-formatting/#font_formats
        font_formats: "Open Sans='Open Sans';Arial=Arial;Cambria=Cambria;Calibri=Calibri;Courier New='Courier New';" +
        "Times New Roman='Times New Roman';Trebuchet MS='Trebuchet MS';Verdana=Verdana;",
        // https://www.tinymce.com/docs/configure/content-filtering/#force_hex_style_colors
        force_hex_style_colors: true,
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
        paste_postprocess: (plugin, args) => { // https://www.tinymce.com/docs/plugins/paste/#paste_postprocess
            Helper.removeAttributeFromNode(args.node, "id");
        },
        mentions: this.mentionService.create(this.emailDiscussionsEnabled),
        init_instance_callback: (editor) => { // https://www.tinymce.com/docs/configure/integration-and-setup/#init_instance_callback
            this.commentEditor = editor;
            this.editorBody = editor.getBody() as HTMLElement;
            editor.focus();
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
            this.addDragAndDropListeners();
        },
        setup: function (editor) {
            editor.addButton("format", {
                title: "Format",
                type: "menubutton",
                text: "",
                icon: "format",
                menu: [
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
                    }]
            });
        }
    };

    public callPostComment() {
        if (!this.isWaiting) {
            this.isWaiting = true;
            this.postComment({comment: this.commentEditor ? this.commentEditor.getContent() : ""}).finally(() => {
                this.isWaiting = false;
            });
        }
    }

    // The DragEvent sequence is comprised of events being fired between the element being dragged and the drop target.
    // The order is:
    // elem.dragstart
    // target.dragenter
    // elem.drag/target.dragover (each every few hundreds milliseconds)
    // target.drop or target.dragleave (depending if the element has been dropped on the target or dragged outside)
    // elem.dragend
    // see http://www.developerfusion.com/article/144828/the-html5-drag-and-drop-api/
    private addDragAndDropListeners = () => {
        if (this.editorBody) {
            this.editorBody.addEventListener("drop", this.onDragEnd);
        }
    };

    private removeDragAndDropListeners = () => {
        if (this.editorBody) {
            this.editorBody.removeEventListener("drop", this.onDragEnd);
        }
    };

    private onDragEnd = (e: DragEvent) => {
        this.$scope.$applyAsync(() => {
            this.commentEditor.save();
            const images = this.commentEditor.getBody().getElementsByTagName("img");
            _.forEachRight(images, (image: HTMLImageElement) => {
                image.parentNode.removeChild(image);
            });
        });
    };

    public $onDestroy() {
        this.removeDragAndDropListeners();
        this.tinymceOptions.setup = null;
        this.tinymceOptions.init_instance_callback = null;
        this.tinymceOptions = null;

        // the following is to avoid TFS BUG 4776
        // The bug is caused by IE9-11 not being able to focus on other INPUT elements if the focus was
        // on a destroyed/removed from DOM element before. See also:
        // http://stackoverflow.com/questions/19581464
        // http://stackoverflow.com/questions/8978235
        let isIE11 = false;
        if (this.$window.navigator) {
            const ua = this.$window.navigator.userAgent;
            isIE11 = !!(ua.match(/Trident/) && ua.match(/rv[ :]11/)) && !ua.match(/edge/i);
        }
        if (isIE11) {
            const focusCatcher = this.$window.document.body.querySelector("input[type='text']") as HTMLElement;
            if (focusCatcher) {
                focusCatcher.focus();
                focusCatcher.blur();
            }
        }
    }
}
