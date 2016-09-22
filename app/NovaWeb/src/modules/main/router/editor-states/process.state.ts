import {BaseEditorStateController} from "./base-editor-state-controller";

export class ProcessState implements ng.ui.IState {
    public template = require("./process.state.html");
    public controller = "processStateController";
    public controllerAs = "$content";
    public params: any = { context: null };
}

export class ProcessStateController extends BaseEditorStateController {
    constructor($state: angular.ui.IStateService) {
        super($state);
    }
}