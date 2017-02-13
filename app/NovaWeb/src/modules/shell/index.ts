/*This folder will become root*/
import "rx/dist/rx.lite.js";
import {AppComponent} from "./app.component";
import {AuthSvc, IUser} from "./login/auth.svc";
import {SessionSvc, ISession} from "./login/session.svc";
import {ServerLoggerSvc} from "./log/server-logger.svc";
import {Logger} from "./log/logger";
import {SessionTokenInterceptor} from "./login/session-token-interceptor";
import {ArtifactHistory} from "./bp-utility-panel/bp-history-panel/artifact-history.svc";
import {
    RelationshipDetailsService,
    IRelationshipDetailsService
} from "./bp-utility-panel/bp-relationships-panel/bp-artifact-relationship-item/relationship-details.svc";
import {BPUtilityPanel} from "./bp-utility-panel/bp-utility-panel";
import {UtilityPanelService} from "./bp-utility-panel/utility-panel.svc";
import {BPHistoryPanel} from "./bp-utility-panel/bp-history-panel/bp-history-panel";
import {BPPropertiesPanel} from "./bp-utility-panel/bp-properties-panel/bp-properties-panel";
import {BPRelationshipsPanel} from "./bp-utility-panel/bp-relationships-panel/bp-relationships-panel";
import {BPArtifactHistoryItem} from "./bp-utility-panel/bp-history-panel/bp-artifact-history-item/bp-artifact-history-item";
import {BPArtifactRelationshipItem} from "./bp-utility-panel/bp-relationships-panel/bp-artifact-relationship-item/bp-artifact-relationship-item";
import {ArtifactDiscussions} from "./bp-utility-panel/bp-discussion-panel/artifact-discussions.svc";
import {BPDiscussionPanel} from "./bp-utility-panel/bp-discussion-panel/bp-discussions-panel";
import {BPArtifactDiscussionItem} from "./bp-utility-panel/bp-discussion-panel/bp-artifact-discussion-item/bp-artifact-discussion-item";
import {BPAttachmentsPanel} from "./bp-utility-panel/bp-attachments-panel/bp-attachments-panel";
import {BPAttachmentItem} from "./bp-utility-panel/bp-attachments-panel/bp-attachment-item/bp-attachment-item";
import {BPDocumentItem} from "./bp-utility-panel/bp-attachments-panel/bp-document-item/bp-document-item";
import {BPDiscussionReplyItem} from "./bp-utility-panel/bp-discussion-panel/bp-discussion-reply-item/bp-discussion-reply-item";
import {BPCommentEdit} from "./bp-utility-panel/bp-discussion-panel/bp-comment-edit/bp-comment-edit";
import {ErrorComponent} from "./error/error.component";
import {AppRoutes} from "./app.router";
import {MentionService} from "./bp-utility-panel/bp-discussion-panel/bp-comment-edit/mention.svc";
import "../shared/filters";
import {ILicenseService, LicenseService} from "./license/license.svc";
import {CommonModule} from "./../commonModule";
import {HeartbeatService} from "./login/heartbeat.service";
import {appRun} from "./app.run";

angular.module("app.shell", [
    "bp.filters",
    CommonModule
])
    .component("app", new AppComponent())
    .service("auth", AuthSvc)
    .service("session", SessionSvc)
    .service("sessionTokenInterceptor", SessionTokenInterceptor)
    .service("serverLogger", ServerLoggerSvc)
    .service("artifactHistory", ArtifactHistory)
    .service("relationshipDetailsService", RelationshipDetailsService)
    .service("artifactDiscussions", ArtifactDiscussions)
    .service("mentionService", MentionService)
    .service("licenseService", LicenseService)
    .service("utilityPanelService", UtilityPanelService)
    .service("heartbeatService", HeartbeatService)
    .component("bpUtilityPanel", new BPUtilityPanel())
    .component("bpHistoryPanel", new BPHistoryPanel())
    .component("bpPropertiesPanel", new BPPropertiesPanel())
    .component("bpRelationshipsPanel", new BPRelationshipsPanel())
    .component("bpArtifactHistoryItem", new BPArtifactHistoryItem())
    .component("bpArtifactRelationshipItem", new BPArtifactRelationshipItem())
    .component("bpDiscussionPanel", new BPDiscussionPanel())
    .component("bpArtifactDiscussionItem", new BPArtifactDiscussionItem())
    .component("bpAttachmentsPanel", new BPAttachmentsPanel())
    .component("bpAttachmentItem", new BPAttachmentItem())
    .component("bpDocumentItem", new BPDocumentItem())
    .component("bpDiscussionReplyItem", new BPDiscussionReplyItem())
    .component("bpCommentEdit", new BPCommentEdit())
    .component("error", new ErrorComponent())
    .config(Logger)
    .config(AppRoutes)
    .config(initializeInterceptors)
    .run(appRun);

function initializeInterceptors($httpProvider: ng.IHttpProvider) {
    $httpProvider.interceptors.push("sessionTokenInterceptor");
    $httpProvider.interceptors.push("httpErrorInterceptor");
    $httpProvider.useLegacyPromiseExtensions(false);
}

initializeInterceptors.$inject = ["$httpProvider"];

export {IUser, ISession, RelationshipDetailsService, IRelationshipDetailsService}
export {IServerLogger} from "./log/server-logger.svc";
export {ILicenseService, LicenseService} from "./license/license.svc";
