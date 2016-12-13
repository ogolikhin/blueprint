﻿import {Message, MessageType, IMessage} from "./message";
import {ISettingsService} from "../configuration/settings";

export interface IMessageService {

    /**
     * Given a message, displays it as a ribbon header
     * msg: Message to display.
     */
    addMessage(msg: Message): void;
    addError(text: string | Error | any, persist?: boolean): void;
    addWarning(text: string, ...params: any[]): void;
    addInfo(text: string, ...params: any[]): void;
    deleteMessageById(id: number): void;
    clearMessages(): void;
    messages: IMessage[];
    dispose(): void;
}

export class MessageService implements IMessageService {
    private timers: { [id: number]: ng.IPromise<any>; } = {};
    private id: number = 0;

    public static $inject = [
        "$timeout",
        "settings"
    ];

    constructor(private $timeout: ng.ITimeoutService,
                private settings: ISettingsService) {
        this.initialize();
    }

    public initialize = () => {
        this._messages = [];
    };

    public dispose(): void {
        this.clearMessages();
    }

    private _messages: IMessage[];

    public get messages(): IMessage[] {
        return this._messages || (this._messages = []);
    }

    private cancelTimer = (id: number) => {
        if (this.timers && this.timers[id]) {
            this.$timeout.cancel(this.timers[id]);
            this.timers[id] = null;
        }
    };

    private clearMessageAfterInterval = (id: number) => {
        this.deleteMessageById(id);
        this.cancelTimer(id);
    };

    private getMessageTimeout(messageType: MessageType): number {
        /**
         * Note: expect timeout settings to be a JSON formatted string:
         * {"Warning": 0,"Info": 5500,"Error": 0}
         */
        let result = 0;
        let timeout = this.settings.getObject("StorytellerMessageTimeout", {"Warning": 0, "Info": 5500, "Error": 0});  //TODO to change name?

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

    public clearMessages(): void {
        if (this._messages) {
            for (let index = this._messages.length - 1; index >= 0; index--) {
                let msg: IMessage = this._messages[index];
                if (!msg.persistent) {
                    this.cancelTimer(msg.id);
                    this._messages.splice(index, 1);
                }
            }
        }
    }

    private findDuplicateMessages(message: IMessage): IMessage[] {
        return this.messages.filter((msg: IMessage) => {
            return message.messageType === msg.messageType && message.messageText === msg.messageText;
        });
    }

    public addError(error: string | Error | any, persist: boolean = false): void {
        if (!error) {
            this.addMessage(new Message(MessageType.Error, "Undefined error.", persist));
        } else if (error instanceof Error) {
            this.addMessage(new Message(MessageType.Error, (error as Error).message, persist));
        } else if (error.message) {
            this.addMessage(new Message(MessageType.Error, error.message, persist));
        } else {
            this.addMessage(new Message(MessageType.Error, String(error), persist));
        }
    }

    public addWarning(msg: string, ...params: any[]): void {
        if (!msg) {
            return;
        }

        this.addMessage(new Message(MessageType.Warning, msg, false, ...params));
    }

    public addInfo(msg: string, ...params: any[]): void {
        if (!msg) {
            return;
        }
        this.addMessage(new Message(MessageType.Info, msg, true, ...params));
    }

    /**
     * Given a message, displays it as a ribbon header
     * msg: Message to display.
     * messageTimeout: Optional, default is based on msg.messageType type. Time to display message in ms. 0 = no timeout.
     */
    public addMessage(msg: Message): void {
        // if the message of that type and with that text is already displayed, don't add another one
        if (this.findDuplicateMessages(msg).length) {
            return;
        }


        msg.id = this.id;
        this.id++;
        this._messages.push(msg);
        const messageTimeout = msg.timeout || this.getMessageTimeout(msg.messageType);

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
