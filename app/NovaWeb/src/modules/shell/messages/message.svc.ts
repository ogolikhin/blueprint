import {Message, MessageType} from "../../shell";
import {IConfigValueHelper, ConfigValueHelper } from "../../core";

export interface IMessageService {
    addMessage(msg: Message): void;
    addError(text: string): void;
    getMessages(): Message[];
    clearMessages(): void;   
    deleteMessageById(id: number): void;   
}

export class MessageService implements IMessageService {
    private timers: { [id: number]: ng.IPromise<any>; } = {};
    private id: number = 0;

    public static $inject = ["$timeout", "configValueHelper"];
    constructor(private $timeout: ng.ITimeoutService, private configValueHelper: IConfigValueHelper) {
    }

    private messages: Message[] = [];

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
        }
        else {
            // use defaults if timeout values not configured
            timeout = JSON.parse('{ "Warning": 0, "Info": 7000, "Error": 0 }');
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
        for (var msg in this.messages) {
            this.cancelTimer(msg["id"]);
        }  
        this.messages.length = 0;      
    }

    public addError(text: string): void {
        this.addMessage(new Message(MessageType.Error, text));
    }

    public addMessage(msg: Message): void {
        msg.id = this.id;
        this.id++;
        this.messages.push(msg);
     
        let messageTimeout = this.getMessageTimeout(msg.messageType);
        if (messageTimeout > 0) {
            this.timers[msg.id ] = this.$timeout(this.clearMessageAfterInterval.bind(null, msg.id), messageTimeout);
        }
    }
  
    public deleteMessageById(id: number): void {
        let i = this.messages.length
        while (i--) {
            if (this.messages[i].id === id) {
                this.messages.splice(i, 1);
                break;
            }
        }
    }

    public getMessages(): Message[] {
        return this.messages;
    }

}



