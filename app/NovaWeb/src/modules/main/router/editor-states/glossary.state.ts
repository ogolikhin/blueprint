import {IEditorParameters} from "../artifact.state";

export class GlossaryState implements ng.ui.IState {
    public template = require("./glossary.state.html");
    public params: IEditorParameters = { context: null };
    public controller = "glossaryStateController";
    public controllerAs = "$content";
}

export class GlossaryStateController {
    public static $inject = ["$state"];
    public context;
    constructor(private $state: angular.ui.IStateService) {
        this.context = $state.params["context"];
    }
}