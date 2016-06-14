import {IMessageService, MessageService, Message, MessageType} from "../../shell";

export interface IMessageContainerController {

}

export class MessageContainerController implements IMessageContainerController {
    public messages: any;
    public nmbrOfMessages: number;
    public hasMessages: (nType: MessageType) => boolean;

    public static $inject = ["messageService"];
    constructor(private messageService: IMessageService) {
        this.messages = this.messageService.getMessages();

        this.hasMessages = (mType) => {
            this.nmbrOfMessages = this.countsMessageByType(mType);
            return this.nmbrOfMessages > 0;
        };
    }

    private countsMessageByType(messageType: MessageType) {
        var count = 0;
        for (var i = 0; i < this.messages.length; i++) {
            if (this.messages[i].messageType === messageType) {
                count = count + 1;
                if (count > 1) {
                    break;
                }
            }
        }
        return count;
    }

    public closeMessages(messageType: MessageType) {
        for (var i = this.messages.length - 1; i >= 0; i--) {
            if (this.messages[i].messageType === messageType) {
                this.messages.splice(i, 1);
            }
        }
    }
}

export class MessagesContainerDirective implements ng.IDirective {
    public restrict = "AE";

    public scope = {      
        isPopup: "@"
    };

    public static directive: any[] = [
        "$compile",     
        ($compile: ng.ICompileService) => new MessagesContainerDirective($compile)];

    constructor(private $compile: ng.ICompileService) {
    }

    public link = ($scope: ng.IScope, $element: ng.IAugmentedJQuery) => {       
        for (var i = 1; i <= 3; i++) {
            var mType = MessageType[i].toLowerCase();
            $element
                .append(this.$compile("<message id=\"" + mType + "\" data-ng-if=\"messageContainterCntrl.hasMessages(" + i + ")\" data-message-type=\"" + mType + "\"  data-on-message-closed=\"messageContainterCntrl.closeMessages(" + i + ")\">" +
                    "<ul ng-class=\"{nobullets: results.length < 2}\" >" +
                    "<li data-ng-repeat=\"m in messageContainterCntrl.messages | filter:{messageType:" + i + "} as results\">" +
                    "<div data-bp-compile-html=\"m.messageText\" data-on-message-action=\"m.onMessageAction\"></div>" +
                    "</li>" +
                    "</ul>" +
                    "</message>")($scope));
        }
    };

    public controller = MessageContainerController;
    public controllerAs = "messageContainterCntrl";
    public bindToController = false;
}

