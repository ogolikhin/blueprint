export class BaseEditorStateController {
    public static $inject = ["$state"];
    public context;
    public scrollOptions = {
        minScrollbarLength: 20,
        scrollXMarginOffset: 4,
        scrollYMarginOffset: 4
    };
    
    constructor(private $state: angular.ui.IStateService) {
        this.context = $state.params["context"];
    }
}