import * as angular from "angular";
import "angular-mocks";
import "script!mxClient";
import {CopyAction} from "./copy-action";
import {CommunicationManager} from "../../../services/communication-manager";
import {LocalizationServiceMock} from "../../../../../core/localization/localization.mock";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import * as TestModels from "../../../models/test-model-factory";
import {UserTask} from "../../diagram/presentation/graph/shapes/user-task";
import {SystemDecision} from "../../diagram/presentation/graph/shapes/system-decision";
import {SystemTask} from "../../diagram/presentation/graph/shapes/system-task";

describe("CopyAction", () => {
    let $rootScope: ng.IRootScopeService;
    let $q: ng.IQService;
    let localization: LocalizationServiceMock;
    let communicationManager: CommunicationManager;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("communicationManager", CommunicationManager);
    }));

    beforeEach(
        inject((
            _$rootScope_: ng.IRootScopeService,
            _$q_: ng.IQService,
            _localization_: LocalizationServiceMock,
            _communicationManager_: CommunicationManager
        ) => {
            $rootScope = _$rootScope_;
            $q = _$q_;
            localization = _localization_;
            communicationManager = _communicationManager_;

            $rootScope["config"] = {labels: []};
        })
    );

    describe("constructor", () => {
        it("throws error if process is not provided", () => {
            // arrange
            let error: Error;

            // act
            try {
                new CopyAction(null, communicationManager, localization);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Process is not provided or is null");
        });

        it("throws error if communicationManager is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new CopyAction(process, null, localization);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Communication manager is not provided or is null");
        });

        it("throws error if localization is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new CopyAction(process, communicationManager, null);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Localization service is not provided or is null");
        });
    });

    it("returns correct icon", () => {
        // arrange
        const process = createStatefulProcessArtifact();
        const action = new CopyAction(process, communicationManager, localization);
        const expectedIcon = "fonticon2-copy-shapes";

        // act
        const icon = action.icon;

        // assert
        expect(icon).toEqual(expectedIcon);
    });

    it("returns correct tooltip", () => {
        // arrange
        const process = createStatefulProcessArtifact();
        const action = new CopyAction(process, communicationManager, localization);
        const expectedTooltip = localization.get("App_Toolbar_Copy_Shapes");

        // act
        const tooltip = action.tooltip;

        // assert
        expect(tooltip).toEqual(expectedTooltip);
    });

    describe("on selection change", () => {
        it("sets disabled to true when selection contains no shapes", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new CopyAction(process, communicationManager, localization);

            // act
            communicationManager.processDiagramCommunication.action(ProcessEvents.SelectionChanged, []);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("sets disabled to true when selection contains single invalid shape", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new CopyAction(process, communicationManager, localization);
            const invalidShape = createSystemTask(12);

            // act
            communicationManager.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [invalidShape]);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("sets disabled to true when selection contains multiple invalid shapes", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new CopyAction(process, communicationManager, localization);
            const invalidShape1 = createSystemTask(12);
            const invalidShape2 = createSystemDecision(23);

            // act
            communicationManager.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [invalidShape1, invalidShape2]);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("sets disabled to true when selection contains multiple valid and invalid shapes", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new CopyAction(process, communicationManager, localization);
            const validShape = createUserTask(23);
            const invalidShape = createSystemTask(12);

            // act
            communicationManager.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [validShape, invalidShape]);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("sets disabled to false when selection contains single User Task shape", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new CopyAction(process, communicationManager, localization);
            const validShape = createUserTask(23);

            // act
            communicationManager.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [validShape]);

            // assert
            expect(action.disabled).toEqual(false);
        });

        it("sets disabled to false when selection contains multiple User Task shapes", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new CopyAction(process, communicationManager, localization);
            const validShape1 = createUserTask(23);
            const validShape2 = createUserTask(53);

            // act
            communicationManager.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [validShape1, validShape2]);

            // assert
            expect(action.disabled).toEqual(false);
        });
    });

    describe("execute", () => {
        it("calls copy when selected shapes are valid", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new CopyAction(process, communicationManager, localization);
            const spy = spyOn(communicationManager.toolbarCommunicationManager, "copySelection");
            const validShape = createUserTask(23);
            communicationManager.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [validShape]);

            // act
            action.execute();

            // assert
            expect(spy).toHaveBeenCalled();
        });

        it("doesn't call copy when selected shapes are not valid", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new CopyAction(process, communicationManager, localization);
            const spy = spyOn(communicationManager.toolbarCommunicationManager, "copySelection");
            const invalidShape = createSystemTask(12);
            communicationManager.processDiagramCommunication.action(ProcessEvents.SelectionChanged, [invalidShape]);

            // act
            action.execute();

            // assert
            expect(spy).not.toHaveBeenCalled();
        });
    });

    describe("dispose", () => {
        it("calls dispose on the event observer", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const handle = "test";
            const registerSpy = spyOn(communicationManager.processDiagramCommunication, "register").and.callFake(() => handle);
            const spy = spyOn(communicationManager.processDiagramCommunication, "unregister");
            const action = new CopyAction(process, communicationManager, localization);

            // act
            action.dispose();

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.SelectionChanged, handle);
        });
    });

    function createStatefulProcessArtifact(version: number = 1): StatefulProcessArtifact {
        const artifactModel = {id: 1};
        const process = new StatefulProcessArtifact(artifactModel, null);

        return process;
    }

    function createUserTask(id: number): UserTask {
        return new UserTask(TestModels.createUserTask(id), $rootScope, undefined, undefined);
    }

    function createSystemTask(id: number): SystemTask {
        return new SystemTask(TestModels.createSystemTask(id), $rootScope, undefined, undefined, undefined);
    }

    function createSystemDecision(id: number): SystemDecision {
        return new SystemDecision(TestModels.createSystemDecision(id), $rootScope, undefined);
    }
});
