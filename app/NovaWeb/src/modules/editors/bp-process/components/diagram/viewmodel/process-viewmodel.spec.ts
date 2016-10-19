import * as angular from "angular";
import {IArtifactService} from "../../../../../managers/artifact-manager/";
import {ArtifactServiceMock} from "../../../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {ProcessServiceMock} from "../../../services/process.svc.mock";
import {IProcessService} from "../../../services/process.svc";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {StatefulProcessSubArtifact} from "../../../process-subartifact";
import {Models} from "../../../../../main/models";
import {ProcessViewModel} from "./process-viewmodel";
import * as TestModels from "../../../models/test-model-factory";
import {ProcessShapeType, ProcessType} from "../../../models//enums";
import {
    IStatefulProcessArtifactServices,
    StatefulArtifactServices,
    StatefulProcessArtifactServices
} from "../../../../../managers/artifact-manager/services";
import {ILoadingOverlayService, LoadingOverlayService} from "../../../../../core/loading-overlay";

describe("ProcessViewModel", () => {
    let services: IStatefulProcessArtifactServices;
    let $q: ng.IQService;
    let $rootScope: ng.IRootScopeService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("processService", ProcessServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
    }));

    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$q_: ng.IQService,
                       artifactService: IArtifactService,
                       processService: IProcessService,
                       loadingOverlayService: ILoadingOverlayService) => {
        $rootScope = _$rootScope_;
        $q = _$q_;
        let artitfactServices = new StatefulArtifactServices(_$q_, null, null, null, null, artifactService, null, null, null, loadingOverlayService);
        services = new StatefulProcessArtifactServices(artitfactServices, _$q_, processService);
    }));
    it("test add stateful Shape", () => {
        //Arrange

        const artifact = {
            id: 1,
            name: "",
            projectId: 1,
            predefinedType: Models.ItemTypePredefined.TextualRequirement
        } as Models.IArtifact;

        let processArtifact = new StatefulProcessArtifact(artifact, services);

        processArtifact.shapes = [];

        let processModel = new ProcessViewModel(processArtifact, null, null, null);

        let processShape = TestModels.createShapeModel(ProcessShapeType.UserTask, 20, 0, 0);

        //Act
        processModel.addShape(processShape);

        //Assert
        expect(processArtifact.subArtifactCollection.list().length).toBe(1);
    });

    it("test remove stateful shape", () => {
        //Arrange

        const artifact = {
            id: 1,
            name: "",
            projectId: 1,
            predefinedType: Models.ItemTypePredefined.TextualRequirement
        } as Models.IArtifact;

        let shapeId = 20;

        let processArtifact = new StatefulProcessArtifact(artifact, services);

        processArtifact.shapes = [];

        let processModel = new ProcessViewModel(processArtifact, null, null, null);

        let processShape = TestModels.createShapeModel(ProcessShapeType.UserTask, shapeId, 0, 0);
        processArtifact.shapes.push(processShape);

        let statefulProcessShape = new StatefulProcessSubArtifact(processArtifact,
            processShape, processArtifact.getServices());
        processArtifact.subArtifactCollection.add(statefulProcessShape);

        //Act
        processModel.removeShape(shapeId);

        //Assert
        expect(processArtifact.subArtifactCollection.list().length).toBe(0);
    });
});
