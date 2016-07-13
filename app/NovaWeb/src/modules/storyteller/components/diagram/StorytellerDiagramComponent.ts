module Storyteller {
    /**
    * Storyteller Diagram Component
    *
    * This is a component version of the Storyteller diagram that can
    * be created several times on the same page. It is not hooked into
    * the header or other global objects in the Storyteller application
    * because it is intended to be a stand alone diagram.
    *
    * Note: the processId is required to load the process model from 
    * the web service. This id should be passed to the component directive
    * as an attribute of the directive.
    */

    export class StorytellerDiagramComponentController {

        public static $inject = ["processModelService", "artifactVersionControlService", "shapesFactoryService", "$log"];
        public process: IStorytellerViewModel;
        public processGraphChangedCallback: () => void;
        public needUtilityPanel = false;

        constructor(public processModelService: IProcessModelService,
                    public artifactVersionControlService: Shell.IArtifactVersionControlService,
                    public shapesFactoryService: ShapesFactoryService,
                    public $log: ng.ILogService) {
            
            // processId passed to the directive in data attribute
            var processId: string = this["data"];

            if (processId === null || typeof processId === "undefined") {
                throw new Error("Fatal: processId is null. A valid process Id " +
                    "must be passed in the data attribute of " +
                    "the directive");
            }

            if (processModelService == null) {
                throw new Error("Fatal: processModelService wasn't injected or is null");
            }

            if (shapesFactoryService == null) {
                throw new Error("Fatal: shapesFactoryService wasn't injected or is null");
            }

            // Process is always read-only in rapid review
            processModelService.load(processId, undefined, undefined, undefined, true).then((result: IProcess) => {
                this.process = new StorytellerViewModel(result);
                if (this.processGraphChangedCallback != null) {
                    this.processGraphChangedCallback();
                }
            });
        }
    }

    export class StorytellerDiagramComponent implements ng.IDirective {

        private graph: IProcessGraph;

        constructor(
            private $rootScope: ng.IRootScopeService,
            private artifactSelector: Review.ArtifactSelector) { }

        public restrict = "E";

        public scope = {
            data: "="
        };

        public link = ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $atts: ng.IAttributes, controller: StorytellerDiagramComponentController) => {
            
            controller.processGraphChangedCallback = () => {
                this.graph = this.createProcessGraph($scope, $element, $atts, controller);
            };

            let artifactId: number = controller["data"];
            this.registerGlobalFocusListener($scope, artifactId);

            $scope.$on("$destroy", () => {
                if (this.graph != null) {
                    this.graph.destroy();
                }
            });
        };

        private registerGlobalFocusListener($scope: ng.IScope, currentArtifactId: number) {
            this.artifactSelector.onArtifactIdChange($scope, (artifactId) => {

                // if selection is anything but the current graph, unselect it
                if (artifactId !== currentArtifactId.toString()) {
                    if (this.graph) {
                        this.graph.graph.clearSelection();
                    }
                }
            });
        }
        
        public createProcessGraph($scope: ng.IScope, $element: ng.IAugmentedJQuery, $atts: ng.IAttributes, controller: StorytellerDiagramComponentController): ProcessGraph {
            $scope["graphWrapper"] = $element.find(".storyteller-graph-wrapper")[0];
            $scope["graphContainer"] = $element.find(".storyteller-graph-container")[0];
            $scope["isSpa"] = $atts["isSpa"] ? $atts["isSpa"] : false;
            
            var graph = new ProcessGraph(this.$rootScope, $scope,
                controller.processModelService,
                controller.artifactVersionControlService,
                controller.process,
                controller.shapesFactoryService,
                null,
                controller.$log);
    
            graph.render(false, null);

            var utilityPanel: Shell.IPropertiesMw = this.$rootScope["propertiesSvc"]();
            
            graph.addIconRackListener((element: IProcessShape) => {
                if (utilityPanel != null) {
                    utilityPanel.openModalDialogWithInfo({
                        id: element.id,
                        containerId: element.parentId,
                        name: element.name,
                        typePrefix: element.typePrefix,
                        predefined: element.baseItemTypePredefined,
                        isDiagram: false,
                        itemStateIndicators: BluePrintSys.RC.Business.Internal.Components.RapidReview.Models.ItemIndicatorFlags.None,
                        typeId: undefined
                    });
                }
            });


            this.$rootScope.$on(BaseModalDialogController.dialogOpenEventName, () => {
                if (utilityPanel.isModalDialogOpen()) {
                    utilityPanel.closeModalDialog();
                }
            });

            return graph;
        }

        public template = "<div><div class='storyteller-graph-wrapper'><div class='storyteller-graph-container'/></div></div>";

        public controller = StorytellerDiagramComponentController;
        public controllerAs = "vm";
        public bindToController = true;

        public static directive: any[] = [
            "$rootScope",
            "artifactSelector",
            ($rootScope: ng.IRootScopeService,
                artifactSelector: Review.ArtifactSelector) => {
                return new StorytellerDiagramComponent($rootScope, artifactSelector);
            }];
    }

    var app = angular.module("Storyteller");
    app.directive("storytellerdiagramcomponent", StorytellerDiagramComponent.directive);
}
