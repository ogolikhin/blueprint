import * as angular from "angular";
import "angular-mocks";
require("script!mxClient");
import "../../..";
import { ModalServiceInstanceMock } from "../../../../../shell/login/mocks.spec";
import { ILocalizationService } from "../../../../../core/localization";
import { LocalizationServiceMock } from "../../../../../core/localization/localization.mock";
import { IModalScope } from "../base-modal-dialog-controller";
import { ProcessGraph } from "../../diagram/presentation/graph/process-graph";
import { IProcessGraph, IDiagramNode, IDiagramLink, NodeType, ICondition, IDecision } from "../../diagram/presentation/graph/models";
import { ProcessEvents } from "../../diagram/process-diagram-communication";
import { ProcessDeleteHelper } from "../../diagram/presentation/graph/process-delete-helper";
import { IDialogSettings, IDialogService } from "../../../../../shared";
import { SystemTaskDialogModel } from "./sub-artifact-dialog-model";
import { SystemTaskModalController } from "./system-task-modal-controller";
import { IArtifactReference, ArtifactReference, ProcessModel, ISystemTaskShape } from "../../../models/process-models";
import { DiagramNodeElement, UserTask, SystemTask } from "../../diagram/presentation/graph/shapes/";
import { ShapeModelMock } from "../../diagram/presentation/graph/shapes/shape-model.mock";
import { ShapesFactory } from "../../diagram/presentation/graph/shapes/shapes-factory";
import { Models } from "../../../../../main/models/";
import { StatefulArtifactFactoryMock } from "../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import { ArtifactServiceMock } from "../../../../../managers/artifact-manager/artifact/artifact.svc.mock";
import { StatefulProcessSubArtifact } from "../../../process-subartifact";
import { StatefulProcessArtifact } from "../../../process-artifact";

describe("SystemTaskModalController", () => {
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

    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
        _$timeout_: ng.ITimeoutService,
        _$location_: ng.ILocationService,
        _localization_: ILocalizationService,
        _$uibModalInstance_: ng.ui.bootstrap.IModalServiceInstance
    ) => {
        $rootScope = _$rootScope_;
        $timeout = _$timeout_;
        localization = _localization_;
        $uibModalInstance = _$uibModalInstance_;
        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
    }));

    function createSystemTaskNode(): SystemTask {
        return <SystemTask>{
            model: { id: 1 },
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
                localization,
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
                localization,
                $uibModalInstance,
                model);
            const artifactReference: IArtifactReference = <IArtifactReference>{
                typePrefix: "<Inaccessible>"
            };

            // act
            const label = controller.formatIncludeLabel(artifactReference);

            // assert
            expect(localizationSpy).toHaveBeenCalledWith("HttpError_Forbidden");

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
                localization,
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
                localization,
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
            model.persona = "PM/PO";
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
                localization,
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
            expect(model.originalItem.persona).toEqual(model.persona);
        });

        describe("Stateful Changes - save data - ", () => {
            let model: SystemTaskDialogModel;
            let controller: SystemTaskModalController;
            let statefulArtifact: StatefulProcessArtifact;
            beforeEach(() => {
                const factory = new StatefulArtifactFactoryMock();
                const artifact: Models.IArtifact = ArtifactServiceMock.createArtifact(1);
                artifact.predefinedType = Models.ItemTypePredefined.Process;
                statefulArtifact = <StatefulProcessArtifact>factory.createStatefulArtifact(artifact);

                const processModel = new ProcessModel();
                const mock = ShapeModelMock.instance().SystemTaskMock();
                processModel.shapes.push(mock);

                factory.populateStatefulProcessWithProcessModel(statefulArtifact, processModel);
                const statefulSubArtifact = <StatefulProcessSubArtifact>statefulArtifact.subArtifactCollection.get(mock.id);
                const shapesFactory = new ShapesFactory($rootScope, factory);
                const diagramNode = new SystemTask(<ISystemTaskShape>statefulArtifact.shapes[0], $rootScope, "", null, shapesFactory);
                model = new SystemTaskDialogModel();
                model.originalItem = diagramNode;
                model.isReadonly = false;
                model.isHistoricalVersion = false;
                model.action = diagramNode.action;
                model.imageId = diagramNode.imageId;
                model.associatedImageUrl = diagramNode.associatedImageUrl;
                model.label = diagramNode.label;
                model.persona = diagramNode.persona;
                model.associatedArtifact = diagramNode.associatedArtifact;
                model.originalItem = diagramNode;

                const $scope = <IModalScope>$rootScope.$new();
                const localizationSpy = spyOn(localization, "get");
                controller = new SystemTaskModalController($scope,
                    $rootScope,
                    $timeout,
                    dialogService,
                    localization,
                    $uibModalInstance,
                    model);
            });


            it("persona changes, triggers lock and is dirty", () => {

                spyOn(statefulArtifact, "refresh")();
                const lockSpy = spyOn(statefulArtifact, "lock");

                model.persona = "new persona";

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
                    link: "",
                    typePrefix: ""
                }
                model.associatedArtifact = artifactReference;

                controller.saveData();

                expect(lockSpy).toHaveBeenCalled();
                expect(statefulArtifact.artifactState.dirty).toBeTruthy();
            });
        });

    });


});