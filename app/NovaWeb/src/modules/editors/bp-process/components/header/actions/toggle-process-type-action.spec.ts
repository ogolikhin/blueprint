import * as angular from "angular";
import "angular-mocks";
import {ToggleProcessTypeAction} from "./toggle-process-type-action";
import {CommunicationManager} from "../../../";
import {LocalizationServiceMock} from "../../../../../core/localization/localization.mock";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {RolePermissions, LockedByEnum} from "../../../../../main/models/enums";
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

    describe("toggle", () => {
        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new ToggleProcessTypeAction(
                process, 
                communicationManager.toolbarCommunicationManager, 
                localization
            );

            // act
            process.artifactState.setState({readonly: true }, false);

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is enabled if process is not read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();

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
});

function createStatefulProcessArtifact(version: number = 1): StatefulProcessArtifact {
    const artifactModel = {
        id: 1,
        permissions: RolePermissions.Edit,
        version: version
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