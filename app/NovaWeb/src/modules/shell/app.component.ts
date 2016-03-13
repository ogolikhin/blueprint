import {ISession, SessionSvc} from "./login/session.svc";
import {IUser} from "./login/auth.svc";

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
    static $inject: [string] = ["session"];

    constructor(private session: ISession) {
        
    }

    public get currentUser(): IUser {
        return this.session.currentUser;
    }
}
