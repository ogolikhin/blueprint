export class GlossaryState implements ng.ui.IState {
    public template = "<bp-glossary context='$content.context.artifact.id'></bp-glossary>";
    public params = { context: null };
    public controller = "glossaryStateController";
    public controllerAs = "$content";
}

export class GlossaryStateController {
    public static $inject = ["$state"];
    public context;
    constructor(
        private $state) {
        this.context = $state.params["context"];
    }
}