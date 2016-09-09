import {IEditorParameters} from "../artifact.state";
import {BaseEditorStateController} from "./base-editor-state-controller";

export class ArtifactDetailsState implements ng.ui.IState {
    public template = require("./details.state.html");
    public params: IEditorParameters = { context: null };
    public controller = "detailsStateController";
    public controllerAs = "$content";
}

export class DetailsStateController extends BaseEditorStateController {
    constructor($state: angular.ui.IStateService) {
        super($state);
    }
}