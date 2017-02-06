import * as angular from "angular";
import "angular-mocks";
import "script!mxClient";
import {ToggleProcessTypeAction} from "./toggle-process-type-action";
import {CommunicationManager} from "../../../";
import {LocalizationServiceMock} from "../../../../../commonModule/localization/localization.service.mock";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {RolePermissions, LockedByEnum, ReuseSettings} from "../../../../../main/models/enums";
import {ProcessType} from "../../../models/enums";

describe("ToggleProcessTypeAction", () => {
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
        })
    );

    describe("constructor", () => {
        it("throws error if process is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new ToggleProcessTypeAction(
                    null,
                    communicationManager.toolbarCommunicationManager,
                    localization
                );
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Process is not provided or is null");
        });

        it("throws error if toolbar communication manager is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new ToggleProcessTypeAction(
                    process,
                    null,
                    localization
                );
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Toolbar communication is not provided or is null");
        });

        it("throws error if localization service is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new ToggleProcessTypeAction(
                    process,
                    communicationManager.toolbarCommunicationManager,
                    null
                );
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Localization service is not provided or is null");
        });
    });

    describe("on artifact loaded", () => {
        it("set current value to BusinessProcess for business process", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const subject = new Rx.BehaviorSubject<StatefulProcessArtifact>(process);
            process.propertyValues["clientType"].value = ProcessType.BusinessProcess;
            spyOn(process, "getObservable").and.returnValue(subject);

            // act
            const action = new ToggleProcessTypeAction(process, communicationManager.toolbarCommunicationManager, localization);

            // assert
            expect(action.currentValue).toEqual(ProcessType.BusinessProcess);
        });

        it("set current value to UserToSystem for user-to-system process", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            process.propertyValues["clientType"].value = ProcessType.UserToSystemProcess;
            const subject = new Rx.BehaviorSubject<StatefulProcessArtifact>(process);
            spyOn(process, "getObservable").and.returnValue(subject);

            // act
            const action = new ToggleProcessTypeAction(process, communicationManager.toolbarCommunicationManager, localization);

            // assert
            expect(action.currentValue).toEqual(ProcessType.UserToSystemProcess);
        });
    });

    describe("toggle", () => {
        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const subject = new Rx.BehaviorSubject<StatefulProcessArtifact>(process);
            spyOn(process, "getObservable").and.returnValue(subject);
            const action = new ToggleProcessTypeAction(
                process,
                communicationManager.toolbarCommunicationManager,
                localization
            );

            // act
            process.artifactState.setState({readonly: true}, false);

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled if process is resued and has subartifacts reuse read-only settings", () => {
            // arrange
            const process = createStatefulProcessArtifact(1, ReuseSettings.Subartifacts);
            const subject = new Rx.BehaviorSubject<StatefulProcessArtifact>(process);
            spyOn(process, "getObservable").and.returnValue(subject);
            const action = new ToggleProcessTypeAction(
                process,
                communicationManager.toolbarCommunicationManager,
                localization
            );

            // act
            process.artifactState.setState({readonly: false}, false);

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is enabled if process is reuse read-only for attachments", () => {
            // arrange
            const process = createStatefulProcessArtifact(1, ReuseSettings.Attachments);
            const subject = new Rx.BehaviorSubject<StatefulProcessArtifact>(process);
            spyOn(process, "getObservable").and.returnValue(subject);
            const action = new ToggleProcessTypeAction(
                process,
                communicationManager.toolbarCommunicationManager,
                localization
            );

            // act
            process.artifactState.setState({readonly: false}, false);

            // assert
            expect(action.disabled).toBe(false);
        });

        it("is enabled if process is not read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const subject = new Rx.BehaviorSubject<StatefulProcessArtifact>(process);
            spyOn(process, "getObservable").and.returnValue(subject);

            // act
            const action = new ToggleProcessTypeAction(
                process,
                communicationManager.toolbarCommunicationManager,
                localization
            );

            // assert
            expect(action.disabled).toBe(false);
        });

        it("notifies about process type change", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const subject = new Rx.BehaviorSubject<StatefulProcessArtifact>(process);
            spyOn(process, "getObservable").and.returnValue(subject);
            const action = new ToggleProcessTypeAction(
                process,
                communicationManager.toolbarCommunicationManager,
                localization
            );
            const toggleSpy = spyOn(communicationManager.toolbarCommunicationManager, "toggleProcessType");

            // act
            const value = ProcessType.BusinessProcess;
            action.currentValue = value;

            // assert
            expect(toggleSpy).toHaveBeenCalledWith(value);
        });
    });

    describe("dispose", () => {
        it("doesn't handle artifact change", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const subject = new Rx.BehaviorSubject<StatefulProcessArtifact>(process);
            spyOn(process, "getObservable").and.returnValue(subject);
            const action = new ToggleProcessTypeAction(process, communicationManager.toolbarCommunicationManager, localization);
            const spy = spyOn(action, "onArtifactLoaded");
            action.dispose();

            // act
            process.propertyValues["clientType"].value = ProcessType.BusinessProcess;

            // assert
            expect(spy).not.toHaveBeenCalled();
        });
    });
});

function createStatefulProcessArtifact(version: number = 1, reuseSettings?: ReuseSettings): StatefulProcessArtifact {
    const artifactModel = {
        id: 1,
        permissions: RolePermissions.Edit,
        version: version,
        readOnlyReuseSettings: reuseSettings
    };
    const process = new StatefulProcessArtifact(artifactModel, null);
    process.propertyValues = {
        "clientType": {
            propertyName: "clientType",
            typeId: 0,
            typePredefined: 0,
            value: ProcessType.UserToSystemProcess
        }
    };

    return process;
}
