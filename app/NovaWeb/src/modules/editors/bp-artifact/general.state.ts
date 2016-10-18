import {BaseEditorStateController} from "../base-editor-state.controller";

export class GeneralState implements ng.ui.IState {
    public template = require("./general.state.html");
    public controller = "generalStateController";
    public controllerAs = "$content";
    public params: any = {context: null};
}

export class GeneralStateController extends BaseEditorStateController {
    constructor($state: angular.ui.IStateService) {
        super($state);
    }
}
