import "angular-mocks";
import "script!mxClient";
import {LocalizationServiceMock} from "../../../../../commonModule/localization/localization.service.mock";
import {ItemTypePredefined} from "../../../../../main/models/item-type-predefined";
import * as TestModels from "../../../models/test-model-factory";
import * as TestShapes from "../../../models/test-shape-factory";
import {IStatefulProcessArtifact, StatefulProcessArtifact} from "./../../../process-artifact";
import {CommunicationManager} from "./../../../services/communication-manager";
import {ProcessEvents} from "./../../diagram/process-diagram-communication";
import {IProcessDiagramCommunication} from "./../../diagram/process-diagram-communication";
import {OpenProcessImpactAnalysisAction} from "./open-process-impact-analysis-action";
import * as angular from "angular";

describe("OpenProcessImpactAnalysisAction", () => {
    let $rootScope: ng.IRootScopeService;
    let statefulProcess: IStatefulProcessArtifact;
    let localization: LocalizationServiceMock;
    let processDiagramCommunication: IProcessDiagramCommunication;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("communicationManager", CommunicationManager);
    }));

    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
        _localization_: LocalizationServiceMock,
        _communicationManager_: CommunicationManager
    ) => {
        $rootScope = _$rootScope_;
        $rootScope["config"] = {labels: []};

        const processModel = TestModels.createProcessModel();
        processModel["predefinedType"] = ItemTypePredefined.Process;
        statefulProcess = new StatefulProcessArtifact(processModel, null);

        localization = _localization_;
        processDiagramCommunication = _communicationManager_.processDiagramCommunication;
    }));

    describe("constructor", () => {
        it("throws error if localization is not provided", () => {
            // arrange
            let error: Error;

            // act
            try {
                const action = new OpenProcessImpactAnalysisAction(statefulProcess, null, processDiagramCommunication);
            } catch (ex) {
                error = ex;
            }

            // assert
            expect(error).toBeDefined();
            expect(error.message).toEqual("Localization service not provided or is null");
        });

        it("throws error if process diagram communication manager is not provided", () => {
            // arrange
            let error: Error;

            // act
            try {
                const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, null);
            } catch (ex) {
                error = ex;
            }

            // assert
            expect(error).toBeDefined();
            expect(error.message).toEqual("Process diagram communication is not provided or is null");
        });

        it("sets disabled to true if no stateful process is provided", () => {
            // act
            const action = new OpenProcessImpactAnalysisAction(null, localization, processDiagramCommunication);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("registers selection change listener", () => {
            // arrange
            const spy = spyOn(processDiagramCommunication, "register").and.callThrough();

            // act
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.SelectionChanged, jasmine.any(Function));
        });
    });

    describe("on selection changed", () => {
        it("sets disabled to false when no shapes are selected", () => {
            // arrange
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, []);

            // assert
            expect(action.disabled).toEqual(false);
        });

        it("sets tooltip to artifact tooltip when no shapes are selected", () => {
            // arrange
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, []);

            // assert
            expect(action.tooltip).toEqual(localization.get("App_Toolbar_Open_Impact_Analysis"));
        });

        it("sets disabled to false when a single shapes is selected", () => {
            // arrange
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);

            // assert
            expect(action.disabled).toEqual(false);
        });

        it("sets tooltip to shape tooltip when a single shapes is selected", () => {
            // arrange
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);

            // assert
            expect(action.tooltip).toEqual(localization.get("App_Toolbar_Open_Impact_Analysis"));
        });

        it("sets disabled to true when multiple shapes are selected", () => {
            // arrange
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(14, $rootScope);
            const userTask2 = TestShapes.createUserTask(15, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask, userTask2]);

            // assert
            expect(action.disabled).toEqual(true);
        });

        it("sets disabled to true when new shape is selected", () => {
            // arrange
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);
            const userTask = TestShapes.createUserTask(-1, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);

            // assert
            expect(action.disabled).toEqual(true);
        });
    });

    describe("execute", () => {
        it("opens impact analysis for process when no shapes are selected", () => {
            // arrange
            const processId = statefulProcess.id;
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);
            const spy = spyOn(window, "open");
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, []);

            // act
            action.execute();

            // assert
            expect(spy).toHaveBeenCalledWith(`Web/#/ImpactAnalysis/${processId}`);
        });

        it("opens impact analysis for process shape when a single shapes is selected", () => {
            // arrange
            const processShapeId = 14;
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);
            const spy = spyOn(window, "open");
            const userTask = TestShapes.createUserTask(processShapeId, $rootScope);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);

            // act
            action.execute();

            // assert
            expect(spy).toHaveBeenCalledWith(`Web/#/ImpactAnalysis/${processShapeId}`);
        });

        it("doesn't open impact analysis when multiple shapes are selected", () => {
            // arrange
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);
            const spy = spyOn(window, "open");
            const userTask = TestShapes.createUserTask(14, $rootScope);
            const userTask2 = TestShapes.createUserTask(15, $rootScope);
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask, userTask2]);

            // act
            action.execute();

            // assert
            expect(spy).not.toHaveBeenCalled();
        });
    });

    describe("dispose", () => {
        it("unregisters selection change listener", () => {
            // arrange
            const spy = spyOn(processDiagramCommunication, "unregister").and.callThrough();
            const action = new OpenProcessImpactAnalysisAction(statefulProcess, localization, processDiagramCommunication);

            // act
            action.dispose();

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.SelectionChanged, jasmine.any(String));
        });
    });
});
