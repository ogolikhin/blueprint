import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "rx/dist/rx.lite.js";
import core from "../core";
import {AppComponent} from "./app.component";
import {AuthSvc} from "./login/auth.svc";
import {SessionSvc} from "./login/session.svc";
import {HttpErrorInterceptor} from "./login/http-error-interceptor";
import {ServerLoggerSvc} from "./log/server-logger.svc";
import {Logger} from "./log/logger.ts";
import {SessionTokenInterceptor} from "./login/session-token-interceptor";
import {ArtifactHistory} from "./bp-utility-panel/bp-history-panel/artifact-history.svc";
import {BPUtilityPanel} from "./bp-utility-panel/bp-utility-panel";
import {BPHistoryPanel} from "./bp-utility-panel/bp-history-panel/bp-history-panel";
import {BPArtifactHistoryItem} from "./bp-utility-panel/bp-history-panel/bp-artifact-history-item/bp-artifact-history-item";
import {ArtifactDiscussions} from "./bp-utility-panel/bp-discussion-panel/artifact-discussions.svc";
import {BPDiscussionPanel} from "./bp-utility-panel/bp-discussion-panel/bp-Discussions-panel";
import {BPArtifactDiscussionItem} from "./bp-utility-panel/bp-discussion-panel/bp-artifact-discussion-item/bp-artifact-discussion-item";
import {MessageDirective} from "./messages/message";
import {MessageContainerComponent} from "./messages/message-container";
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
    .service("artifactDiscussions", ArtifactDiscussions)
    .component("bpUtilityPanel", new BPUtilityPanel())
    .component("bpHistoryPanel", new BPHistoryPanel())
    .component("bpArtifactHistoryItem", new BPArtifactHistoryItem())
    .component("bpDiscussionPanel", new BPDiscussionPanel())
    .component("bpArtifactDiscussionItem", new BPArtifactDiscussionItem())
    .service("messageService", MessageService)
    .directive("message", MessageDirective.factory())
    .component("messagesContainer", new MessageContainerComponent())   
    .config(Logger)
    .config(routesConfig)
    .config(initializeInterceptors);

function initializeInterceptors($httpProvider: ng.IHttpProvider) {
    $httpProvider.interceptors.push("sessionTokenInterceptor");
    $httpProvider.interceptors.push("httpErrorInterceptor");
}
initializeInterceptors.$inject = ["$httpProvider"];

export { IServerLogger } from "./log/server-logger.svc";
export {MessageDirective, MessageContainerComponent, MessageService};
export { IMessageService } from "./messages/message.svc";
export { IMessage, Message, MessageType} from "./messages/message";