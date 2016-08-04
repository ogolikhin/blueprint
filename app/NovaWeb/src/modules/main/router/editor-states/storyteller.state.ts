
export class StorytellerState implements ng.ui.IState {
    public template = "<bp-storyteller-editor context='$content.context.artifact.id'></bp-storyteller-editor>";
    public params = { context: null };
    public controller = "storytellerStateController";
    public controllerAs = "$content";
}

export class StorytellerStateController {
    public static $inject = ["$state"];
    public context;
    constructor(
        private $state) {
        this.context = $state.params["context"];
    }
}