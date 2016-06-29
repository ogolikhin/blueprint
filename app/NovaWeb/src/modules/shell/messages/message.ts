export enum MessageType {
    Error = 1, Info = 2, Warning = 3
}

export interface IMessage {
    onMessageAction: (actionName: string) => void;
    id: number;
    messageType: MessageType;
    messageText: string;
}

export class Message implements IMessage {
    public onMessageAction: (actionName: string) => void;
    public id: number;

    constructor(public messageType: MessageType, public messageText: string) {
        this.messageText = messageText;
        this.messageType = messageType;
    }
}

export interface IMessageController {
    messageType: MessageType;
    closeAlert: Function;
    onMessageClosed: Function;
}

export class MessageController implements IMessageController {
    public messageType: MessageType;
    public closeAlert: Function;
    public onMessageClosed: Function;
}

export interface IMessageScope extends ng.IScope {
    messageCntrl: MessageController;    
}

export class MessageDirective implements ng.IDirective { 
    public template: string = require("./message.html");
    public restrict = "E";

    public transclude = true;

    public scope = {
        onMessageClosed: "&"     
    };

    public static factory() {
        const directive = () => new MessageDirective();
        directive["$inject"] = [];
        return directive;
    }

    constructor() {}

    public link: ng.IDirectiveLinkFn = ($scope: IMessageScope, $element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
       $scope.messageCntrl.messageType = attrs["messageType"];
    };

    public controller = MessageController;
    public controllerAs = "messageCntrl";
    public bindToController = true;
}


