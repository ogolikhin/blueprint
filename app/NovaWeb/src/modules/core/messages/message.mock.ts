import { Message, IMessage } from "./message";
import { IMessageService} from "./message.svc";

export class MessageServiceMock implements IMessageService {

    public addMessage(msg: Message) {

    }

    public addError(text: string) {
    }

    public deleteMessageById(id: number) {

    }

    public messages: Array<IMessage>;

    public dispose() {

    }
}

