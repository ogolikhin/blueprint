import * as angular from "angular";
import "angular-mocks";
import "../../..";
import {ModalServiceInstanceMock} from "../../../../../shell/login/mocks.spec";
import {LocalizationServiceMock} from "../../../../../core/localization/localization.mock";
import {IModalScope} from "../base-modal-dialog-controller";
import {NodeType} from "../../diagram/presentation/graph/models";
import {IDialogService} from "../../../../../shared";
import {UserTaskDialogModel} from "./sub-artifact-dialog-model";
import {UserTaskModalController} from "./user-task-modal-controller";
import {IArtifactReference} from "../../../models/process-models";
import {UserTask} from "../../diagram/presentation/graph/shapes/";
import {ILocalizationService} from "../../../../../core/localization/localizationService";
require("script!mxClient");

describe("UserTaskModalController", () => {
    let $rootScope: ng.IRootScopeService;
    let $timeout: ng.ITimeoutService;
    let $anchorScroll: ng.IAnchorScrollService;
    let localization: ILocalizationService;
    let $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance;
    let dialogService: IDialogService;

    beforeEach(angular.mock.module("bp.editors.process", ($provide: ng.auto.IProvideService) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$timeout_: ng.ITimeoutService,
                       _$location_: ng.ILocationService,
                       _localization_: ILocalizationService,
                       _$uibModalInstance_: ng.ui.bootstrap.IModalServiceInstance) => {
        $rootScope = _$rootScope_;
        $timeout = _$timeout_;
        localization = _localization_;
        $uibModalInstance = _$uibModalInstance_;
    }));

    function createUserTaskNode(): UserTask {
        return <UserTask>{
            model: {id: 1},
            direction: null,
            action: null,
            label: null,
            row: null,
            column: null,
            newShapeColor: null,
            getNodeType: () => NodeType.UserTask
        };
    }

    describe("model is non-readonly ", () => {

        it("save data should be successful", () => {

            // arrange
            const model = new UserTaskDialogModel();
            model.isReadonly = false;
            model.isHistoricalVersion = false;
            model.action = "Custom Action";
            model.objective = "Custom Objective";
            model.label = "Custom Label";
            model.persona = "PM/PO";
            model.associatedArtifact = <IArtifactReference>{
                id: 5,
                name: "associated",
                typePrefix: "PRO"
            };
            model.originalItem = createUserTaskNode();

            const $scope = <IModalScope>$rootScope.$new();
            const localizationSpy = spyOn(localization, "get");
            const controller = new UserTaskModalController($scope,
                $rootScope,
                $timeout,
                dialogService,
                localization,
                $uibModalInstance,
                model);

            const artifactReference: IArtifactReference = null;

            //act
            controller.saveData();

            //assert
            expect(model.originalItem.action).toEqual(model.action);
            expect(model.originalItem.associatedArtifact).toEqual(model.associatedArtifact);
            expect(model.originalItem.objective).toEqual(model.objective);
            expect(model.originalItem.label).toEqual(null);
            expect(model.originalItem.persona).toEqual(model.persona);
        });

    });


});
