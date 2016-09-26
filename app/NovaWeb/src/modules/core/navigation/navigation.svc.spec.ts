import * as angular from "angular";
import "angular-mocks";
import {INavigationContext, INavigationService, NavigationService} from "./navigation.svc";

describe("NavigationService", () => {
    let $q: ng.IQService;
    let $state: ng.ui.IStateService;
    let navigationService: INavigationService;

    beforeEach(angular.mock.module("ui.router"));

    beforeEach(inject((_$q_: ng.IQService, _$state_: ng.ui.IStateService) => {
        $q = _$q_;
        $state = _$state_;

        navigationService = new NavigationService($state);
    }));

    describe("navigateToMain method", () => {
        it("initiates state transition to main state", () => {
            // arrange
            const expectedState = "main";
            const stateGoSpy = spyOn($state, "go");

            // act
            navigationService.navigateToMain();

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState);
        });
    });

    describe("navigateToArtifact", () => {
        it("initiates state transition to artifact state", () => {
            // arrange
            const artifactId = 63;
            const expectedState = "main.artifact";
            const expectedParams = { id: artifactId };
            const expectedOptions = { inherit: false };
            const stateGoSpy = spyOn($state, "go");

            // act
            navigationService.navigateToArtifact(artifactId);

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
        });

        it("initiates state transition to artifact state with path if navigating from previous artifact", () => {
            // arrange
            const artifactId = 63;
            const sourceArtifactId = 22;
            const path = sourceArtifactId.toString();
            const context: INavigationContext = { sourceArtifactId: sourceArtifactId };
            const expectedState = "main.artifact";
            const expectedParams = { id: artifactId, path: path };
            const expectedOptions = { inherit: false };
            const stateGoSpy = spyOn($state, "go");

            // act
            navigationService.navigateToArtifact(artifactId, context);

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
        });

        it("doesn't include path if previous artifact is not correctly indicated", () => {
            // arrange
            const artifactId = 63;
            const context: INavigationContext = { sourceArtifactId: null };
            const expectedState = "main.artifact";
            const expectedParams = { id: artifactId };
            const expectedOptions = { inherit: false };
            const stateGoSpy = spyOn($state, "go");

            // act
            navigationService.navigateToArtifact(artifactId, context);

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
        });

        it("doesn't include path if previous artifact is the same as the new artifact", () => {
            // arrange
            const artifactId = 63;
            const context: INavigationContext = { sourceArtifactId: artifactId };
            const expectedState = "main.artifact";
            const expectedParams = { id: artifactId };
            const expectedOptions = { inherit: false };
            const stateGoSpy = spyOn($state, "go");

            // act
            navigationService.navigateToArtifact(artifactId, context);

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
        });

        it("initiates state transition to artifact state with path if navigating from previous artifact in a chain of transitions", () => {
            // arrange
            const delimiter = ",";
            const artifactId = 63;
            const sourceArtifactId = 22;
            const path = `52${delimiter}23`;
            const context: INavigationContext = { sourceArtifactId: sourceArtifactId };
            const expectedState = "main.artifact";
            const expectedParams = { id: artifactId, path: `${path}${delimiter}${sourceArtifactId}` };
            const expectedOptions = { inherit: false };
            $state.params["path"] = path;
            const stateGoSpy = spyOn($state, "go");

            // act
            navigationService.navigateToArtifact(artifactId, context);

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
        });
    });
});