import * as angular from "angular";
import "angular-mocks";
import "angular-ui-router";
import {INavigationService, NavigationService} from "./navigation.svc";

describe("NavigationService", () => {
    let $q: ng.IQService;
    let $scope: ng.IScope;
    let $state: ng.ui.IStateService;
    let navigationService: INavigationService;

    let mainState = "main";
    let artifactState = "main.item";

    beforeEach(angular.mock.module("ui.router"));

    beforeEach(inject(($rootScope: ng.IRootScopeService, _$q_: ng.IQService, _$state_: ng.ui.IStateService) => {
        $q = _$q_;
        $scope = $rootScope.$new();
        $state = _$state_;
        navigationService = new NavigationService($q, $state);
    }));

    describe("navigateToMain", () => {
        it("initiates state transition to main state from main state", () => {
            // arrange
            const expectedState = mainState;
            const expectedParams = {};
            const expectedOptions = {location: true};
            const stateGoSpy = spyOn($state, "go");

            $state.current.name = "main";

            // act
            navigationService.navigateToMain();

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            expect($state.current.params).toBeUndefined();
        });

        it("redirects to main state from another artifact", () => {
            // arrange
            const sourceArtifactId: number = 54;
            const expectedState = mainState;
            const expectedParams = {};
            const expectedOptions = {location: "replace"};
            const stateGoSpy = spyOn($state, "go");

            $state.params["id"] = sourceArtifactId.toString();
            $state.current.name = "main.item";

            // act
            navigationService.navigateToMain(true);

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            expect($state.current.params).toBeUndefined();
        });

        it("initiates state transition to main state from another artifact", () => {
            // arrange
            const sourceArtifactId: number = 54;
            const expectedState = mainState;
            const expectedParams = {};
            const expectedOptions = {location: true};
            const stateGoSpy = spyOn($state, "go");

            $state.params["id"] = sourceArtifactId.toString();
            $state.current.name = "main.item";

            // act
            navigationService.navigateToMain();

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            expect($state.current.params).toBeUndefined();
        });

        it("initiates state transition to main state from an artifact that has already been navigated to", () => {
            // arrange
            let predecessorArtifactId: number = 11;
            let sourceArtifactId: number = 54;
            const expectedState = mainState;
            const expectedParams = {};
            const expectedOptions = {location: true};
            const stateGoSpy = spyOn($state, "go");

            $state.params["id"] = sourceArtifactId.toString();
            $state.params["path"] = predecessorArtifactId.toString();
            $state.current.name = "main.item";

            // act
            navigationService.navigateToMain();

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
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
                $state.current.name = "main";
            });

            afterEach(() => {
                $state.current = null;
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is not defined", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = {id: targetArtifactId};
                const expectedOptions = {inherit: false, location: true};

                // act
                navigationService.navigateTo({ id: targetArtifactId });

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is disabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = {id: targetArtifactId};
                const expectedOptions = {inherit: false, location: true};

                // act
                navigationService.navigateTo({ id: targetArtifactId });

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is enabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = {id: targetArtifactId};
                const expectedOptions = {inherit: false, location: true};

                // act
                navigationService.navigateTo({ id: targetArtifactId, enableTracking: true });

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });
        });

        describe("from same artifact", () => {
            let sourceArtifactId: number;

            beforeEach(() => {
                sourceArtifactId = targetArtifactId;
                $state.params["id"] = sourceArtifactId.toString();
                $state.current.name = "main.item";
            });

            afterEach(() => {
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.current.name = "main";
            });

            it("doesn't transition if path is not specified", (done) => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                // act
                navigationService.navigateTo({ id: targetArtifactId })
                    .catch((error: any) => {
                        // assert
                        expect(stateGoSpy).not.toHaveBeenCalledWith();
                        done();
                    });

                $scope.$digest();
            });
        });

        describe("from another artifact", () => {
            let sourceArtifactId: number;

            beforeEach(() => {
                sourceArtifactId = 54;
                $state.params["id"] = sourceArtifactId.toString();
                $state.current.name = "main.item";
            });

            afterEach(() => {
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.current.name = "main";
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is not defined", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = {id: targetArtifactId};
                const expectedOptions = {inherit: false, location: true};

                // act
                navigationService.navigateTo({ id: targetArtifactId });

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is disabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = {id: targetArtifactId};
                const expectedOptions = {inherit: false, location: true};

                // act
                navigationService.navigateTo({ id: targetArtifactId });

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and correct path if navigation tracking is enabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = {id: targetArtifactId, path: sourceArtifactId.toString()};
                const expectedOptions = {inherit: false, location: true};

                // act
                navigationService.navigateTo({ id: targetArtifactId, enableTracking: true });

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
                $state.current.name = "main.item";
            });

            afterEach(() => {
                predecessorArtifactId = null;
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.params["path"] = null;
                $state.current.name = "main";
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is not defined", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = {id: targetArtifactId};
                const expectedOptions = {inherit: false, location: true};

                // act
                navigationService.navigateTo({ id: targetArtifactId });

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and no path if navigation tracking is disabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = {id: targetArtifactId};
                const expectedOptions = {inherit: false, location: true};

                // act
                navigationService.navigateTo({ id: targetArtifactId });

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
            });

            it("initiates state transition to artifact state with correct id and correct path if navigation tracking is enabled", () => {
                // arrange
                const stateGoSpy = spyOn($state, "go");

                const expectedState = artifactState;
                const expectedParams = {id: targetArtifactId, path: `${predecessorArtifactId},${sourceArtifactId}`};
                const expectedOptions = {inherit: false, location: true};

                // act
                navigationService.navigateTo({ id: targetArtifactId, enableTracking: true });

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
            $state.current.name = "main.item";
        });

        afterEach(() => {
            predecessorArtifactId1 = null;
            predecessorArtifactId2 = null;
            sourceArtifactId = null;
            $state.params["id"] = null;
            $state.params["path"] = null;
            $state.current.name = "main";
        });

        it("transitions to last artifact if path index is not provided", () => {
            // arrange
            const stateGoSpy = spyOn($state, "go");
            const expectedParams = {id: predecessorArtifactId2, version: undefined, path: `${predecessorArtifactId1}`};
            const expectedOptions = {inherit: false};

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
            const expectedParams = {id: predecessorArtifactId1, version: undefined};
            const expectedOptions = {inherit: false};

            // act
            navigationService.navigateBack(pathIndex);

            // assert
            expect(stateGoSpy).toHaveBeenCalledWith(artifactState, expectedParams, expectedOptions);
        });
    });
});
