import {BaseEditorStateController} from "./base-editor-state-controller";

export class GlossaryState implements ng.ui.IState {
    public template = require("./glossary.state.html");
    public controller = "glossaryStateController";
    public controllerAs = "$content";
}

export class GlossaryStateController extends BaseEditorStateController {
    constructor($state: angular.ui.IStateService) {
        super($state);
    }
}