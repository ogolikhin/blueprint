export enum MessageType {
    Error = 1,
    Info = 2,
    Warning = 3,
    Lock = 4,
    Deleted = 5
}

export interface IMessage {
    onMessageAction: (actionName: string) => void;
    id: number;
    messageType: MessageType;
    messageText: string;
    persistent?: boolean;
}

export class Message implements IMessage {
    public onMessageAction: (actionName: string) => void;
    public id: number;

    constructor(public messageType: MessageType, public messageText: string, public persistent: boolean = false) {
        this.messageText = messageText;
        this.messageType = messageType;
        this.persistent = persistent;
    }
}

export interface IMessageController {
    messageType: string;
    onMessageClosed: Function;
}

export class MessageController implements IMessageController {
    public messageType: string;
    public onMessageClosed: Function;
    public canBeClosed: boolean; 
    
    constructor() {
        this.canBeClosed = this.messageType === "error" || this.messageType === "warning";
    }
}

export class MessageComponent implements ng.IComponentOptions {
    public template: string = require("./message.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = MessageController;
    public transclude: boolean = true;
    public bindings: any = {
        onMessageClosed: "&",
        messageType: "@"
    };
}
