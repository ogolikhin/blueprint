import { ILocalizationService } from "../../../../core";
import { Models } from "../../../../main";

export class BPDiscussionReplyItem implements ng.IComponentOptions {
    public template: string = require("./bp-discussion-reply-item.html");
    public controller: Function = BPDiscussionReplyItemController;
    public bindings: any = {
        replyInfo: "="
    };
}

export class BPDiscussionReplyItemController {
    constructor() {
    }
}
