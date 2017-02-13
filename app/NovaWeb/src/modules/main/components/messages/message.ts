export enum MessageType {
    Error = 1,
    Info = 2,
    Warning = 3,
    Lock = 4,
    Deleted = 5,
    LinkInfo = 6
}

export interface IMessage {
    onMessageAction: (actionName: string) => void;
    id: number;
    messageType: MessageType;
    messageText: string;
    persistent?: boolean;
    timeout?: number;
    canBeClosedManually: boolean;
    parameters?: any[];
}

export class Message implements IMessage {
    public onMessageAction: (actionName: string) => void;
    public id: number;
    public timeout: number;
    public canBeClosedManually: boolean;
    public parameters: any[];

    constructor(public messageType: MessageType, public messageText: string, public persistent: boolean = false, ...params: any[]) {
        if (params.length) {
            this.parameters = params;
        }
        this.canBeClosedManually = messageType === MessageType.Error || messageType === MessageType.Warning;
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

    public onMouseOut() {
        const container = document.getElementsByClassName("messages__container").item(0) as HTMLElement;
        if (container) {
            container.className = "messages__container";
        }
    }

    public onMouseOver(messageType: string) {
        const container = document.getElementsByClassName("messages__container").item(0) as HTMLElement;
        if (container) {
            container.classList.add("messages__container--over-" + messageType);
        }
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
