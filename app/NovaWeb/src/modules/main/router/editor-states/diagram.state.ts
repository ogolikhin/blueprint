import {IEditorParameters} from "../artifact.state";
import {BaseEditorStateController} from "./base-editor-state-controller";

export class DiagramState implements ng.ui.IState {
    public template = require("./diagram.state.html");
    public params: IEditorParameters = { context: null };
    public controller = "diagramStateController";
    public controllerAs = "$content";
}

export class DiagramStateController extends BaseEditorStateController {
    constructor($state: angular.ui.IStateService) {
        super($state);
    }
}