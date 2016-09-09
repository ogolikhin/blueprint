import {IEditorParameters} from "../artifact.state";
import {BaseEditorStateController} from "./base-editor-state-controller";

export class ProcessState implements ng.ui.IState {
    public template = require("./process.state.html");
    public params: IEditorParameters = { context: null };
    public controller = "processStateController";
    public controllerAs = "$content";
}

export class ProcessStateController extends BaseEditorStateController {
    constructor($state: angular.ui.IStateService) {
        super($state);
    }
}