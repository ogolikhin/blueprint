import {IMessageService} from "./message.svc";
import {IMessage, MessageType} from "./message";
import {ILocalizationService} from "../../../commonModule/localization/";
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
        const container = document.getElementsByClassName("messages__container").item(0) as HTMLElement;
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

        const messageText = message.messageType !== MessageType.LinkInfo ? _.escape(text) : text;

        return this.$sce.trustAsHtml(messageText);
    }
}
