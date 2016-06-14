import {IMessageService, MessageService, Message, MessageType} from "../../shell";

export interface IMessageContainerController {

}

export class MessageContainerController implements IMessageContainerController {
    public hasMessages: (nType: MessageType) => boolean;

    public static $inject = ["messageService"];
    constructor(private messageService: IMessageService) {
        this.hasMessages = (mType) => {
            return this.messageService.hasMessages(mType);
        };
    }

    private countsMessageByType(messageType: MessageType) {
        return this.messageService.countsMessageByType(messageType);
    }

    public closeMessages(messageType: MessageType) {     
        this.messageService.deleteMessages(messageType);
    }

    public getFirstOfTypeMessage(messageType: MessageType): Message {
        return this.messageService.getFirstOfTypeMessage(messageType);
    }
}

export class MessagesContainerDirective implements ng.IDirective {
    public restrict = "AE";

    public scope = {      
        isPopup: "@"
    };

    public static factory() {
        const directive = ($compile: ng.ICompileService) => new MessagesContainerDirective($compile);
        directive["$inject"] = ["$compile"];
        return directive;
    }
    constructor(private $compile: ng.ICompileService) {
    }

    public link = ($scope: ng.IScope, $element: ng.IAugmentedJQuery) => {       
        for (let i = 1; i <= 3; i++) {
            let mType = MessageType[i].toLowerCase();
            $element
                .append(this.$compile("<message id=\"" + mType + "\" data-ng-if=\"messageContainterCntrl.hasMessages(" + i + ")\" data-message-type=\"" + mType +
                    "\"  data-on-message-closed=\"messageContainterCntrl.closeMessages(" + i + ")\">" +
                    "<div data-bp-compile-html=\"messageContainterCntrl.getFirstOfTypeMessage(" + i + ").messageText\"" +
                    "data-on-message-action=\"messageContainterCntrl.getFirstOfTypeMessage(" + i + ").onMessageAction\"></div>" +
                    "</message>")($scope));
        }
    };

    public controller = MessageContainerController;
    public controllerAs = "messageContainterCntrl";
    public bindToController = false;
}

