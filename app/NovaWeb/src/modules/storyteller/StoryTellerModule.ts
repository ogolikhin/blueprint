module Storyteller {

    // Storyteller Module
    const app = angular.module("Storyteller", ["ui.bootstrap"]);

    export class StorytellerController {

        public static $inject = ["$rootScope"];

        constructor($rootScope) {

            // display the Storyteller module name on the header
            // Note: the header title is bound to $rootScope.module.name   
            $rootScope.module.name = $rootScope.config.labels["ST_Storyteller"];
            
           // set the title of the browser tab   
            document.title = "Blueprint " + $rootScope.config.labels["ST_Storyteller"];
        }
    }
     
    app.controller("StorytellerController", StorytellerController);
}
