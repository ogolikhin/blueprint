module Storyteller {

    export class StorytellerHeaderDirectiveController {

        public static $inject = [
            "$window",
            "$scope",
            "$rootScope",
            "header"
        ];
        public processArtifact: ItemTypePredefined = BluePrintSys.RC.CrossCutting.ItemTypePredefined.Process;
        private isHeaderTextOverflown: boolean = false;

        constructor(
            private $window: ng.IWindowService,
            private $scope: IStorytellerHeaderDirectiveScope,
            private $rootScope,
            private header: IStorytellerHeader) {

            // using controllerAs syntax
            $scope.vm = this;
            this.$rootScope.$on("changeSystemTasksVisibility",
                (e, value) => {
                    this.header.isUserToSystemProcess = value;
                });

            this.$window.addEventListener("resize", () => this.resizeHeader());

            $scope.$on('$destroy', () => {
                this.$window.removeEventListener("resize", () => this.resizeHeader());
                if ($scope["vm"].header !== null) {
                    $scope["vm"].header.destroy();
                    $scope["vm"].header = null;
                }
            });
        }

        public resizeHeader = () => {
            let element = $("#desctext");
            if (element && element[0]) {
                this.isHeaderTextOverflown = this.calculateIsHeaderTextOverflown(element);
            } else {
                this.isHeaderTextOverflown = false;
            }
            this.$scope.$digest();
        }

        private calculateIsHeaderTextOverflown(element: JQuery): boolean {
            return (element[0].scrollWidth > element.innerWidth() || element[0].offsetHeight > 20 ||
                (this.header && this.header.doesDescriptionContainNewLine()));
        }                 

        public openInParentWindow(id: number): void {
            let parentWindow: Window = window.opener;
            let url = "../?ArtifactId=" + id;

            if (parentWindow) {
                try {
                    parentWindow.location.href = url;
                    parentWindow.focus();
                } catch(error) {
                    // parent refused to reload main experience with linked artifact
                }
            } else {
                window.open(url, '_blank');
            }
        }

        public updateSelection($event, id) {
            var checkbox = $event.target;
            this.$rootScope.$broadcast("changeSystemTasksVisibility", checkbox.checked);
        };
    }

    interface IStorytellerHeaderDirectiveScope extends ng.IScope {
        vm: StorytellerHeaderDirectiveController;
    }

    export class StorytellerHeaderDirective implements ng.IDirective {

        constructor(private $timeout: ng.ITimeoutService) { }

        public restrict = "E";
        public scope = {};
        public templateUrl = "/Areas/Web/App/Components/Storyteller/Header/StorytellerHeaderTemplate.html";
        public controller = StorytellerHeaderDirectiveController;
        public controllerAs = "vm";
        public bindToController = true;

        public static directive: any[] = [
            "$timeout",
            ($timeout: ng.ITimeoutService) => {
                return new StorytellerHeaderDirective($timeout);
            }];
    }

    let app = angular.module("Storyteller");
    app.directive("storytellerheader", StorytellerHeaderDirective.directive);
    app.value('header', new StorytellerHeader());
}
