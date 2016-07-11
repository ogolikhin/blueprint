module Storyteller {

    export class StorytellerBreadcrumbDirectiveController {

        public static $inject = [
            "$state",
            "$window",
            "breadcrumbService",
            "bpUrlParsingService"
        ];

        private breadcrumbs: IArtifactReference[];

        constructor(
            private $state: ng.ui.IState,
            private $window: ng.IWindowService,
            private breadcrumbService: Shell.IBreadcrumbService,
            public bpUrlParsingService: IBpUrlParsingService) {

            this.getArtifactPathLinks();
        }

        private getArtifactPathLinks(): void {
            
            const storytellerParams = this.bpUrlParsingService.getStateParams();            

            this.breadcrumbService.getNavigationPath(storytellerParams.id,
                storytellerParams.versionId,
                storytellerParams.revisionId,
                storytellerParams.baselineId,
                storytellerParams.readOnly).then((result: IArtifactReference[]) => {
                this.breadcrumbs = result;
            });
        }

        public get artifactPathLinks(): IArtifactReference[] {
            if (this.breadcrumbs) {
                return this.breadcrumbs;
            }
            return null;
        }

        public navigateToBreadcrumb(url: string) {
            var data: ICommandData = { processId: 173, url: url };
            StorytellerCommands.getStorytellerCommands().getNavigateToProcessCommand().execute(data);
        }
    }

    export class StorytellerBreadcrumbDirective implements ng.IDirective {

        constructor() { }
        public restrict = "E";
        public scope = {};
        public templateUrl = "/Areas/Web/App/Components/Storyteller/Header/StorytellerBreadcrumbTemplate.html";
        public controller = StorytellerBreadcrumbDirectiveController;
        public controllerAs = "vm";
        public bindToController = true;

        public static factory(): ng.IDirective {
            return new StorytellerBreadcrumbDirective();
        }
    }

    var app = angular.module("Storyteller");
    app.directive("breadcrumbs", StorytellerBreadcrumbDirective.factory);
}