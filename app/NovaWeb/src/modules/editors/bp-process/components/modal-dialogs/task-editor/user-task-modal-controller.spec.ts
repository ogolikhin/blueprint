import {MessageServiceMock} from "../../../../../main/components/messages/message.mock";
import {LoadingOverlayServiceMock} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {LocalizationServiceMock} from "../../../../../commonModule/localization/localization.service.mock";
import {ILoadingOverlayService} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";
import * as angular from "angular";
import {ArtifactServiceMock} from "../../../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {StatefulArtifactFactoryMock} from "../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {DialogServiceMock} from "../../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {UserTask} from "../../diagram/presentation/graph/shapes/user-task";
import {ModalServiceInstanceMock} from "../../../../../shell/login/mocks.spec";
import {CreateArtifactService, ICreateArtifactService} from "../../../../../main/components/projectControls/create-artifact.svc";
import {IArtifactReference, NodeType} from "../../diagram/presentation/graph/models";
import {UserTaskDialogModel} from "./userTaskDialogModel";
import {link} from "fs";
import {version} from "punycode";
import {IModalScope} from "../base-modal-dialog-controller";
import {UserTaskModalController} from "./user-task-modal-controller";
import {IDialogService} from "../../../../../shared/widgets/bp-dialog/bp-dialog";
import {IArtifactService, IStatefulArtifactFactory} from "../../../../../managers/artifact-manager/artifact";
import {Models} from "../../../../../main/models/";

require("script!mxClient");

describe("UserTaskModalController", () => {
    let $rootScope: ng.IRootScopeService;
    let $timeout: ng.ITimeoutService;
    let $anchorScroll: ng.IAnchorScrollService;
    let localization: ILocalizationService;
    let $q: ng.IQService;
    let createArtifactService: ICreateArtifactService;
    let statefulArtifactFactory: IStatefulArtifactFactory;
    let messageService: IMessageService;
    let artifactService: IArtifactService;
    let loadingOverlayService: ILoadingOverlayService;
    let $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance;
    let dialogService: IDialogService;

    beforeEach(angular.mock.module("bp.editors.process", ($provide: ng.auto.IProvideService) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("createArtifactService", CreateArtifactService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
    }));

    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$timeout_: ng.ITimeoutService,
                       _$location_: ng.ILocationService,
                       _localization_: ILocalizationService,
                       _$q_: ng.IQService,
                       _createArtifactService_: ICreateArtifactService,
                       _statefulArtifactFactory_: IStatefulArtifactFactory,
                       _messageService_: IMessageService,
                       _artifactService_: IArtifactService,
                       _loadingOverlayService_: ILoadingOverlayService,
                       _$uibModalInstance_: ng.ui.bootstrap.IModalServiceInstance) => {
        $rootScope = _$rootScope_;
        $timeout = _$timeout_;
        localization = _localization_;
        $q = _$q_;
        createArtifactService = _createArtifactService_;
        statefulArtifactFactory = _statefulArtifactFactory_;
        messageService = _messageService_;
        artifactService = _artifactService_;
        loadingOverlayService = _loadingOverlayService_;
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
            model.associatedArtifact = <IArtifactReference>{
                id: 5,
                name: "associated",
                typePrefix: "PRO"
            };
            model.originalItem = createUserTaskNode();
            model.personaReference = {
                id: -1,
                projectId: null,
                name: "User",
                typePrefix: null,
                baseItemTypePredefined: Models.ItemTypePredefined.Actor,
                projectName: null,
                link: null,
                version: null
            };

            const $scope = <IModalScope>$rootScope.$new();
            const localizationSpy = spyOn(localization, "get");
            const controller = new UserTaskModalController($scope,
                $rootScope,
                $timeout,
                dialogService,
                $q,
                localization,
                createArtifactService,
                statefulArtifactFactory,
                messageService,
                artifactService,
                loadingOverlayService,
                $uibModalInstance,
                model);

            const artifactReference: IArtifactReference = null;

            //act
            controller.saveData();
            $rootScope.$digest();

            //assert
            expect(model.originalItem.action).toEqual(model.action);
            expect(model.originalItem.associatedArtifact).toEqual(model.associatedArtifact);
            expect(model.originalItem.objective).toEqual(model.objective);
            expect(model.originalItem.label).toEqual(null);
            expect(model.originalItem.personaReference).toEqual(model.personaReference);
        });
    });
});
