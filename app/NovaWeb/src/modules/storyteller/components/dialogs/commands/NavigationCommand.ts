module Storyteller {

    export class NavigationCommand implements ICommand {

        constructor(private $rootScope: ng.IRootScopeService,
            private $location: ng.ILocationService,
            private dialogService: Shell.IDialogService,
            private processModelService: IProcessModelService,
            private $window: ng.IWindowService,
            private $q: ng.IQService,
            private artifactVersionControlService: Shell.IArtifactVersionControlService,
            private $timeout: ng.ITimeoutService) {
        }

        public execute = (data: ICommandData) => {
            this.$rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName);
            var processId = data.processId;
            var url = data.url;
            var defer = this.$q.defer<boolean>();
            this.$timeout(() => {
                this.navigate(processId, url);
                defer.resolve(true);
            }, 0);

            return defer.promise;
        }

        private navigate(processId: number, url: string) {
            if (url) {
                this.$window.location.href = url;
            } else {
                var path = this.$location.path();
                this.$location.path(path + (path.lastIndexOf("/") === path.length - 1 ? "" : "/") + processId);
            }
        }
    }
}