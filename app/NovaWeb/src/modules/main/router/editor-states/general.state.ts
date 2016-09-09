import {IEditorParameters} from "../artifact.state";
import {BaseEditorStateController} from "./base-editor-state-controller";

export class GeneralState implements ng.ui.IState {
    public template = require("./general.state.html");
    public params: IEditorParameters = { context: null };
    public controller = "generalStateController";
    public controllerAs = "$content";
}

export class GeneralStateController extends BaseEditorStateController {
    constructor($state: angular.ui.IStateService) {
        super($state);
    }
}