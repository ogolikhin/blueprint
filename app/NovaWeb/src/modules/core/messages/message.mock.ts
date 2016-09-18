import { Message, IMessage, MessageType } from "./message";
import { IMessageService} from "./message.svc";

export class MessageServiceMock implements IMessageService {

    constructor() {
        this.messages = new Array<IMessage>();
    }

    public addMessage(msg: Message) {
        this.messages.push(msg);
    }

    public addError(text: string | Error): void {
        if (text instanceof Error) {
            this.addMessage(new Message(MessageType.Error, (text as Error).message));
        } else {
            this.addMessage(new Message(MessageType.Error, text as string));
        }
    }
    public addWarning(msg: string): void {
        if (!msg) {
            return;
        }

        this.addMessage(new Message(MessageType.Warning, msg));
    }

    public addInfo(msg: string): void {
        if (!msg) {
            return;
        }

        this.addMessage(new Message(MessageType.Info, msg));
    }

    public deleteMessageById(id: number) {
        let i = this.messages.length;
        while (i--) {
            if (this.messages[i].id === id) {
                this.messages.splice(i, 1);

                break;
            }
        }
    }

    public messages: Array<IMessage>;

    public dispose() {

    }
}

