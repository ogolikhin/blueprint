import {ISession} from "./login/session.svc";
import {IUser} from "./login/auth.svc";
import {IConfigValueHelper} from "../core/config.value.helper";

export class AppComponent implements ng.IComponentOptions {
    // Inline template
    //public template: string = "<ui-view></ui-view>";

    // Template will be injected on the build time
    public template: string = require("./app.html");

    // 'External' template should ends with *.view.html to be copied to the dest folder
    //public templateUrl: string = "/modules/application/app.view.html"

    public controller: Function = AppController;
    public controllerAs = "app";
}

export class AppController {
    static $inject: [string] = ["$state", "session", "configValueHelper"];

    constructor(private $state: ng.ui.IStateService, private session: ISession, private configValueHelper: IConfigValueHelper) {
    }

    public get currentUser(): IUser {
        return this.session.currentUser;
    }

    public logout(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.session.logout().finally(() => this.$state.reload());
    }

    public navigateToHelpUrl(evt: ng.IAngularEvent) {
        evt.preventDefault();

        //We want to open a new window, not a tab, to match old Silverlight behaviour.
        //Note: Hiding the URL bar is no longer possible in most browsers (security feature).
        window.open(this.configValueHelper.getStringValue("HelpURL"), "_blank", "menubar=no, location=no, resizable=yes, scrollbars=yes, titlebar=no");
    }
}
