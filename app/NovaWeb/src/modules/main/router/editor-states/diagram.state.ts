import {IEditorParameters} from "../artifact.state";

export class DiagramState implements ng.ui.IState {
    public template = require("./diagram.state.html");
    public params: IEditorParameters = { context: null };
    public controller = "diagramStateController";
    public controllerAs = "$content";
}

export class DiagramStateController {
    public static $inject = ["$state"];
    public context;
    constructor(private $state: angular.ui.IStateService) {
        this.context = $state.params["context"];
    }
}