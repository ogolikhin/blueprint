import { ILocalizationService } from "../../../../core";
import { Models } from "../../../../main";

export class BPCommentEdit implements ng.IComponentOptions {
    public template: string = require("./bp-comment-edit.html");
    public controller: Function = BPCommentEditController;
    public bindings: any = {
        addButtonText: "@",
        cancelButtonText: "@",
        commentPlaceHolderText: "@",
        cancelComment: "&"
    };
}

export class BPCommentEditController {
    public cancelComment: Function;
    public addButtonText: string;
    public cancelButtonText: string;
    public commentPlaceHolderText: string;

    constructor() {
        tinymce.baseURL = "../novaweb/libs/tinymce";
        setTimeout(() => {
            tinymce.init({
                mode: 'textareas',
                toolbar: 'styleselect | bold italic underline | link image',
                menu: {
                },
                height: "80px"
            });
        });
    }
}
