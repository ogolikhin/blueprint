import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "rx/dist/rx.lite.js";
import core from "../core";
import {AppComponent} from "./app.component";
import {AuthSvc, IUser} from "./login/auth.svc";
import {SessionSvc, ISession} from "./login/session.svc";
import {HttpErrorInterceptor} from "./error/http-error-interceptor";
import {ServerLoggerSvc} from "./log/server-logger.svc";
import {Logger} from "./log/logger.ts";
import {SessionTokenInterceptor} from "./login/session-token-interceptor";
import {ArtifactHistory} from "./bp-utility-panel/bp-history-panel/artifact-history.svc";
import {ArtifactRelationships} from "./bp-utility-panel/bp-relationships-panel/artifact-relationships.svc";
import {BPUtilityPanel} from "./bp-utility-panel/bp-utility-panel";
import {BPHistoryPanel} from "./bp-utility-panel/bp-history-panel/bp-history-panel";
import {BPRelationshipsPanel} from "./bp-utility-panel/bp-relationships-panel/bp-relationships-panel";
import {BPArtifactHistoryItem} from "./bp-utility-panel/bp-history-panel/bp-artifact-history-item/bp-artifact-history-item";
import {BPArtifactRelationshipItem} from "./bp-utility-panel/bp-relationships-panel/bp-artifact-relationship-item/bp-artifact-relationship-item";
import {ArtifactDiscussions} from "./bp-utility-panel/bp-discussion-panel/artifact-discussions.svc";
import {BPDiscussionPanel} from "./bp-utility-panel/bp-discussion-panel/bp-discussions-panel";
import {BPArtifactDiscussionItem} from "./bp-utility-panel/bp-discussion-panel/bp-artifact-discussion-item/bp-artifact-discussion-item";
import {ArtifactAttachments} from "./bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";
import {BPAttachmentsPanel} from "./bp-utility-panel/bp-attachments-panel/bp-attachments-panel";
import {BPArtifactAttachmentItem} from "./bp-utility-panel/bp-attachments-panel/bp-artifact-attachment-item/bp-artifact-attachment-item";
import {BPArtifactDocumentItem} from "./bp-utility-panel/bp-attachments-panel/bp-artifact-document-item/bp-artifact-document-item";
import {BPDiscussionReplyItem} from "./bp-utility-panel/bp-discussion-panel/bp-discussion-reply-item/bp-discussion-reply-item";
import {BPCommentEdit} from "./bp-utility-panel/bp-discussion-panel/bp-comment-edit/bp-comment-edit";
import {MessageComponent} from "./messages/message";
import {MessageContainerComponent} from "./messages/message-container";
import {MessageService} from "./messages/message.svc";
import {Routes} from "./router/router.config";
import {ArtifactStateController} from "./router/artifact.state";
import {ErrorComponent} from "./error/error.component";
import "../editors/bp-storyteller";
import "../editors/bp-artifact";
import "../editors/bp-diagram";
import "../editors/bp-glossary";


export { IUser, ISession}
export { IServerLogger } from "./log/server-logger.svc";
export  {MessageComponent, MessageContainerComponent, MessageService};
export { IMessageService } from "./messages/message.svc";
export { IArtifactAttachment, IArtifactAttachments, IArtifactAttachmentsResultSet, IArtifactDocRef }
        from "./bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";
export { IMessage, Message, MessageType} from "./messages/message";

angular.module("app.shell",
    [
        core,
        "ui.router",
        "ui.bootstrap",
        "ngSanitize",
        "bp.editors.storyteller",
        "bp.editors.details",
        "bp.editors.glossary",
        "bp.editors.diagram",
    ])
    .component("app", new AppComponent())
    .service("auth", AuthSvc)
    .service("session", SessionSvc)
    .service("sessionTokenInterceptor", SessionTokenInterceptor)
    .service("httpErrorInterceptor", HttpErrorInterceptor)
    .service("serverLogger", ServerLoggerSvc)
    .service("artifactHistory", ArtifactHistory)
    .service("artifactRelationships", ArtifactRelationships)
    .service("artifactDiscussions", ArtifactDiscussions)
    .service("artifactAttachments", ArtifactAttachments)
    .service("messageService", MessageService)
    .component("bpUtilityPanel", new BPUtilityPanel())
    .component("bpHistoryPanel", new BPHistoryPanel())
    .component("bpRelationshipsPanel", new BPRelationshipsPanel())
    .component("bpArtifactHistoryItem", new BPArtifactHistoryItem())
    .component("bpArtifactRelationshipItem", new BPArtifactRelationshipItem())
    .component("bpDiscussionPanel", new BPDiscussionPanel())
    .component("bpArtifactDiscussionItem", new BPArtifactDiscussionItem())
    .component("bpAttachmentsPanel", new BPAttachmentsPanel())
    .component("bpArtifactAttachmentItem", new BPArtifactAttachmentItem())
    .component("bpArtifactDocumentItem", new BPArtifactDocumentItem())
    .component("bpDiscussionReplyItem", new BPDiscussionReplyItem())
    .component("bpCommentEdit", new BPCommentEdit())   
    .component("message", new MessageComponent())
    .component("messagesContainer", new MessageContainerComponent())  
    .component("error", new ErrorComponent())
    .controller("artifactStateController", ArtifactStateController)
    .config(Logger)
    .config(Routes)
    .config(initializeInterceptors);

function initializeInterceptors($httpProvider: ng.IHttpProvider) {
    $httpProvider.interceptors.push("sessionTokenInterceptor");
    $httpProvider.interceptors.push("httpErrorInterceptor");
}
initializeInterceptors.$inject = ["$httpProvider"];

