import {BaseEditorStateController} from "./base-editor-state-controller";

export class ArtifactDetailsState implements ng.ui.IState {
    public template = require("./details.state.html");
    public controller = "detailsStateController";
    public controllerAs = "$content";
    public params: any = {context: null};
}

export class DetailsStateController extends BaseEditorStateController {
    constructor($state: angular.ui.IStateService) {
        super($state);
    }
}
