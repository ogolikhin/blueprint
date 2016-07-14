module Storyteller {

    // Storyteller Routes

    // Configure the routes and route resolvers for the Storyteller module

    // Note: the state change occurs when the URL changes to /Storyteller/{id:int}
    
    export class StorytellerRoutes {

        public static $inject = ["$stateProvider", "$urlRouterProvider"];

        constructor(
            $stateProvider: ng.ui.IStateProvider,
            $urlRouterProvider: ng.ui.IUrlRouterProvider) {

            $stateProvider
                .state("Storyteller.load", {
                    url: "{id:any}?versionId&revisionId&baselineId&readOnly",
                views: {
                    "Storyteller.editorView": {
                        templateUrl: "/Areas/Web/App/Components/Storyteller/StorytellerEditorView.html"
                    }
                }
            });
        }
    }

    var app = angular.module("Storyteller");

    app.config(StorytellerRoutes);

}
