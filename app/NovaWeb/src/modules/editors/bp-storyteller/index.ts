import {BpStorytellerEditor} from "./bp-storyteller-editor";
import {ProcessService} from "./services/process/process.svc";
import * as ProcessModels from "./models/processModels";

angular.module("app.storyteller", [])
    .component("bpStorytellerEditor", new BpStorytellerEditor)
    .service("processService", ProcessService);

export {IProcessService} from "./services/process/process.svc";
export {
    BpStorytellerEditor,
    ProcessService,
    ProcessModels
};