import {MessageServiceMock} from "../../../../../core/messages/message.mock";
import {LoadingOverlayServiceMock} from "../../../../../core/loading-overlay/loading-overlay.svc.mock";
import {DialogServiceMock} from "../../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {ILoadingOverlayService} from "../../../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../../../core/messages/message.svc";
import {CreateArtifactService, ICreateArtifactService} from "../../../../../main/components/projectControls/create-artifact.svc";
import * as angular from "angular";
import "angular-mocks";
import "../../..";
import {ModalServiceInstanceMock} from "../../../../../shell/login/mocks.spec";
import {LocalizationServiceMock} from "../../../../../core/localization/localization.mock";
import {IModalScope} from "../base-modal-dialog-controller";
import {NodeType} from "../../diagram/presentation/graph/models";
import {IDialogService} from "../../../../../shared";
import {SystemTaskDialogModel} from "./sub-artifact-dialog-model";
import {SystemTaskModalController} from "./system-task-modal-controller";
import {IArtifactReference, ArtifactReference, ProcessModel, ISystemTaskShape} from "../../../models/process-models";
import {SystemTask} from "../../diagram/presentation/graph/shapes/";
import {ShapeModelMock} from "../../diagram/presentation/graph/shapes/shape-model.mock";
import {ShapesFactory} from "../../diagram/presentation/graph/shapes/shapes-factory";
import {Models} from "../../../../../main/models/";
import {StatefulArtifactFactoryMock} from "../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ArtifactServiceMock} from "../../../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {StatefulProcessSubArtifact} from "../../../process-subartifact";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {ILocalizationService} from "../../../../../core/localization/localizationService";
require("script!mxClient");
import {ProcessViewModel, IProcessViewModel} from "../../diagram/viewmodel/process-viewmodel";
import {CommunicationManager, ICommunicationManager} from "../../../services/communication-manager";
import {ProcessGraph} from "../../diagram/presentation/graph/process-graph";
import {IArtifactService, IStatefulArtifactFactory} from "../../../../../managers/artifact-manager/artifact";

