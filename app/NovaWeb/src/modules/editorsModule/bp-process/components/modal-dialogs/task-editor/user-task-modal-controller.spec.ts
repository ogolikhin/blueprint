import "../../..";
import "angular-mocks";
import {ILoadingOverlayService} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {LoadingOverlayServiceMock} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";
import {LocalizationServiceMock} from "../../../../../commonModule/localization/localization.service.mock";
import {MessageServiceMock} from "../../../../../main/components/messages/message.mock";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {CreateArtifactService, ICreateArtifactService} from "../../../../../main/components/projectControls/create-artifact.svc";
import {ItemTypePredefined} from "../../../../../main/models/itemTypePredefined.enum";
import {IArtifactService, IStatefulArtifactFactory} from "../../../../../managers/artifact-manager/artifact";
import {StatefulArtifactFactoryMock} from "../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ArtifactServiceMock} from "../../../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {ISelectionManager} from "../../../../../managers/selection-manager/selection-manager";
import {SelectionManagerMock} from "../../../../../managers/selection-manager/selection-manager.mock";
import {IDialogService} from "../../../../../shared/widgets/bp-dialog/bp-dialog";
import {DialogServiceMock} from "../../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {ModalServiceInstanceMock} from "../../../../../shell/login/mocks.spec";
import {ISession} from "../../../../../shell/login/session.svc";
import {SessionSvcMock} from "../../../../../shell/login/session.svc.mock";
import {IArtifactReference, NodeType} from "../../diagram/presentation/graph/models";
import {UserTask} from "../../diagram/presentation/graph/shapes/user-task";
import {IModalScope} from "../base-modal-dialog-controller";
import {UserTaskModalController} from "./user-task-modal-controller";
import {UserTaskDialogModel} from "./userTaskDialogModel";
import * as angular from "angular";
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
    let session: ISession;
    let selectionManager: ISelectionManager;

    beforeEach(angular.mock.module("bp.editors.process", ($provide: ng.auto.IProvideService) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("createArtifactService", CreateArtifactService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("session", SessionSvcMock);
        $provide.service("selectionManager", SelectionManagerMock);
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
                       _session_: ISession,
                       _selectionManager_: ISelectionManager,
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
        session = _session_;
        selectionManager = _selectionManager_;
        $uibModalInstance = _$uibModalInstance_;
    }));

    describe("applyChanges", () => {
        let model: UserTaskDialogModel;
        let controller: UserTaskModalController;

        beforeEach(() => {
            model = new UserTaskDialogModel();
            model.isReadonly = false;
            model.isHistoricalVersion = false;
            model.action = "Custom Action";
            model.objective = "Custom Objective";
            model.label = "Custom Label";
            model.associatedArtifact = null;
            model.originalItem = createUserTaskNode();
            model.personaReference = {
                id: -1,
                projectId: null,
                name: "User",
                typePrefix: null,
                baseItemTypePredefined: ItemTypePredefined.Actor,
                projectName: null,
                link: null,
                version: null
            };

            controller = new UserTaskModalController(
                <IModalScope>$rootScope.$new(),
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
                session,
                selectionManager,
                $uibModalInstance,
                model);
        });

        it("throws error for read-only model", () => {
            // arrange
            let error: any;
            model.isReadonly = true;

            // act
            controller.applyChanges().catch((err: any) => error = err);
            $rootScope.$digest();

            // assert
            expect(error).toEqual(Error("Changes cannot be made or saved as this is a read-only item"));
        });

        it("throws error for historical model", () => {
            // arrange
            let error: any;
            model.isHistoricalVersion = true;

            // act
            controller.applyChanges().catch((err: any) => error = err);
            $rootScope.$digest();

            // assert
            expect(error).toEqual(Error("Changes cannot be made or saved as this is a read-only item"));
        });

        it("saves action", () => {
            // arrange
            model.action = "Test Action";

            // act
            controller.applyChanges();
            $rootScope.$digest();

            // assert
            expect(model.originalItem.action).toEqual(model.action);
        });

        it("saves associated artifact (include)", () => {
            // arrange
            model.associatedArtifact = <IArtifactReference>{
                id: 5,
                name: "associated",
                typePrefix: "PRO"
            };

            // act
            controller.applyChanges();
            $rootScope.$digest();

            // assert
            expect(model.originalItem.associatedArtifact).toEqual(model.associatedArtifact);
        });

        it("saves objective", () => {
            // arrange
            model.objective = "Test Objective";

            // act
            controller.applyChanges();
            $rootScope.$digest();

            // assert
            expect(model.originalItem.objective).toEqual(model.objective);
        });

        it("saves label", () => {
            // arrange
            model.label = "Test Label";

            // act
            controller.applyChanges();
            $rootScope.$digest();

            // assert
            expect(model.originalItem.label).toEqual(null);
        });

        it("saves persona reference", () => {
            // arrange
            model.personaReference = {
                id: 28,
                projectId: 1,
                name: "Custom Actor",
                typePrefix: "AC",
                baseItemTypePredefined: ItemTypePredefined.Actor,
                projectName: "Test Project",
                link: null,
                version: 26
            };

            // act
            controller.applyChanges();
            $rootScope.$digest();

            // assert
            expect(model.originalItem.personaReference).toEqual(model.personaReference);
        });
    });

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
});
