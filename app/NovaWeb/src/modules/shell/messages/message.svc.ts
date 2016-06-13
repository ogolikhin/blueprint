import {Message, MessageType} from "./message";

export interface IMessageService {
    addMessage(msg: Message): void;
    addError(text: string): void;
    getMessages(): Message[];
    clearMessages(): void;
}


export class MessageService implements IMessageService {
    private timer: ng.IPromise<any>;
  
    public static $inject = ["$rootScope", "$timeout"];
    constructor(private $rootScope: ng.IRootScopeService, private $timeout: ng.ITimeoutService) {
    }
  
    private messages: Message[] = [];

    private cancelTimer = () => {
        if (this.timer) {
            this.$timeout.cancel(this.timer);
            this.timer = null;
        }
    }

    private clearMessagesAfterInterval = () => {
        this.clearMessages();
        this.cancelTimer();
    }

    private getMessageTimeout(messageType: MessageType): number {
        /**
         * Note: expect timeout settings to be a JSON formatted string:
         * {"Warning": 0,"Info": 30000,"Error": 0}
         */
        var result = 0;
        var timeout = null;

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
        this.clearMessages();
        this.messages.push(msg);
        this.cancelTimer();

        var messageTimeout = this.getMessageTimeout(msg.messageType);
        if (messageTimeout > 0) {
            this.timer = this.$timeout(this.clearMessagesAfterInterval, messageTimeout);
        }
    }

    public getMessages(): Message[] {
        return this.messages;
    } 
}



