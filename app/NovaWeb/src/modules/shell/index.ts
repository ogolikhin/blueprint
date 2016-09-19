import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "rx/dist/rx.lite.js";
import "../core";
import {AppComponent} from "./app.component";
import {AuthSvc, IUser} from "./login/auth.svc";
import {SessionSvc, ISession} from "./login/session.svc";
import {HttpErrorInterceptor} from "./error/http-error-interceptor";
import {ServerLoggerSvc} from "./log/server-logger.svc";
import {Logger} from "./log/logger.ts";
import {SessionTokenInterceptor} from "./login/session-token-interceptor";
import {ArtifactHistory} from "./bp-utility-panel/bp-history-panel/artifact-history.svc";
import {RelationshipDetailsService, IRelationshipDetailsService} from "./bp-utility-panel/bp-relationships-panel/bp-artifact-relationship-item/relationship-details.svc";
import {BPUtilityPanel} from "./bp-utility-panel/bp-utility-panel";
import {BPHistoryPanel} from "./bp-utility-panel/bp-history-panel/bp-history-panel";
import {BPPropertiesPanel} from "./bp-utility-panel/bp-properties-panel/bp-properties-panel";
import {BPRelationshipsPanel} from "./bp-utility-panel/bp-relationships-panel/bp-relationships-panel";
import {BPArtifactHistoryItem} from "./bp-utility-panel/bp-history-panel/bp-artifact-history-item/bp-artifact-history-item";
import {BPArtifactRelationshipItem} from "./bp-utility-panel/bp-relationships-panel/bp-artifact-relationship-item/bp-artifact-relationship-item";
import {ArtifactDiscussions} from "./bp-utility-panel/bp-discussion-panel/artifact-discussions.svc";
import {BPDiscussionPanel} from "./bp-utility-panel/bp-discussion-panel/bp-discussions-panel";
import {BPArtifactDiscussionItem} from "./bp-utility-panel/bp-discussion-panel/bp-artifact-discussion-item/bp-artifact-discussion-item";
import {BPAttachmentsPanel} from "./bp-utility-panel/bp-attachments-panel/bp-attachments-panel";
import {BPArtifactAttachmentItem} from "./bp-utility-panel/bp-attachments-panel/bp-artifact-attachment-item/bp-artifact-attachment-item";
import {BPArtifactDocumentItem} from "./bp-utility-panel/bp-attachments-panel/bp-artifact-document-item/bp-artifact-document-item";
import {BPDiscussionReplyItem} from "./bp-utility-panel/bp-discussion-panel/bp-discussion-reply-item/bp-discussion-reply-item";
import {BPCommentEdit} from "./bp-utility-panel/bp-discussion-panel/bp-comment-edit/bp-comment-edit";
import {ErrorComponent} from "./error/error.component";
import {Routes} from "./router/router.config";
import {UsersAndGroupsService} from "./bp-utility-panel/bp-discussion-panel/bp-comment-edit/users-and-groups.svc";
import {MentionService} from "./bp-utility-panel/bp-discussion-panel/bp-comment-edit/mention.svc";
import "../shared/filters";

export { IUser, ISession, RelationshipDetailsService, IRelationshipDetailsService }
export { IServerLogger } from "./log/server-logger.svc";
export { IMessageService, IMessage, MessageType, MessageService, Message, } from "../core";

angular.module("app.shell",
    [
        "app.core",
        "ui.router",
        "ui.bootstrap",
        "ngSanitize",
        "bp.filters"
    ])
    .component("app", new AppComponent())
    .service("auth", AuthSvc)
    .service("session", SessionSvc)
    .service("sessionTokenInterceptor", SessionTokenInterceptor)
    .service("httpErrorInterceptor", HttpErrorInterceptor)
    .service("serverLogger", ServerLoggerSvc)
    .service("artifactHistory", ArtifactHistory)
    .service("relationshipDetailsService", RelationshipDetailsService)
    .service("artifactDiscussions", ArtifactDiscussions)
    .service("mentionService", MentionService)
    .service("usersAndGroupsService", UsersAndGroupsService)
    .component("bpUtilityPanel", new BPUtilityPanel())
    .component("bpHistoryPanel", new BPHistoryPanel())
    .component("bpPropertiesPanel", new BPPropertiesPanel()) 
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
    .component("error", new ErrorComponent())
    .config(Logger)
    .config(Routes)
    .config(initializeInterceptors);

function initializeInterceptors($httpProvider: ng.IHttpProvider) {
    $httpProvider.interceptors.push("sessionTokenInterceptor");
    $httpProvider.interceptors.push("httpErrorInterceptor");
    $httpProvider.useLegacyPromiseExtensions(false);
}
initializeInterceptors.$inject = ["$httpProvider"];

