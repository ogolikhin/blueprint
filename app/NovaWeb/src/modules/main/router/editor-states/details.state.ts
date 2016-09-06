import {IEditorParameters} from "../artifact.state";

export class ArtifactDetailsState implements ng.ui.IState {
    public template = require("./details.state.html");
    public params: IEditorParameters = { context: null };
    public controller = "detailsStateController";
    public controllerAs = "$content";
}

export class DetailsStateController {
    public static $inject = ["$state"];
    public context;
    constructor(private $state: angular.ui.IStateService) {
        this.context = $state.params["context"];
    }
}