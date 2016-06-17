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
        this.messageService.addMessage(new Message(1, "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in "));
        this.messageService.addMessage(new Message(2, "2"));
        this.messageService.addMessage(new Message(3, "3"));
      
    }
}