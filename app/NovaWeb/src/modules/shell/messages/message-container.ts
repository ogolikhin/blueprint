import {IMessageService, Message, MessageType} from "../../shell";

export interface IMessageContainerController {

}

export class MessageContainerController implements IMessageContainerController {
    public messages: Message[];

    public static $inject = ["messageService"];
    constructor(private messageService: IMessageService) {
        this.messages = messageService.getMessages();
    }

    public closeMessage(id: number) {
        this.messageService.deleteMessageById(id);
    }

    public destroy() {
        this.messageService.clearMessages();
    }
}

export class MessagesContainerDirective implements ng.IDirective {
    public restrict = "AE";

    public scope = {
        isPopup: "@"
    };

    public static factory() {
        const directive = ($compile: ng.ICompileService, $sce: any) => new MessagesContainerDirective($compile, $sce);
        directive["$inject"] = ["$compile", "$sce"];
        return directive;
    }
    constructor(private $compile: ng.ICompileService, private $sce: any) {
    }

    public link = ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attr: ng.IAttributes, $cntr: MessageContainerController) => {
        

        $scope.$on("$destroy", function () {
            $cntr.destroy();
        });

        $scope.$watch(() => { return $cntr.messages; }, (newVal) => {
            angular.element($element[0]).children().remove();
            for (let i = 0; i < $cntr.messages.length; i++) {
                let message = $cntr.messages[i];
                let mType = MessageType[message.messageType].toLowerCase();
                $element
                    .append(this.$compile("<message id=\"" + mType + "\"  data-message-type=\"" + mType +
                        "\"  data-on-message-closed=\"messageContainterCntrl.closeMessage(" + message.id + ")\">" +                      
                        "<div data-on-message-action=\"" + message.onMessageAction + "\"> " + this.$sce.trustAsHtml(message.messageText) + "</div>" +
                        "</message>")($scope));

            }
        }, true);
    };

    public controller = MessageContainerController;
    public controllerAs = "messageContainterCntrl";
    public bindToController = false;
}

