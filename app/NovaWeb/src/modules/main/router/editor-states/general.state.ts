export class GeneralState implements ng.ui.IState {
    public template = "<bp-general-editor context='$content.context'></bp-general-editor>";
    public params = { context: null };
    public controller = "generalStateController";
    public controllerAs = "$content";
}

export class GeneralStateController {
    public static $inject = ["$state"];
    public context;
    constructor(
        private $state) {
        this.context = $state.params["context"];
    }
}