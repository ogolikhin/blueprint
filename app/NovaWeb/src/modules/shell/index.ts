import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "rx/dist/rx.lite.js";
import core from "../core";
import {AppComponent} from "./app.component";
import {AuthSvc} from "./login/auth.svc";
import {ISession, SessionSvc} from "./login/session.svc";
import {HttpErrorInterceptor} from "./login/http-error-interceptor";
import {ServerLoggerSvc} from "./log/server-logger.svc";
import {Logger} from "./log/logger.ts";
import {SessionTokenInterceptor} from "./login/session-token-interceptor";
import {ArtifactHistory} from "./bp-utility-panel/bp-history-panel/artifact-history.svc";
import {BPUtilityPanel} from "./bp-utility-panel/bp-utility-panel";
import {BPHistoryPanel} from "./bp-utility-panel/bp-history-panel/bp-history-panel";
import {BPArtifactHistoryItem} from "./bp-utility-panel/bp-history-panel/bp-artifact-history-item/bp-artifact-history-item";
import {MessageDirective} from "./messages/message";
import {MessagesContainerDirective} from "./messages/message-container";
import {MessageService} from "./messages/message.svc";
import {config as routesConfig} from "./error.state";

angular.module("app.shell",
    [
        core,
        "ui.router",
        "ui.bootstrap",
        "ngSanitize"
    ])
    .component("app", new AppComponent())
    .service("auth", AuthSvc)
    .service("session", SessionSvc)
    .service("sessionTokenInterceptor", SessionTokenInterceptor)
    .service("httpErrorInterceptor", HttpErrorInterceptor)
    .service("serverLogger", ServerLoggerSvc)
    .service("artifactHistory", ArtifactHistory)
    .component("bpUtilityPanel", new BPUtilityPanel())
    .component("bpHistoryPanel", new BPHistoryPanel())
    .component("bpArtifactHistoryItem", new BPArtifactHistoryItem())
    .service("messageService", MessageService)
    .directive("message", MessageDirective.factory())
    .directive("messagesContainer", MessagesContainerDirective.factory())   
    .config(Logger)
    .config(routesConfig)
    .config(initializeInterceptors);

function initializeInterceptors($httpProvider: ng.IHttpProvider) {
    $httpProvider.interceptors.push("sessionTokenInterceptor");
    $httpProvider.interceptors.push("httpErrorInterceptor");
}
initializeInterceptors.$inject = ["$httpProvider"];

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

export { IServerLogger } from "./log/server-logger.svc";
export {MessageDirective, MessagesContainerDirective, MessageService};
export { IMessageService } from "./messages/message.svc";
export { Message, MessageType} from "./messages/message";