describe("SystemTaskModalController", () => {
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
    let communicationManager: ICommunicationManager;

    beforeEach(angular.mock.module("bp.editors.process", ($provide: ng.auto.IProvideService) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("communicationManager", CommunicationManager);
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
                       _communicationManager_: ICommunicationManager,
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
        communicationManager = _communicationManager_;
        $q = _$q_;
        createArtifactService = _createArtifactService_;
        statefulArtifactFactory = _statefulArtifactFactory_;
        messageService = _messageService_;
        artifactService = _artifactService_;
        loadingOverlayService = _loadingOverlayService_;
        $uibModalInstance = _$uibModalInstance_;
        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
    }));

    function createSystemTaskNode(): SystemTask {
        return <SystemTask>{
            model: {id: 1},
            direction: null,
            action: null,
            label: null,
            row: null,
            column: null,
            newShapeColor: null,
            getNodeType: () => NodeType.SystemTask
        };
    }

    describe("retrieve included artifact info ", () => {

        it("when no included artifact, label should be empty", () => {

            // arrange
            const model = new SystemTaskDialogModel();
            model.isReadonly = true;
            model.isHistoricalVersion = false;

            const $scope = <IModalScope>$rootScope.$new();
            const localizationSpy = spyOn(localization, "get");

            const controller = new SystemTaskModalController($scope,
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

            // act
            const label = controller.formatIncludeLabel(artifactReference);

            // assert
            expect(label).toBe("");

        });

        it("when included artifact is not accessible, label should indicate forbidden information", () => {

            // arrange
            const model = new SystemTaskDialogModel();
            model.isReadonly = true;
            model.isHistoricalVersion = false;

            const $scope = <IModalScope>$rootScope.$new();
            const localizationSpy = spyOn(localization, "get");
            const controller = new SystemTaskModalController($scope,
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
            const artifactReference: IArtifactReference = <IArtifactReference>{
                typePrefix: "<Inaccessible>"
            };

            // act
            const label = controller.formatIncludeLabel(artifactReference);

            // assert
            expect(localizationSpy).toHaveBeenCalledWith("ST_Inaccessible_Include_Artifact_Label");

        });

        it("with proper included artifact, label should contain prefix, id and name", () => {

            // arrange
            const model = new SystemTaskDialogModel();
            model.isReadonly = true;
            model.isHistoricalVersion = false;

            const $scope = <IModalScope>$rootScope.$new();
            const localizationSpy = spyOn(localization, "get");
            const controller = new SystemTaskModalController($scope,
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

            const artifactReference: IArtifactReference = <IArtifactReference>{
                typePrefix: "PR",
                id: 1,
                name: "This Artifact"
            };

            const expectedLabel = artifactReference.typePrefix + artifactReference.id + " - " + artifactReference.name;

            // act
            const label = controller.formatIncludeLabel(artifactReference);

            // assert
            expect(label).toEqual(expectedLabel);

        });

    });

    describe("model is readonly ", () => {

        it("save data should not occur", () => {

            // arrange
            const model = new SystemTaskDialogModel();
            model.isReadonly = true;
            model.isHistoricalVersion = false;

            const $scope = <IModalScope>$rootScope.$new();
            const localizationSpy = spyOn(localization, "get");
            const controller = new SystemTaskModalController($scope,
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

            // act and assert
            expect(controller.saveData).toThrow();
        });

    });

    describe("model is non-readonly ", () => {

        it("save data should be successful", () => {

            // arrange
            const model = new SystemTaskDialogModel();
            model.isReadonly = false;
            model.isHistoricalVersion = false;
            model.action = "Custom Action";
            model.imageId = "lll";
            model.associatedImageUrl = "lll-lll";
            model.label = "Custom Label";
            model.personaReference = {
                id: 1,
                projectId: 1,
                name: "new persona",
                typePrefix: "PRO",
                baseItemTypePredefined: Models.ItemTypePredefined.Actor,
                projectName: "test project",
                link: null,
                version: null
            };
            model.associatedArtifact = <IArtifactReference>{
                id: 5,
                name: "associated",
                typePrefix: "PRO"
            };
            model.originalItem = createSystemTaskNode();

            const $scope = <IModalScope>$rootScope.$new();
            const localizationSpy = spyOn(localization, "get");
            const controller = new SystemTaskModalController($scope,
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

            //assert
            expect(model.originalItem.action).toEqual(model.action);
            expect(model.originalItem.associatedArtifact).toEqual(model.associatedArtifact);
            expect(model.originalItem.associatedImageUrl).toEqual(model.associatedImageUrl);
            expect(model.originalItem.imageId).toEqual(model.imageId);
            expect(model.originalItem.label).toEqual(null);
            expect(model.originalItem.personaReference).toEqual(model.personaReference);
        });

        describe("Stateful Changes - save data - ", () => {
            let model: SystemTaskDialogModel;
            let controller: SystemTaskModalController;
            let statefulArtifact: StatefulProcessArtifact;
            let viewModel: ProcessViewModel;
            let wrapper, container, localScope;

            beforeEach(() => {
                const factory = new StatefulArtifactFactoryMock();
                const artifact: Models.IArtifact = ArtifactServiceMock.createArtifact(1);
                artifact.predefinedType = Models.ItemTypePredefined.Process;
                statefulArtifact = <StatefulProcessArtifact>factory.createStatefulArtifact(artifact);

                const processModel = new ProcessModel();

                const mock = ShapeModelMock.instance().SystemTaskMock();
                processModel.shapes.push(mock);
                processModel.userTaskPersonaReferenceList = [];
                processModel.systemTaskPersonaReferenceList = [];


                wrapper = document.createElement("DIV");
                container = document.createElement("DIV");
                wrapper.appendChild(container);
                document.body.appendChild(wrapper);


                localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};

                viewModel = new ProcessViewModel(processModel, communicationManager);

                factory.populateStatefulProcessWithProcessModel(statefulArtifact, processModel);
                const statefulSubArtifact = <StatefulProcessSubArtifact>statefulArtifact.subArtifactCollection.get(mock.id);
                const shapesFactory = new ShapesFactory($rootScope, factory);

                const graph = new ProcessGraph($rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);
                const diagramNode = new SystemTask(<ISystemTaskShape>statefulArtifact.shapes[0], $rootScope, null, null, shapesFactory);
                diagramNode.render(graph, 80, 80, false);
                diagramNode.renderLabels();
                model = new SystemTaskDialogModel();
                model.originalItem = diagramNode;
                model.isReadonly = false;
                model.isHistoricalVersion = false;
                model.action = diagramNode.action;
                model.imageId = diagramNode.imageId;
                model.associatedImageUrl = diagramNode.associatedImageUrl;
                model.label = diagramNode.label;
                model.associatedArtifact = diagramNode.associatedArtifact;
                model.originalItem = diagramNode;
                model.personaReference = {
                    id: -1,
                    projectId: null,
                    name: "System",
                    typePrefix: null,
                    baseItemTypePredefined: Models.ItemTypePredefined.Actor,
                    projectName: null,
                    link: null,
                    version: null
                };

                const $scope = <IModalScope>$rootScope.$new();
                const localizationSpy = spyOn(localization, "get");
                controller = new SystemTaskModalController($scope,
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
            });


            it("personaReference changes, triggers lock and is dirty", () => {

                spyOn(statefulArtifact, "refresh")();
                const lockSpy = spyOn(statefulArtifact, "lock");

                model.personaReference = {
                    id: 1,
                    projectId: 1,
                    name: "new persona",
                    typePrefix: "PRO",
                    baseItemTypePredefined: Models.ItemTypePredefined.Actor,
                    projectName: "test project",
                    link: null,
                    version: null
                };

                controller.saveData();

                expect(lockSpy).toHaveBeenCalled();
                expect(statefulArtifact.artifactState.dirty).toBeTruthy();
            });


            it("action (label) changes, triggers lock and is dirty", () => {

                spyOn(statefulArtifact, "refresh")();
                const lockSpy = spyOn(statefulArtifact, "lock");

                model.action = "new system action";

                controller.saveData();

                expect(lockSpy).toHaveBeenCalled();
                expect(statefulArtifact.artifactState.dirty).toBeTruthy();
            });

            it("image changes, triggers lock and is dirty", () => {

                spyOn(statefulArtifact, "refresh")();
                const lockSpy = spyOn(statefulArtifact, "lock");

                model.imageId = "6b021f82-0e3c-4df7-8eb2-74730b92dc3a";

                controller.saveData();

                expect(lockSpy).toHaveBeenCalled();
                expect(statefulArtifact.artifactState.dirty).toBeTruthy();
            });

            it("associated artifact changes, triggers lock and is dirty", () => {

                spyOn(statefulArtifact, "refresh")();
                const lockSpy = spyOn(statefulArtifact, "lock");

                const artifactReference: ArtifactReference = {
                    baseItemTypePredefined: Models.ItemTypePredefined.Actor,
                    id: 99,
                    name: "test actor",
                    projectId: 2,
                    projectName: "Test",
                    version: null,
                    link: "",
                    typePrefix: ""
                };

                model.associatedArtifact = artifactReference;

                controller.saveData();

                expect(lockSpy).toHaveBeenCalled();
                expect(statefulArtifact.artifactState.dirty).toBeTruthy();
            });
        });

    });


});
