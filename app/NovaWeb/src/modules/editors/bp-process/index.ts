import {BpProcessTypeToggle} from "./components/header/bp-process-type-toggle";
import {BpProcessHeader} from "./components/header/bp-process-header";
import {BpProcessEditor} from "./bp-process-editor";
import {ProcessService} from "./services/process/process.svc";
import * as ProcessModels from "./models/process-models";
import * as ProcessEnums from "./models/enums";
import {ICommunicationManager, CommunicationManager} from "./services/communication-manager";
import {ContextualHelpDirective} from "./components/modal-dialogs/contextual-help";
import {ClearTextDirective} from "./components/modal-dialogs/clear-text";

angular.module("bp.editors.process", ["ui.bootstrap"])
    .component("bpProcessTypeToggle", new BpProcessTypeToggle())
    .component("bpProcessHeader", new BpProcessHeader())
    .component("bpProcessEditor", new BpProcessEditor())
    .service("processService", ProcessService)
    .service("communicationManager", CommunicationManager)
    .directive("contextualHelp", ContextualHelpDirective.factory())
    .directive("cleartext", () => new ClearTextDirective());

export {IProcessService} from "./services/process/process.svc";
export {
    BpProcessHeader,
    BpProcessEditor,
    ProcessService,
    ProcessModels,
    ProcessEnums,
    ICommunicationManager, CommunicationManager
};