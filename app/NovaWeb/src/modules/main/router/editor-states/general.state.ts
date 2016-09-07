import {IEditorParameters} from "../artifact.state";

export class GeneralState implements ng.ui.IState {
    public template = require("./general.state.html");
    public params: IEditorParameters = { context: null };
    public controller = "generalStateController";
    public controllerAs = "$content";
}

export class GeneralStateController {
    public static $inject = ["$state"];
    public context;
    constructor(private $state: angular.ui.IStateService) {
        this.context = $state.params["context"];
    }
}