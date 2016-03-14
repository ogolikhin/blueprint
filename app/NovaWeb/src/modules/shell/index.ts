import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "bootstrap/dist/css/bootstrap.css"
import "../core";
import {AppComponent} from "./app.component";
import {AuthSvc} from "./login/auth.svc";
import {ISession, SessionSvc} from "./login/session.svc";

angular.module("app.shell",
    [
        "app.core",
        "ui.router",
        "ui.bootstrap"
    ])
    .component("app", new AppComponent())
    .service("auth", AuthSvc)
    .service("session", SessionSvc);

//TODO: move to other file
export class AuthenticationRequired {
    private static key = "authenticated";
    public resolve = {};

    constructor() {
        this.resolve[AuthenticationRequired.key] = [
            "$log", "session", ($log: ng.ILogService, session: ISession): ng.IPromise<any> => {
                $log.debug("AuthenticationRequired...called");
                return session.ensureAuthenticated();
            }
        ];
    }
}