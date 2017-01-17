import {IMessageService} from "./message.svc";
import {IMessage, MessageType} from "./message";
import {ILocalizationService} from "../localization/localizationService";

export class MessageContainerComponent implements ng.IComponentOptions {
    public template: string = require("./message-container.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = MessageContainerController;
    public transclude: boolean = true;
}

export interface IMessageContainerController {
    closeMessage(id: number);
    getType(type: number);
    getText(message: IMessage);
}

export class MessageContainerController implements IMessageContainerController {
    public messages: Array<IMessage>;
    public static $inject = ["messageService", "localization", "$sce"];

    constructor(private messageService: IMessageService, private localization: ILocalizationService, private $sce: any) {
        this.messages = messageService.messages;
    }

    public $onDestroy() {
        this.messageService.dispose();
    }

    public onMouseOut() {
        const container = document.querySelector(".messages__container") as HTMLElement;
        if (container) {
            container.className = "messages__container";
        }
    }

    public closeMessage(id: number) {
        this.messageService.deleteMessageById(id);
    }

    public getType(type: number) {
        return MessageType[type].toLowerCase();
    }

    public getText(message: IMessage) {
        let text: string = this.localization.get(message.messageText);
        if (message.parameters && message.parameters.length) {
            for (let i: number = 0; i < message.parameters.length; i++) {
                text = text.replace("{" + i + "}", message.parameters[i]);
            }
        }
        if (message.messageType === MessageType.LinkInfo) {
            return this.$sce.trustAsHtml(text);            
        } else {
            return this.$sce.trustAsHtml(_.escape(text));
        }
    }
}
