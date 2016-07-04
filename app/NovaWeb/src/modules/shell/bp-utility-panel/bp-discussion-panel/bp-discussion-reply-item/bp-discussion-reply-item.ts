import { ILocalizationService } from "../../../../core";

export class BPDiscussionReplyItem implements ng.IComponentOptions {
    public template: string = require("./bp-discussion-reply-item.html");
    public controller: Function = BPDiscussionReplyItemController;
    public bindings: any = {
        replyInfo: "="
    };
}

export class BPDiscussionReplyItemController {

    public static $inject: [string] = [
        "localization"
    ];

    constructor(private localization: ILocalizationService) {
    }
}
