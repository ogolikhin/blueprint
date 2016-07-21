// References to StorytellerDiagramDirective

export class BpStorytellerEditor implements ng.IComponentOptions {
    public template: string = require("./bp-storyteller-editor.html");
    public controller: Function = BpStorytellerEditorController;
    public controllerAs = "$ctrl";
    public bindings: any = {};
    public transclude: boolean = true;
}

export class BpStorytellerEditorController {
    
    public static $inject = [
        "$rootScope",
        "$scope",
        "$state",
        "$timeout",
        "$q",
        "$log"
    ];

    public get test(): string {
        return "Storyteller Placeholder";
    }

    constructor(
        public $rootScope: ng.IRootScopeService,
        public $scope: ng.IScope,
        public $state: ng.ui.IState,
        public $timeout: ng.ITimeoutService,
        public $q: ng.IQService,
        public $log: ng.ILogService) {


    }

    public $onInit() { }
    public $onDestroy() { }
}