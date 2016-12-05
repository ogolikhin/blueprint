import * as angular from "angular";
import {BpProcessHeader} from "./components/header/bp-process-header";
import {BpProcessEditor} from "./bp-process-editor";
import {ProcessService} from "./services/process.svc";
import {BreadcrumbService} from "./services/breadcrumb.svc";
import {UserStoryService} from "./services/user-story.svc";
import * as ProcessModels from "./models/process-models";
import * as ProcessEnums from "./models/enums";
import {ICommunicationManager, CommunicationManager} from "./services/communication-manager";
import {ContextualHelpDirective} from "./components/modal-dialogs/contextual-help";
import {ClearTextDirective} from "./components/modal-dialogs/clear-text";
import {UploadImageDirective} from "./components/modal-dialogs/upload-image";
import {PreviewCenterComponent} from "./components/modal-dialogs/user-story-preview/preview-center";
import {PreviewWingDirective} from "./components/modal-dialogs/user-story-preview/preview-wing";
import {ZoomableImageDirective} from "./components/modal-dialogs/user-story-preview/zoomable-image";
import {DecisionEditor} from "./components/modal-dialogs/decision-editor";
import {UserTaskEditor, SystemTaskEditor} from "./components/modal-dialogs/task-editor";
import {ShapesFactory} from "./components/diagram/presentation/graph/shapes/shapes-factory";
import {ClipboardService} from "./services/clipboard.svc";

angular.module("bp.editors.process", ["ui.bootstrap"])
    .component("bpProcessHeader", new BpProcessHeader())
    .component("bpProcessEditor", new BpProcessEditor())
    .component("previewCenter", new PreviewCenterComponent())
    .component("decisionEditor", new DecisionEditor())
    .component("userTaskEditor", new UserTaskEditor())
    .component("systemTaskEditor", new SystemTaskEditor())
    .directive("previewWing", PreviewWingDirective.directive)
    .directive("zoomableImage", ZoomableImageDirective.directive)
    .service("processService", ProcessService)
    .service("shapesFactory", ShapesFactory)
    .service("breadcrumbService", BreadcrumbService)
    .service("communicationManager", CommunicationManager)
    .service("userStoryService", UserStoryService)
    .service("clipboardService", ClipboardService)
    .directive("contextualHelp", ContextualHelpDirective.factory())
    .directive("cleartext", () => new ClearTextDirective())
    .directive("uploadImage", UploadImageDirective.factory());

export {IProcessService} from "./services/process.svc";
export {
    BpProcessHeader,
    BpProcessEditor,
    ProcessService,
    ProcessModels,
    ProcessEnums,
    ICommunicationManager,
    CommunicationManager,
    ShapesFactory
};
