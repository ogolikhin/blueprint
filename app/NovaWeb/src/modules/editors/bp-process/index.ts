import {BpProcessHeader} from "./components/header/bp-process-header";
import {BpProcessEditor} from "./bp-process-editor";
import {ProcessService} from "./services/process/process.svc";
import * as ProcessModels from "./models/processModels";
import * as ProcessEnums from "./models/enums";
import {ICommunicationManager, CommunicationManager} from "./services/communication-manager";

angular.module("bp.editors.process", ["ui.bootstrap"])
    .component("bpProcessHeader", new BpProcessHeader())
    .component("bpProcessEditor", new BpProcessEditor())
    .service("processService", ProcessService)
    .service("communicationManager", CommunicationManager);

export {IProcessService} from "./services/process/process.svc";
export {
    BpProcessHeader,
    BpProcessEditor,
    ProcessService,
    ProcessModels,
    ProcessEnums,
    ICommunicationManager, CommunicationManager
};