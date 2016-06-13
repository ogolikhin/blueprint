import {IMessageService} from "../../../shell/messages/message.svc";
import {Message} from "../../../shell/messages/message";

export class PageContent implements ng.IComponentOptions {
    public template: string = require("./pagecontent.html");

    public controller: Function = PageContentCtrl;
    public controllerAs = "pageContentCtrl";
}

class PageContentCtrl {
    public static $inject: [string] = ["messageService"];
    constructor(private messageService: IMessageService) {
    }
    //TODO remove after testing
    public addMsg() {
        this.messageService.addMessage(new Message(3,"warning"));
    }
}