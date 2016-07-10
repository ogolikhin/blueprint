module Storyteller {
    export class StorytellerToolbarDirectiveController {
        
        public static $inject = ["$element", "$scope", "$rootScope", "processModelService", "artifactVersionControlService", "userstoryService", "messageService", "selectionManager", "dialogService", "$window", "$uibModal"];
        private generateCommand;
        public deleteCommand: DeleteCommand = null;
        public saveProcessCommand: SaveProcessCommand = null;
        public publishProcessCommand: PublishProcessCommand = null;
        public discardChangesCommand: DiscardChangesCommand = null;
        private selectedNodes: Array<IDiagramNode>;
        private unsubscribeToolbarEvents = [];
        private isToolbarEnabled: boolean;

        constructor(private $element: JQuery,
            private $scope: IStorytellerToolbarDirectiveScope,
            private $rootScope,
            private processModelService: IProcessModelService,
            private artifactVersionControlService: Shell.IArtifactVersionControlService,
            private userstoryService: IUserstoryService,
            private messageService: Shell.IMessageService,
            private selectionManager: ISelectionManager,
            private dialogService: Shell.IDialogService,
            private $window,
            private $uibModal: angular.ui.bootstrap.IModalService) {
            
            // using controllerAs syntax
            $scope.vm = this;
                        
            // create toolbar commands 
            this.deleteCommand = new DeleteCommand($rootScope, $scope, selectionManager, dialogService);
            this.generateCommand = new GenerateUserStoryCommand($rootScope, this.processModelService, this.userstoryService, this.dialogService, this.messageService, this.artifactVersionControlService);
            this.saveProcessCommand = new SaveProcessCommand($rootScope, $scope, processModelService);
            this.publishProcessCommand = new PublishProcessCommand($rootScope, $scope, processModelService);
            this.discardChangesCommand = new DiscardChangesCommand($rootScope, $scope, processModelService, dialogService, messageService);

            this.subscribeToToolbarEvents();

            $rootScope.$on("disableStorytellerToolbar", () => {
                this.isToolbarEnabled = false;
            });
            $rootScope.$on("enableStorytellerToolbar", () => {
                this.isToolbarEnabled = true;
            });
        }

        private subscribeToToolbarEvents() {
            if (this.$scope.subscribe) {
                if (this.unsubscribeToolbarEvents.length > 0) {
                    // remove previous event listeners 
                    this.removeToolbarEventListeners();
        }
                // subscribe to the SelectionChanged event 

                this.unsubscribeToolbarEvents.push(
                    this.$scope.subscribe("SelectionManager:SelectionChanged", (event, elements) => {
                        this.selectedNodes = elements;
                    })
                );
            }
        }

        private removeToolbarEventListeners() {

            if (this.unsubscribeToolbarEvents.length > 0) {
                for (var i = 0; i < this.unsubscribeToolbarEvents.length; i++) {
                    this.unsubscribeToolbarEvents[i]();
                    this.unsubscribeToolbarEvents[i] = null;
                }
            }
            this.unsubscribeToolbarEvents = [];
        }

        public generateUserstories(generateAll: boolean) {
            (generateAll) ? this.generateCommand.execute(null) : this.generateCommand.execute(this.selectedNodes);
            }

        public get canGenerateForSelectedItems() {
            if (this.selectedNodes == null || this.selectedNodes.length === 0 || parseInt(this.selectedNodes[0].getId()) < 0) {
                return false;
            }
            return this.generateCommand.canExecute(this.selectedNodes);
        }

        public openHelpLink() {
            this.$rootScope.$broadcast(Storyteller.BaseModalDialogController.dialogOpenEventName);
            return this.$uibModal.open(<angular.ui.bootstrap.IModalSettings>{
                templateUrl: `/Areas/Web/App/Common/Shell/Dialogs/HelpCarouselTemplate.html`,
                controller: "HelpCarouselController",
                controllerAs: "vm",
                windowClass: "image-preview-modal onboarding"
            });
        }

        public destroy() {
            if (this.selectedNodes) {
                this.selectedNodes.length = 0;
        }

            this.removeToolbarEventListeners();

            this.deleteCommand.destroy();
        }
    }

    export interface IStorytellerToolbarDirectiveScope extends ng.IScope {

        vm: StorytellerToolbarDirectiveController;
        isMenuDisabled: boolean;

        // event bus routed through this scope 
        subscribe(event, listener);
        publish(event, data);
    }

    export class StorytellerToolbarDirective implements ng.IDirective {

        public static factory(): ng.IDirective {
            return new StorytellerToolbarDirective();
        }

        constructor() {
        }

        public restrict = "E";

        public scope = {};

        public templateUrl = "/Areas/Web/App/Components/Storyteller/components/toolbar/StorytellerToolbar.html";

        public controller = StorytellerToolbarDirectiveController;
        public controllerAs = "vm";
        public bindToController = true;

        public link = ($scope: ng.IScope, $element: ng.IAugmentedJQuery, controller: StorytellerToolbarDirectiveController) => {
            $element.on("$destroy", () => {
            if(controller && controller.destroy)
                controller.destroy();
            });
        }
    }

    var app = angular.module("Storyteller");

    app.directive("storytellertoolbar", StorytellerToolbarDirective.factory);

}
