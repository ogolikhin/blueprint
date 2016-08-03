import "angular-ui-tinymce";

export class BPCommentEdit implements ng.IComponentOptions {
    public template: string = require("./bp-comment-edit.html");
    public controller: Function = BPCommentEditController;
    public bindings: any = {
        addButtonText: "@",
        cancelButtonText: "@",
        commentPlaceHolderText: "@",
        cancelComment: "&",
        postComment: "&",
        commentText: "@"
    };
}

export class BPCommentEditController {
    public cancelComment: Function;
    public postComment: Function;
    public addButtonText: string;
    public cancelButtonText: string;
    public commentPlaceHolderText: string;
    public commentText: string;
    public tinymceOptions = {
        plugins: "textcolor table noneditable autolink link",
        //toolbar: "fontsize | bold italic underline strikethrough | forecolor format | link",
        toolbar: "fontsize | bold italic underline | forecolor format | link",
        convert_urls: false,
        relative_urls: false,
        remove_script_host: false,
        statusbar: false,
        menubar: false,
        init_instance_callback: function (editor) { // https://www.tinymce.com/docs/configure/integration-and-setup/#init_instance_callback
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
            //editor.on("init", function () {
            //    this.getDoc().body.style.fontFamily = 'Lucida';
            //    this.getDoc().body.style.fontSize = '20';
            //});
        }
    };

    constructor() {
    }

    public callPostComment() {
        //this.postComment({ comment: this.commentText });
        this.postComment({ comment: tinymce.activeEditor.contentDocument.body.innerHTML });
    }
}