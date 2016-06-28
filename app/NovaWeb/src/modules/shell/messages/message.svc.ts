import {Message, MessageType, IMessage} from "../../shell";
import {IConfigValueHelper } from "../../core";
export {IMessage, Message, MessageType}
export interface IMessageService {
    addMessage(msg: Message): void;
    addError(text: string): void;    
    deleteMessageById(id: number): void;   
    messages: Rx.BehaviorSubject<IMessage[]>;
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
        this._messages = new Rx.BehaviorSubject<IMessage[]>([]);              
    }

    public dispose(): void {
        this.clearMessages();
    }

    private _messages: Rx.BehaviorSubject<IMessage[]>;

    public get messages(): Rx.BehaviorSubject<IMessage[]> {
        return this._messages || (this._messages = new Rx.BehaviorSubject<IMessage[]>([]));
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
            this._messages.getValue().length = 0;
        }
    }

    public addError(text: string): void {
        this.addMessage(new Message(MessageType.Error, text));
    }

    public addMessage(msg: Message): void {
        msg.id = this.id;
        this.id++;
        this._messages.getValue().push(msg);
        this.messages.onNext(this._messages.getValue());

        let messageTimeout = this.getMessageTimeout(msg.messageType);
        if (messageTimeout > 0) {
            this.timers[msg.id] = this.$timeout(this.clearMessageAfterInterval.bind(null, msg.id), messageTimeout);
        }
    }

    public deleteMessageById(id: number): void {
        let messages = this._messages.getValue();
        let i = messages.length;
        while (i--) {
            if (messages[i].id === id) {
                messages.splice(i, 1);
                this.messages.onNext(messages);
                break;
            }
        }
    }
}
