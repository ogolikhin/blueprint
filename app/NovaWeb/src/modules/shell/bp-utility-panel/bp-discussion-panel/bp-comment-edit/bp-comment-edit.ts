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
    }
}
