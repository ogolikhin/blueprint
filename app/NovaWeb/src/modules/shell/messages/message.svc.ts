﻿import {Message, MessageType, IMessage} from "../../shell";
import {IConfigValueHelper } from "../../core";
export {IMessage, Message, MessageType}
export interface IMessageService {
    addMessage(msg: Message): void;
    addError(text: string | Error): void;    
    deleteMessageById(id: number): void;
    messages: Array<IMessage>;
    dispose(): void;
}

export class MessageService implements IMessageService {
    private timers: { [id: number]: ng.IPromise<any>; } = {};
    private id: number = 0;

    public static $inject = ["$timeout", "configValueHelper"];
    constructor(private $timeout: ng.ITimeoutService, private configValueHelper: IConfigValueHelper) {
        this.initialize();
    }

    public initialize = () => {      
        this._messages = new Array<IMessage>();              
    }

    public dispose(): void {
        this.clearMessages();
    }

    private _messages: Array<IMessage>;

    public get messages(): Array<IMessage> {
        return this._messages || (this._messages = new Array<IMessage>());
    }

    private cancelTimer = (id: number) => {

        if (this.timers && this.timers[id]) {
            this.$timeout.cancel(this.timers[id]);
            this.timers[id] = null;
        }
    }

    private clearMessageAfterInterval = (id: number) => {
        this.deleteMessageById(id);
        this.cancelTimer(id);
    }

    private getMessageTimeout(messageType: MessageType): number {
        /**
         * Note: expect timeout settings to be a JSON formatted string:
         * {"Warning": 0,"Info": 70000,"Error": 0}
         */
        let result = 0;
        let timeout = this.configValueHelper.getStringValue("StorytellerMessageTimeout");  //TODO to change name?

        if (timeout) {
            timeout = JSON.parse(timeout);
        } else {
            // use defaults if timeout values not configured
            timeout = JSON.parse(`{ "Warning": 0, "Info": 7000, "Error": 0 }`);
        }

        switch (messageType) {
            case MessageType.Error:
                result = timeout.Error;
                break;
            case MessageType.Info:
                result = timeout.Info;
                break;
            case MessageType.Warning:
                result = timeout.Warning;
                break;
            default:
                result = 0;
                break;
        }

        return result;
    }

    private clearMessages(): void {
        if (this._messages) {
            for (var msg in this._messages) {
                this.cancelTimer(msg["id"]);
            }
            this._messages.length = 0;
        }
    }

    private findDuplicateMessages(message: IMessage): IMessage[] {
        return this.messages.filter( (msg: IMessage) => {
            return message.messageType === msg.messageType && message.messageText === msg.messageText;
        });
    }

    public addError(text: string | Error): void {
        if (text instanceof Error) {
            this.addMessage(new Message(MessageType.Error, (text as Error).message));
        } else {
            this.addMessage(new Message(MessageType.Error, text as string));
        }
    }

    public addMessage(msg: Message): void {
        // if the message of that type and with that text is already displayed, don't add another one
        if (this.findDuplicateMessages(msg).length) {
            return;
        }

        msg.id = this.id;
        this.id++;
        this._messages.push(msg);
      
        let messageTimeout = this.getMessageTimeout(msg.messageType);
        if (messageTimeout > 0) {
            this.timers[msg.id] = this.$timeout(this.clearMessageAfterInterval.bind(null, msg.id), messageTimeout);
        }
    }

    public deleteMessageById(id: number): void {      
        let i = this._messages.length;
        while (i--) {
            if (this._messages[i].id === id) {
                this._messages.splice(i, 1);
              
                break;
            }
        }
    }
}
