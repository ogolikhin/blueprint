import {Message, MessageType} from "../../shell";

export interface IMessageService {
    addMessage(msg: Message): void;
    addError(text: string): void;
    getMessages(): Message[];
    clearMessages(): void;
    deleteMessages(messageType: MessageType): void;
    getFirstOfTypeMessage(messageType: MessageType): Message;
    countsMessageByType(messageType: MessageType): number;
    hasMessages(messageType: MessageType): boolean;
}

export class MessageService implements IMessageService {
    private timers: ng.IPromise<any>[] = [];

    public static $inject = ["$rootScope", "$timeout"];
    constructor(private $rootScope: ng.IRootScopeService, private $timeout: ng.ITimeoutService) {
    }

    private messages: Message[] = [];

    private cancelTimer = (messageType: MessageType) => {
        //if (this.timer) {
        //    this.$timeout.cancel(this.timer);
        //    this.timer = null;
        //}
    }

    private clearMessagesAfterInterval = (messageType: MessageType) => {
        this.deleteMessages(messageType);
        this.cancelTimer(messageType);
    }

    private getMessageTimeout(messageType: MessageType): number {
        /**
         * Note: expect timeout settings to be a JSON formatted string:
         * {"Warning": 0,"Info": 30000,"Error": 0}
         */
        let result = 0;
        let timeout = null;

        //TODO
        //if (this.$rootScope["config"] && this.$rootScope["config"]["settings"]) {
        //    timeout = this.$rootScope["config"]["settings"]["StorytellerMessageTimeout"];
        //}
        if (timeout) {
            timeout = JSON.parse(timeout);
        }
        else {
            // use defaults if timeout values not configured
            timeout = JSON.parse('{ "Warning": 0, "Info": 3000, "Error": 0 }');
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

    public clearMessages(): void {
        this.messages.length = 0;
    }

    public addError(text: string): void {
        this.addMessage(new Message(MessageType.Error, text));
    }

    public addMessage(msg: Message): void {       
        this.messages.push(msg);
       // this.cancelTimer();

        let messageTimeout = this.getMessageTimeout(msg.messageType);
        if (messageTimeout > 0) {           
            this.timers.push(this.$timeout(this.clearMessagesAfterInterval.bind(null, msg.messageType), messageTimeout));
        }
    }

    public deleteMessages(messageType: MessageType): void {
        let i = this.messages.length
        while (i--) {       
            if (this.messages[i].messageType === messageType) {
                this.messages.splice(i, 1);

            }
        }
    }

    public getMessages(): Message[] {
        return this.messages;
    }

    public getFirstOfTypeMessage(messageType: MessageType): Message {
        for (let i = 0; i< this.messages.length; i++) {
            if (this.messages[i].messageType === messageType) {
                return this.messages[i];
            }
        }
        return null;
    }

    public countsMessageByType(messageType: MessageType): number {
        let count = 0;
        for (let i = 0; i < this.messages.length; i++) {
            if (this.messages[i].messageType === messageType) {
                count = count + 1;
                if (count > 1) {
                    break;
                }
            }
        }
        return count;
    }

    public hasMessages(messageType: MessageType): boolean {
        return this.countsMessageByType(messageType) > 0;
    };
}



