import * as angular from "angular";
import "angular-mocks";
import "angular-ui-router";
import {MainState} from "../../shell/router/main.state";
import {ArtifactState} from "../../main/router/artifact.state";
import {INavigationService, NavigationService} from "./navigation.svc";

describe("NavigationService", () => {
    let $q: ng.IQService;
    let $state: ng.ui.IStateService;
    let navigationService: INavigationService;

    let mainState = "main";
    let artifactState = "main.artifact";

    beforeEach(angular.mock.module("ui.router"));

    beforeEach(inject((_$q_: ng.IQService, _$state_: ng.ui.IStateService) => {
        $q = _$q_;
        $state = _$state_;
        navigationService = new NavigationService($q, $state);
    }));

    describe("navigateToMain", () => {
        it("initiates state transition to main state from main state", () => {
            // arrange
            const expectedState = mainState;
            const stateGoSpy = spyOn($state, "go");

            $state.current = new MainState();

            // act
            navigationService.navigateToMain();

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState);
            expect($state.current.params).toBeUndefined();
        });

        it("initiates state transition to main state from another artifact", () => {
            // arrange
            const sourceArtifactId: number = 54;
            const expectedState = mainState;
            const stateGoSpy = spyOn($state, "go");

            $state.params["id"] = sourceArtifactId.toString();
            $state.current = new ArtifactState();

            // act
            navigationService.navigateToMain();

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState);
            expect($state.current.params).toBeUndefined();
        });

        it("initiates state transition to main state from an artifact that has already been navigated to", () => {
            // arrange
            let predecessorArtifactId: number = 11;
            let sourceArtifactId: number = 54;
            const expectedState = mainState;
            const stateGoSpy = spyOn($state, "go");

            $state.params["id"] = sourceArtifactId.toString();
            $state.params["path"] = predecessorArtifactId.toString();
            $state.current = new ArtifactState();

            // act
            navigationService.navigateToMain();

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState);
            expect($state.current.params).toBeUndefined();
        });
    });

    describe("navigateToArtifact", () => {
        let targetArtifactId: number;

        beforeEach(() => {
            targetArtifactId = 63;
        });

        afterEach(() => {
            targetArtifactId = null;
        });

        describe("from main state", () => {
            beforeEach(() => {
                $state.current = new MainState();
            });

            afterEach(() => {
                $state.current = null;
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is not defined", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is disabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId, false);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is enabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId, true);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });
        });

        describe("from same artifact", () => {
            let sourceArtifactId: number;

            beforeEach(() => {
                sourceArtifactId = targetArtifactId;
                $state.params["id"] = sourceArtifactId.toString();
                $state.current = new ArtifactState();
            });

            afterEach(() => {
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.current = new MainState();
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is not defined", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is disabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId, false);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is enabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId, path: sourceArtifactId.toString() };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId, true);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });
        });

        describe("from another artifact", () => {
            let sourceArtifactId: number;

            beforeEach(() => {
                sourceArtifactId = 54;
                $state.params["id"] = sourceArtifactId.toString();
                $state.current = new ArtifactState();
            });

            afterEach(() => {
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.current = new MainState();
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is not defined", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is disabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId, false);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and correct path if navigation tracking is enabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId, path: sourceArtifactId.toString() };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId, true);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });
        });

        describe("from an artifact already navigated to", () => {
            let predecessorArtifactId: number;
            let sourceArtifactId: number;

            beforeEach(() => {
                predecessorArtifactId = 11;
                sourceArtifactId = 54;
                $state.params["id"] = sourceArtifactId.toString();
                $state.params["path"] = predecessorArtifactId.toString();
                $state.current = new ArtifactState();
            });

            afterEach(() => {
                predecessorArtifactId = null;
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.params["path"] = null;
                $state.current = new MainState();
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is not defined", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is disabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId, false);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and correct path if navigation tracking is enabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = { id: targetArtifactId, path: `${predecessorArtifactId},${sourceArtifactId}` };
                const expectedOptions = { inherit: false };

                // act
                navigationService.navigateToArtifact(targetArtifactId, true);

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });
        });
    });

    describe("navigateBack", () => {
        let predecessorArtifactId1: number;
        let predecessorArtifactId2: number;
        let sourceArtifactId: number;

        beforeEach(() => {
            predecessorArtifactId1 = 11;
            predecessorArtifactId2 = 22;
            sourceArtifactId = 33;
            $state.params["id"] = sourceArtifactId.toString();
            $state.params["path"] = `${predecessorArtifactId1},${predecessorArtifactId2}`;
            $state.current = new ArtifactState();
        });

        afterEach(() => {
            predecessorArtifactId1 = null;
            predecessorArtifactId2 = null;
            sourceArtifactId = null;
            $state.params["id"] = null;
            $state.params["path"] = null;
            $state.current = new MainState();
        });

        it("transitions to last artifact if path index is not provided", () => {
            // arrange
            const stateGoSpy = spyOn($state, "go");
            const expectedParams = { id: predecessorArtifactId2, path: `${predecessorArtifactId1}`};
            const expectedOptions = { inherit: false };

            // act
            navigationService.navigateBack();

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(artifactState, expectedParams, expectedOptions);
        });

        it("doesn't transition if navigation history doesn't exist", () => {
            // arrange
            const pathIndex = 1;
            const stateGoSpy = spyOn($state, "go");

            $state.params["path"] = undefined;

            // act
            navigationService.navigateBack(pathIndex);

            // assert
            expect(stateGoSpy).not.toHaveBeenCalled();
        });

        it("doesn't transition if path index is out of range (above the maximum)", () => {
            // arrange
            const pathIndex = 2;
            const stateGoSpy = spyOn($state, "go");

            // act
            navigationService.navigateBack(pathIndex);

            // assert
            expect(stateGoSpy).not.toHaveBeenCalled();
        });

        it("doesn't transition if path index is out of range (below the maximum)", () => {
            // arrange
            const pathIndex = -1;
            const stateGoSpy = spyOn($state, "go");

            // act
            navigationService.navigateBack(pathIndex);

            // assert
            expect(stateGoSpy).not.toHaveBeenCalled();
        });

        it("transitions if path index is correct", () => {
            // arrange
            const pathIndex = 0;
            const stateGoSpy = spyOn($state, "go");
            const expectedParams = { id: predecessorArtifactId1 };
            const expectedOptions = { inherit: false };

            // act
            navigationService.navigateBack(pathIndex);

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(artifactState, expectedParams, expectedOptions);
        });
    });
});