import "angular";
import "angular-mocks";
import {MainState} from "../../shell/router/main.state";
import {ArtifactState} from "../../main/router/artifact.state";
import {INavigationOptions, INavigationService, NavigationService} from "./navigation.svc";

describe("NavigationService", () => {
    let $state: ng.ui.IStateService;
    let navigationService: INavigationService;

    let mainState = "main";
    let artifactState = "main.artifact";

    beforeEach(angular.mock.module("ui.router"));

    beforeEach(inject((_$state_: ng.ui.IStateService) => {
        $state = _$state_;
        navigationService = new NavigationService($state);
    }));

    describe("navigateToMain", () => {
        describe("from main state", () => {
            beforeEach(() => {
                $state.current = new MainState();
            });

            it("initiates state transition to main state", () => {
                // arrange
                const expectedState = mainState;
                const stateGoSpy = spyOn($state, "go");

                // act
                navigationService.navigateToMain();

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState);
                expect($state.current.params).toBeUndefined();
            });
        });

        describe("from another artifact", () => {
            let sourceArtifactId: number;

            beforeEach(() => {
                sourceArtifactId = 54;
                $state.params["id"] = sourceArtifactId;
                $state.current = new ArtifactState();
            });

            afterEach(() => {
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.current = new MainState();
            });

            it("initiates state transition to main state", () => {
                // arrange
                const expectedState = mainState;
                const stateGoSpy = spyOn($state, "go");

                // act
                navigationService.navigateToMain();

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState);
                expect($state.current.params).toBeUndefined();
            });
        });

        describe("from an artifact already navigated to", () => {
            let predecessorArtifactId: number;
            let sourceArtifactId: number;

            beforeEach(() => {
                predecessorArtifactId = 11;
                sourceArtifactId = 54;
                $state.params["id"] = sourceArtifactId;
                $state.params["path"] = predecessorArtifactId;
                $state.current = new ArtifactState();
            });

            afterEach(() => {
                predecessorArtifactId = null;
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.params["path"] = null;
                $state.current = new MainState();
            });

            it("initiates state transition to main state", () => {
                // arrange
                const expectedState = mainState;
                const stateGoSpy = spyOn($state, "go");

                // act
                navigationService.navigateToMain();

                // assert
                expect(stateGoSpy).toHaveBeenCalledWith(expectedState);
                expect($state.current.params).toBeUndefined();
            });
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

            describe("if navigation options are not defined", () => {
                it("initiates state transition to artifact state with correct id and no path", () => {
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
            });

            describe("if navigation tracking is disabled", () => {
                let options: INavigationOptions = <INavigationOptions>{ enableTracking: false };

                it("initiates state transition to artifact state with correct id and no path", () => {
                    // arrange
                    const stateGoSpy = spyOn($state, "go");

                    const expectedState = artifactState;
                    const expectedParams = { id: targetArtifactId };
                    const expectedOptions = { inherit: false };

                    // act
                    navigationService.navigateToArtifact(targetArtifactId, options);

                    // assert
                    expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
                });
            });

            describe("if navigation tracking is enabled", () => {
                let options: INavigationOptions = <INavigationOptions>{ enableTracking: true };

                it("initiates state transition to artifact state with correct id and no path", () => {
                    // arrange
                    const stateGoSpy = spyOn($state, "go");

                    const expectedState = artifactState;
                    const expectedParams = { id: targetArtifactId };
                    const expectedOptions = { inherit: false };

                    // act
                    navigationService.navigateToArtifact(targetArtifactId, options);

                    // assert
                    expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
                });
            });
        });

        describe("from same artifact", () => {
            let sourceArtifactId: number;

            beforeEach(() => {
                sourceArtifactId = targetArtifactId;
                $state.params["id"] = sourceArtifactId;
                $state.current = new ArtifactState();
            });

            afterEach(() => {
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.current = new MainState();
            });

            describe("if navigation options are not defined", () => {
                it("initiates state transition to artifact state with correct id and no path", () => {
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
            });

            describe("if navigation tracking is disabled", () => {
                let options: INavigationOptions = <INavigationOptions>{ enableTracking: false };

                it("initiates state transition to artifact state with correct id and no path", () => {
                    // arrange
                    const stateGoSpy = spyOn($state, "go");

                    const expectedState = artifactState;
                    const expectedParams = { id: targetArtifactId };
                    const expectedOptions = { inherit: false };

                    // act
                    navigationService.navigateToArtifact(targetArtifactId, options);

                    // assert
                    expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
                });
            });

            describe("if navigation tracking is enabled", () => {
                let options: INavigationOptions = <INavigationOptions>{ enableTracking: true };

                it("initiates state transition to artifact state with correct id and no path", () => {
                    // arrange
                    const stateGoSpy = spyOn($state, "go");

                    const expectedState = artifactState;
                    const expectedParams = { id: targetArtifactId, path: sourceArtifactId.toString() };
                    const expectedOptions = { inherit: false };

                    // act
                    navigationService.navigateToArtifact(targetArtifactId, options);

                    // assert
                    expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
                });
            });
        });

        describe("from another artifact", () => {
            let sourceArtifactId: number;

            beforeEach(() => {
                sourceArtifactId = 54;
                $state.params["id"] = sourceArtifactId;
                $state.current = new ArtifactState();
            });

            afterEach(() => {
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.current = new MainState();
            });

            describe("if navigation options are not defined", () => {
                it("initiates state transition to artifact state with correct id and no path", () => {
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
            });

            describe("if navigation tracking is disabled", () => {
                let options: INavigationOptions = <INavigationOptions>{ enableTracking: false };

                it("initiates state transition to artifact state with correct id and no path", () => {
                    // arrange
                    const stateGoSpy = spyOn($state, "go");

                    const expectedState = artifactState;
                    const expectedParams = { id: targetArtifactId };
                    const expectedOptions = { inherit: false };

                    // act
                    navigationService.navigateToArtifact(targetArtifactId, options);

                    // assert
                    expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
                });
            });

            describe("if navigation tracking is enabled", () => {
                let options: INavigationOptions = <INavigationOptions>{ enableTracking: true };

                it("initiates state transition to artifact state with correct id and correct path", () => {
                    // arrange
                    const stateGoSpy = spyOn($state, "go");

                    const expectedState = artifactState;
                    const expectedParams = { id: targetArtifactId, path: sourceArtifactId.toString() };
                    const expectedOptions = { inherit: false };

                    // act
                    navigationService.navigateToArtifact(targetArtifactId, options);

                    // assert
                    expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
                });
            });
        });

        describe("from an artifact already navigated to", () => {
            let predecessorArtifactId: number;
            let sourceArtifactId: number;

            beforeEach(() => {
                predecessorArtifactId = 11;
                sourceArtifactId = 54;
                $state.params["id"] = sourceArtifactId;
                $state.params["path"] = predecessorArtifactId;
                $state.current = new ArtifactState();
            });

            afterEach(() => {
                predecessorArtifactId = null;
                sourceArtifactId = null;
                $state.params["id"] = null;
                $state.params["path"] = null;
                $state.current = new MainState();
            });

            describe("if navigation options are not defined", () => {
                it("initiates state transition to artifact state with correct id and no path", () => {
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
            });

            describe("if navigation tracking is disabled", () => {
                let options: INavigationOptions = <INavigationOptions>{ enableTracking: false };

                it("initiates state transition to artifact state with correct id and no path", () => {
                    // arrange
                    const stateGoSpy = spyOn($state, "go");

                    const expectedState = artifactState;
                    const expectedParams = { id: targetArtifactId };
                    const expectedOptions = { inherit: false };

                    // act
                    navigationService.navigateToArtifact(targetArtifactId, options);

                    // assert
                    expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
                });
            });

            describe("if navigation tracking is enabled", () => {
                let options: INavigationOptions = <INavigationOptions>{ enableTracking: true };

                it("initiates state transition to artifact state with correct id and correct path", () => {
                    // arrange
                    const stateGoSpy = spyOn($state, "go");

                    const expectedState = artifactState;
                    const expectedParams = { id: targetArtifactId, path: `${predecessorArtifactId},${sourceArtifactId}` };
                    const expectedOptions = { inherit: false };

                    // act
                    navigationService.navigateToArtifact(targetArtifactId, options);

                    // assert
                    expect(stateGoSpy).toHaveBeenCalledWith(expectedState, expectedParams, expectedOptions);
                });
            });
        });
    });
});