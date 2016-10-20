import "angular-ui-tinymce";
import {IMentionService, MentionService} from "./mention.svc";

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
    static $inject: [string] = ["$q", "mentionService"];

    constructor(private $q: ng.IQService, private mentionService: IMentionService) {
    }

    private commentEditor: TinyMceEditor;
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
        menubar: false,
        extended_valid_elements: MentionService.requiredAttributes,
        invalid_elements: "img,frame,iframe,script",
        invalid_styles: {
            "*": "background-image"
        },
        // https://www.tinymce.com/docs/configure/content-formatting/#font_formats
        font_formats: "Open Sans='Open Sans';Arial=Arial;Cambria=Cambria;Calibri=Calibri;Courier New='Courier New';" +
        "Times New Roman='Times New Roman';Trebuchet MS='Trebuchet MS';Verdana=Verdana;",
        mentions: this.mentionService.create(this.emailDiscussionsEnabled),
        init_instance_callback: (editor) => { // https://www.tinymce.com/docs/configure/integration-and-setup/#init_instance_callback
            this.commentEditor = editor;
            editor.focus();
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
            this.postComment({comment: this.commentEditor ? (<any>this.commentEditor).contentDocument.body.innerHTML : ""}).finally(() => {
                this.isWaiting = false;
            });
        }
    }

    public $onDestroy() {
        this.tinymceOptions.setup = null;
        this.tinymceOptions.init_instance_callback = null;
        this.tinymceOptions = null;
    }
}
