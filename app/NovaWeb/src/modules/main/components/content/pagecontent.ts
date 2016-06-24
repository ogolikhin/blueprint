import {IProjectManager, Models} from "../..";
import { IMessageService, Message } from "../../../shell";


export class PageContent implements ng.IComponentOptions {
    public template: string = require("./pagecontent.html");

    public controller: Function = PageContentCtrl;
    public controllerAs = "$content";
    public bindings: any = {
        viewState: "<",
    };

}

class PageContentCtrl {
    public static $inject: [string] = ["messageService"];
    constructor(private messageService: IMessageService) {
    }
    //TODO remove after testing
    public addMsg() {
        //temporary removed to toolbar component under "Refresh" button
    }
    public viewState: boolean;
}