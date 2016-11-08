import {MessageService} from "./message.svc";
import {MessageComponent} from "./message";
import {MessageContainerComponent} from "./message-container";

angular.module("bp.core", [])
    .service("messageService", MessageService)
    .component("message", new MessageComponent())
    .component("messagesContainer", new MessageContainerComponent());
