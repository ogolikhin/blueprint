import * as angular from "angular";
import { IMessageService, MessageService } from "./message.svc";
import { MessageComponent, IMessage, Message, MessageType } from "./message";
import { MessageContainerComponent } from "./message-container";

angular.module("bp.core.messages", ["ui.router", "ui.bootstrap"])
    .service("messageService", MessageService)
    .component("message", new MessageComponent())
    .component("messagesContainer", new MessageContainerComponent());

export { IMessageService, IMessage, MessageService, Message, MessageType }

