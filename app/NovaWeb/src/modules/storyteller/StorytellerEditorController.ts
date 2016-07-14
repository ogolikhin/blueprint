/// <reference path="editor/layout.ts" />
module Storyteller {

    export class StorytellerEditorController {

        public static $inject = [
            "$rootScope",
            "$scope",
            "$state",
            "$document",
            "processModelService",
            "userstoryService"
        ];

        public get Test(): string {
            return "this is a test";
        }
        constructor(private $rootScope: ng.IRootScopeService,
                    private $scope: ng.IScope,
                    $state: ng.ui.IState,
                    $document: ng.IDocumentService,
                    processModelService: IProcessModelService,
                    userstoryService: IUserstoryService) {

            $scope.$on('$destroy', () => {
                $rootScope.$broadcast("storytellerUnloadingEvent");
            });
        }
      
    }

    var app = angular.module("Storyteller");
    app.controller("StorytellerEditorController", StorytellerEditorController);
}
