import * as angular from "angular";
import "angular-mocks";
import "angular-ui-router";
import {ISelectionManager} from "../managers/selection-manager/selection-manager";
import {ArtifactManagerMock} from "../managers/artifact-manager/artifact-manager.mock";
import {MainStateController} from "./app.router";

describe("AppRouter", () => {
    let $rootScope: ng.IRootScopeService,
        $window: ng.IWindowService,
        $state: angular.ui.IStateService,
        $log: ng.ILogService,
        selectionManager: ISelectionManager,
        isServerLicenseValid: boolean,
        ctrl: MainStateController;

    beforeEach(angular.mock.module("ui.router"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("isServerLicenseValid", Boolean);
        $provide.service("session", Boolean);
    }));

    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$window_: ng.IWindowService,
                       _$state_: ng.ui.IStateService,
                       _$log_: ng.ILogService,
                       _selectionManager_: ISelectionManager,
                       _isServerLicenseValid_: boolean) => {

        $rootScope = _$rootScope_;
        $window = _$window_;
        $state = _$state_;
        $log = _$log_;
        selectionManager = _selectionManager_;
        isServerLicenseValid = true;
    }));

    describe("$stateChangeSuccess", () => {
        beforeEach(() => {
            ctrl = new MainStateController($rootScope, $window, $state, $log, selectionManager, isServerLicenseValid, null, null, null);
        });

        it("should change title if navigating to an artifact", () => {
            // arrange
            $window.document.title = "Storyteller";
            const expectedTitle = "PR123: Artifact Name";
            spyOn(selectionManager, "getArtifact").and.returnValue({
                id: 123,
                prefix: "PR",
                name: "Artifact Name"
            });

            // act
            $rootScope.$broadcast("$stateChangeSuccess");

            // assert
            expect($window.document.title).toBe(expectedTitle);
        });

        it("should set title to default value if navigating to main", () => {
            // arrange
            $window.document.title = "";
            const expectedTitle = "Storyteller";
            spyOn(selectionManager, "getArtifact").and.returnValue(undefined);

            // act
            $rootScope.$broadcast("$stateChangeSuccess");

            // assert
            expect($window.document.title).toBe(expectedTitle);
        });
    });
});
