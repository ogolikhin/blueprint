import {IEditorParameters} from "../artifact.state";

export class GlossaryState implements ng.ui.IState {
    public template = "<bp-glossary context='$content.context'></bp-glossary>";
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