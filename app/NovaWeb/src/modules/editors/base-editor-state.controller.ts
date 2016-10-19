export class BaseEditorStateController {
    public static $inject = ["$state"];
    public context;

    constructor(private $state: angular.ui.IStateService) {
        this.context = $state.params["context"];
    }
}
