import * as angular from "angular";
import {ICommunicationManager, CommunicationManager} from "../../services/communication-manager";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {ComponentTest} from "../../../../util/component.test";
import {BpProcessTypeToggleController} from "./bp-process-type-toggle";
import {ProcessType} from "../../models/enums";
import {ILocalizationService} from "../../../../core/localization";

describe("BpProcessTypeToggle", () => {
    let _communicationManager: ICommunicationManager;
    let _localization: ILocalizationService;
    let _controller: BpProcessTypeToggleController;

    beforeEach(angular.mock.module("bp.editors.process"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(inject((
        $rootScope: ng.IRootScopeService, 
        communicationManager: ICommunicationManager,
        localization: ILocalizationService
    ) => {
        $rootScope["config"] = {
            labels: {
                "ST_ProcessType_BusinessProcess_Label": "Business Process mode",
                "ST_ProcessType_UserToSystemProcess_Label": "User-System Process mode"
            }
        };
        _communicationManager = communicationManager;
        _localization = localization;

        let template = "<bp-process-type-toggle></bp-process-type-toggle>";
        let componentTest = new ComponentTest<BpProcessTypeToggleController>(template, "bp-process-type-toggle");
        _controller = componentTest.createComponent([]);
    }));

    describe("default initialization", () => {
        it("results in correct number of options", () => {
            expect(_controller.options.length).toBe(2);
        });

        it("results in options containing all relevant process types", () => {
            expect(_controller.options.filter(o => o.type === ProcessType.BusinessProcess).length).toBe(1);
            expect(_controller.options.filter(o => o.type === ProcessType.UserToSystemProcess).length).toBe(1);
        });

        it("results in options having correct tooltips", () => {
            // arrange
            let businessProcessTooltip = _localization.get("ST_ProcessType_BusinessProcess_Label");
            let userSystemProcessTooltip = _localization.get("ST_ProcessType_UserToSystemProcess_Label");

            // assert
            expect(_controller.options.filter(o => o.type === ProcessType.BusinessProcess)[0].tooltip).toBe(businessProcessTooltip);
            expect(_controller.options.filter(o => o.type === ProcessType.UserToSystemProcess)[0].tooltip).toBe(userSystemProcessTooltip);
        });

        it("results in currentProcessType being undefined", () => {
            expect(_controller.currentProcessType).toBeUndefined();
        });

        it("results in isProcessTypeToggleEnabled being false", () => {
            expect(_controller.isProcessTypeToggleEnabled).toBeFalsy();
        });
    });

    it("initializes process type state from event (editable Business process)", () => {
        // arrange
        _communicationManager.toolbarCommunicationManager.enableProcessTypeToggle(true, ProcessType.BusinessProcess);

        // assert
        expect(_controller.isProcessTypeToggleEnabled).toBe(true);
        expect(_controller.currentProcessType).toBe(ProcessType.BusinessProcess);
    });

    it("initializes process type state from event (editable User-System process)", () => {
        // arrange
        _communicationManager.toolbarCommunicationManager.enableProcessTypeToggle(true, ProcessType.UserToSystemProcess);

        // assert
        expect(_controller.isProcessTypeToggleEnabled).toBe(true);
        expect(_controller.currentProcessType).toBe(ProcessType.UserToSystemProcess);
    });

    it("initializes process type state from event (read-only/historical Business process)", () => {
        // arrange
        _communicationManager.toolbarCommunicationManager.enableProcessTypeToggle(false, ProcessType.BusinessProcess);

        // assert
        expect(_controller.isProcessTypeToggleEnabled).toBe(false);
        expect(_controller.currentProcessType).toBe(ProcessType.BusinessProcess);
    });

    it("initializes process type state from event (read-only/historical User-System process)", () => {
        // arrange
        _communicationManager.toolbarCommunicationManager.enableProcessTypeToggle(false, ProcessType.UserToSystemProcess);

        // assert
        expect(_controller.isProcessTypeToggleEnabled).toBe(false);
        expect(_controller.currentProcessType).toBe(ProcessType.UserToSystemProcess);
    });

    it("communicates change of process type to User-System", () => {
        // arrange
        let toggleProcessTypeSpy = spyOn(_communicationManager.toolbarCommunicationManager, "toggleProcessType");
        _controller.currentProcessType = ProcessType.BusinessProcess;

        // act
         _controller.currentProcessType = ProcessType.UserToSystemProcess;
        _controller.processTypeChanged();

        // assert
        expect(toggleProcessTypeSpy).toHaveBeenCalledWith(ProcessType.UserToSystemProcess);
    });

    it("communicates change of process type to Business", () => {
        // arrange
        let toggleProcessTypeSpy = spyOn(_communicationManager.toolbarCommunicationManager, "toggleProcessType");
        _controller.currentProcessType = ProcessType.UserToSystemProcess;

        // act
         _controller.currentProcessType = ProcessType.BusinessProcess;
        _controller.processTypeChanged();

        // assert
        expect(toggleProcessTypeSpy).toHaveBeenCalledWith(ProcessType.BusinessProcess);
    });
});