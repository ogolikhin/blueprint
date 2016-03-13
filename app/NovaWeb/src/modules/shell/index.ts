import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "bootstrap/dist/css/bootstrap.css"
import "../core";
import {AppComponent} from "./app.component";
import {AuthSvc} from "./login/auth.svc";
import {ILogin, LoginSvc} from "./login/login.svc";

angular.module("app.shell",
    [
        "app.core",
        "ui.router",
        "ui.bootstrap"
    ])
    .component("app", new AppComponent())
    .service("auth", AuthSvc)
    .service("login", LoginSvc);
    //.config(routesConfig);

//TODO: move to other files
export class AuthenticationRequired {
    private static key = "authenticated";
    public resolve = {};

    constructor() {
        this.resolve[AuthenticationRequired.key] = [
            "$log", "login", ($log: ng.ILogService, login: ILogin): ng.IPromise<any> => {
                $log.debug("AuthenticationRequired...called");
                return login.ensureAuthenticated();
            }
        ];
    }
}