import {Message, MessageType, IMessage, IMessageService} from "../../shell";

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

