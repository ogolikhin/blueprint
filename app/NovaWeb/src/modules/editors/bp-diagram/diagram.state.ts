import {BaseEditorStateController} from "../base-editor-state.controller";

export class DiagramState implements ng.ui.IState {
    public template = require("./diagram.state.html");
    public controller = "diagramStateController";
    public controllerAs = "$content";
    public params: any = {context: null};
}

export class DiagramStateController extends BaseEditorStateController {
    constructor($state: angular.ui.IStateService) {
        super($state);
    }
}
