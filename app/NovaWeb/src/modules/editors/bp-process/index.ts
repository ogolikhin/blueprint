import {BpProcessEditor} from "./bp-process-editor";
import {ProcessService} from "./services/process/process.svc";
import * as ProcessModels from "./models/processModels";
import * as ProcessEnums from "./models/enums";

angular.module("bp.editors.process", ["ui.bootstrap"])
    .component("bpProcessEditor", new BpProcessEditor)
    .service("processService", ProcessService);

export {IProcessService} from "./services/process/process.svc";
export {
    BpProcessEditor,
    ProcessService,
    ProcessModels,
    ProcessEnums
};