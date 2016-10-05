import "angular-ui-tinymce";
import { IMentionService, MentionService } from "./mention.svc";


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
        toolbar: "fontsize | bold italic underline | forecolor format | link",
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
        mentions: this.mentionService.create(this.emailDiscussionsEnabled),
        init_instance_callback: function (editor) { // https://www.tinymce.com/docs/configure/integration-and-setup/#init_instance_callback
            editor.focus();
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
    };

    public callPostComment() {
        if (!this.isWaiting) {
            this.isWaiting = true;
            this.postComment({ comment: tinymce.activeEditor ? tinymce.activeEditor.contentDocument.body.innerHTML : "" }).finally(() => {
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