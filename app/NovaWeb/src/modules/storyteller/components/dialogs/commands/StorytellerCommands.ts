module Storyteller {

    export interface IStorytellerCommands {
        getNavigateToProcessCommand(): any;
        getLogoutCommand(): any;
        getChangeStateCommand(): any;
    }

    export class StorytellerCommands implements IStorytellerCommands {
        public static $inject = ["$rootScope", "$location", "dialogService", "processModelService", "$q", "artifactVersionControlService", "$window", "$timeout"];

        constructor(private $rootScope: ng.IRootScopeService,
            private $location: ng.ILocationService,
            private dialogService: Shell.IDialogService,
            private processModelService: IProcessModelService,
            private $q: ng.IQService,
            private artifactVersionControlService: Shell.IArtifactVersionControlService,
            private $window: ng.IWindowService,
            private $timeout: ng.ITimeoutService) {
        }


        private navigateToProcessCommand = new NavigationCommand(this.$rootScope, this.$location, this.dialogService, this.processModelService, this.$window, this.$q, this.artifactVersionControlService, this.$timeout);
        public getNavigateToProcessCommand = () => {

            if (this.navigateToProcessCommand == null) {
                this.navigateToProcessCommand = new NavigationCommand(this.$rootScope, this.$location, this.dialogService, this.processModelService, this.$window, this.$q, this.artifactVersionControlService, this.$timeout);
            }
            return this.navigateToProcessCommand;
        }

        private changeStateCommand: ChangeStateCommand = null;
        public getChangeStateCommand = () => {

            if (this.changeStateCommand == null) {
                this.changeStateCommand = new ChangeStateCommand(this.$rootScope, this.$location, this.dialogService, this.processModelService, this.$window, this.$q, this.artifactVersionControlService);
            }
            return this.changeStateCommand;
        }

        private logoutCommand = new LogoutCommand(this.$rootScope, this.$location, this.dialogService, this.processModelService, this.$q, this.artifactVersionControlService);
        public getLogoutCommand = () => {

            if (this.logoutCommand == null) {
                this.logoutCommand = new LogoutCommand(this.$rootScope, this.$location, this.dialogService, this.processModelService, this.$q, this.artifactVersionControlService);
            }
            return this.logoutCommand;
        }

        /*
        * This static method should be used only in case when there is no way to inject 'StorytellerCommands' instance
        * using AngularJs DI mechanism
        */
        public static getStorytellerCommands() {
            const commands = angular.element(document.body).injector().get("storytellerCommands");
            return commands;
        }
    }

    var app = angular.module("Storyteller");
    app.service("storytellerCommands", StorytellerCommands);
}