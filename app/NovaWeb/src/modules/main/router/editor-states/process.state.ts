import {IEditorParameters} from "../artifact.state";

export class ProcessState implements ng.ui.IState {
    public template = "<bp-process-editor context='$content.context'></bp-process-editor>";
    public params: IEditorParameters = { context: null };
    public controller = "processStateController";
    public controllerAs = "$content";
}

export class ProcessStateController {
    public static $inject = ["$state"];
    public context;
    constructor(private $state: angular.ui.IStateService) {
        this.context = $state.params["context"];
    }
}