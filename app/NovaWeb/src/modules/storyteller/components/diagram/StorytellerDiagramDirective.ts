module Storyteller {
    /***
     * When the directive initializes create a new diagram for
     * the process identified by processId. ProcessId is passed
     * into the directive via the ui-router state params. 
     * The StorytellerDiagram class contains event handlers to 
     * refresh and destroy the diagram as needed.
     */
    export class StorytellerDiagramDirectiveController {

        public storytellerDiagram: StorytellerDiagram = null;
       // public needUtilityPanel: boolean = true;
        public processId: string = null;

        public static $inject = [
            "$rootScope",
            "$scope",
            "$state",
            "header",
            "processModelService",
            "artifactVersionControlService",
            "userstoryService",
            "selectionManager",
            "$timeout",
            "shapesFactoryService",
            "messageService",
            "storytellerCommands",
            "$q",
            "breadcrumbService",
            "bpUrlParsingService",
            "$log",
            "artifactsIndicatorInfo"
        ];

        constructor(
            public $rootScope: ng.IRootScopeService,
            public $scope: ng.IScope,
            public $state: ng.ui.IState,
            public header: IStorytellerHeader,
            public processModelService: IProcessModelService,
            public artifactVersionControlService: Shell.IArtifactVersionControlService,
            public userstoryService: IUserstoryService,
            public selectionManager: ISelectionManager,
            public $timeout: ng.ITimeoutService,
            public shapesFactoryService: ShapesFactoryService,
            public messageService: Shell.IMessageService,
            public storytellerCommands: IStorytellerCommands,
            public $q: ng.IQService,
            public breadcrumbService: Shell.IBreadcrumbService,
            public bpUrlParsingService: IBpUrlParsingService,
            public $log: ng.ILogService,
            public artifactsIndicatorInfo: Shell.ArtifactsIndicatorInfo) {

            $scope["vm"] = this;
             
            $scope.$on("$destroy", () => {
                if (this.storytellerDiagram) {
                    this.storytellerDiagram.destroy();
                    this.storytellerDiagram = null;
                }
                this.processModelService = null;
                this.userstoryService = null;
                this.selectionManager = null;

                if (this.shapesFactoryService) {
                    this.shapesFactoryService.destroy();
                    this.shapesFactoryService = null;
                }
            });

            $scope.$on(Shell.BeforeUnload.onBeforeStateChangeEvent,
                (ev: ng.IAngularEvent, confirmation: Shell.OnStateChangeConfirmation) => {                    
                    this.proceedWithSaveChangesDialog(ev, confirmation);                    
                });

            $scope.$on(
                Shell.BeforeUnload.onBeforeUnloadEvent,
                (ev: ng.IAngularEvent, confirmation: Shell.OnUnloadConfirmation) => {  
                        if (this.processModelService.isChanged) {
                            ev.preventDefault();
                            confirmation.message = this.getMessage("Save_Before_Unload_Message", "You have unsaved changes");
                        }
                   
                });
        }

        private getMessage(key: string, fallbackValue: string) {
            var labels = this.$rootScope["config"] && this.$rootScope["config"].labels ? this.$rootScope["config"].labels : null;

            if (!labels) {
                return fallbackValue;
            }

            return labels[key] || fallbackValue;
        }
   
        private proceedWithSaveChangesDialog(ev: ng.IAngularEvent, confirmation: Shell.OnStateChangeConfirmation) {
            if (confirmation && confirmation.state) {
                var defer = this.$q.defer<boolean>();
                confirmation.canChangeState = defer.promise;
                var data: ICommandData = { model: this.storytellerDiagram.storytellerViewModel, event: ev };
                if (confirmation.state.url === "/login") {
                    this.storytellerCommands.getLogoutCommand().execute(data).then(() => {
                        defer.resolve(true);
                    });
                }
                else {
                    this.storytellerCommands.getChangeStateCommand().execute(data).then(() => {
                        defer.resolve(true);
                    });
                }
            }
        }

        public parseArtifactIdFromParams(): string {
            return this.bpUrlParsingService.getStateParams().lastItemId;
        }

    }

    export class StorytellerDiagramDirective implements ng.IDirective {

        constructor() { }
        public restrict = "E";
        public scope = { };
        public templateUrl = "/Areas/Web/App/Components/Storyteller/components/diagram/StorytellerDiagramTemplate.html";
        public controller = StorytellerDiagramDirectiveController;
        public controllerAs = "vm";
        public bindToController = true;
        public terminal = true;
        public link = (
            $scope: ng.IScope,
            $element: ng.IAugmentedJQuery,
            $atts: ng.IAttributes,
            ctrl: StorytellerDiagramDirectiveController) => {

            $scope["graphWrapper"] = $element.find(".storyteller-graph-wrapper")[0];
            $scope["graphContainer"] = $element.find(".storyteller-graph-container")[0];
            // this app will always be run in SPA mode
            $scope["isSpa"] = true;

            ctrl.processId = ctrl.parseArtifactIdFromParams();
            ctrl.storytellerDiagram = new StorytellerDiagram(
                ctrl.processId,
                ctrl.$rootScope,
                ctrl.$scope,
                ctrl.header,
                ctrl.processModelService,
                ctrl.artifactVersionControlService,
                ctrl.userstoryService,
                ctrl.selectionManager,
                ctrl.$timeout,
                ctrl.shapesFactoryService,
                ctrl.messageService,
                ctrl.breadcrumbService,
                ctrl.bpUrlParsingService,
                ctrl.$log,
                ctrl.artifactsIndicatorInfo);

            ctrl.storytellerDiagram.createDiagram(ctrl.processId);
        };

        public static factory(): ng.IDirective {
            return new StorytellerDiagramDirective();
        }
    }

    angular.module("Storyteller").directive("storytellerdiagram", StorytellerDiagramDirective.factory);
}
