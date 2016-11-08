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
    getText(text: string);
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

    public closeMessage(id: number) {
        this.messageService.deleteMessageById(id);
    }

    public getType(type: number) {
        return MessageType[type].toLowerCase();
    }

    public getText(text: string) {
        return this.$sce.trustAsHtml(this.localization.get(text));
    }
}
