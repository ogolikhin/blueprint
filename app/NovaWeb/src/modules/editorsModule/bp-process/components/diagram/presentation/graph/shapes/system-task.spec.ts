import {LocalizationServiceMock} from "../../../../../../../commonModule/localization/localization.service.mock";
import {Models} from "../../../../../../../main/models/";
import {PropertyTypePredefined} from "../../../../../../../main/models/enums";
import {ItemTypePredefined} from "../../../../../../../main/models/item-type-predefined";
import {IStatefulArtifactFactoryMock, StatefulArtifactFactoryMock} from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ArtifactServiceMock} from "../../../../../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {DialogService} from "../../../../../../../shared/widgets/bp-dialog";
import {ModalServiceMock} from "../../../../../../../shell/login/mocks.spec";
import {CommunicationManager, ICommunicationManager} from "../../../../../../bp-process";
import {ProcessShapeType, ProcessType} from "../../../../../models/enums";
import {ArtifactReference, ProcessLinkModel, ProcessModel, ProcessShapeModel} from "../../../../../models/process-models";
import {StatefulProcessArtifact} from "../../../../../process-artifact";
import {StatefulProcessSubArtifact} from "../../../../../process-subartifact";
import {IProcessViewModel, ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {NodeType} from "../models/";
import {ISystemTask, ISystemTaskShape} from "../models/";
import {ProcessGraph} from "../process-graph";
import {SystemTask} from "./";
import {ShapeModelMock} from "./shape-model.mock";
import {ShapesFactory} from "./shapes-factory";
import * as angular from "angular";

describe("SystemTask", () => {
    let statefulArtifactFactory: IStatefulArtifactFactoryMock;
    const LABEL_EDIT_MAXLENGTH = 35;
    const LABEL_VIEW_MAXLENGTH = 35;
    const PERSONA_EDIT_MAXLENGTH = 40;
    const PERSONA_VIEW_MAXLENGTH = 12;
    let shapesFactory: ShapesFactory;
    let localScope, rootScope: ng.IRootScopeService, wrapper, container;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock;

    const testArtifactReference2 = new ArtifactReference();

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
        $rootScope: ng.IRootScopeService,
        _communicationManager_: ICommunicationManager,
        _dialogService_: DialogService,
        _localization_: LocalizationServiceMock,
        _statefulArtifactFactory_: IStatefulArtifactFactoryMock) => {

        rootScope = $rootScope;
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        statefulArtifactFactory = _statefulArtifactFactory_;

        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
        shapesFactory = new ShapesFactory($rootScope, statefulArtifactFactory);
        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};
    }));


    it("Test SystemTask class", () => {
        // Arrange
        const processModel = new ProcessModel();
        const viewModel = new ProcessViewModel(processModel, communicationManager);

        // Act
        const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory);

        const node = new SystemTask(ShapeModelMock.instance().SystemTaskMock(), rootScope, null, null, shapesFactory);
        node.render(graph, 80, 120, false);
        node.renderLabels();

        node.label = "test label";
        node.description = "test description";
        node.associatedImageUrl = "test.jpg";
        node.imageId = "2";
        node.associatedArtifact = testArtifactReference2;

        //Assert
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.SystemTask);
        expect(node.label).toEqual("test label");
        expect(node.description).toEqual("test description");
        expect(node.associatedImageUrl).toEqual("test.jpg");
        expect(node.imageId).toEqual("2");
        expect(node.associatedArtifact).toEqual(testArtifactReference2);
    });

    describe("Test text elements", () => {

        it("Test latest personaReference value reuse", () => {
            // Arrange
            const processModel = new ProcessModel();
            const viewModel = new ProcessViewModel(processModel, communicationManager);
            const graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory, null, null, null);
            const testSystemTask = ShapeModelMock.instance().SystemTaskMock();

            const node = new SystemTask(testSystemTask, rootScope, null, null, shapesFactory);
            node.render(graph, 80, 80, false);
            node.renderLabels();
            node.personaReference =  {
                id: 1,
                projectId: 1,
                name: "new persona",
                typePrefix: "PRO",
                baseItemTypePredefined: ItemTypePredefined.Actor,
                projectName: "test project",
                link: null,
                version: null
            };


            // Act
            const node1 = new SystemTask(testSystemTask, rootScope, null, null, shapesFactory);

            //Assert
            expect(node1.personaReference).toEqual(node.personaReference);
        });

    });


    describe("when using default process", () => {

        let testModel;
        let processModel: IProcessViewModel;
        let graph: ProcessGraph;

        beforeEach(() => {

            const startModel = new ProcessShapeModel(11);
            startModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            startModel.propertyValues["clientType"].value = ProcessShapeType.Start;
            startModel.propertyValues["x"].value = 0;
            const preconditionModel = new ProcessShapeModel(22);
            preconditionModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            preconditionModel.propertyValues["clientType"].value = ProcessShapeType.PreconditionSystemTask;
            preconditionModel.propertyValues["x"].value = 1;
            const userTask = shapesFactory.createModelUserTaskShape(2, 1, 33, 2, 0);
            const systemTask = shapesFactory.createModelSystemTaskShape(2, 1, 44, 3, 0);
            const endModel = new ProcessShapeModel(55);
            endModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
            endModel.propertyValues["clientType"].value = ProcessShapeType.Start;
            endModel.propertyValues["x"].value = 4;

            testModel = new ProcessModel();
            testModel.propertyValues = {};
            testModel.propertyValues["clientType"] = shapesFactory.createClientTypeValueForProcess(ProcessType.UserToSystemProcess);
            testModel.shapes = [];
            testModel.shapes.push(startModel);
            testModel.shapes.push(preconditionModel);
            testModel.shapes.push(userTask);
            testModel.shapes.push(systemTask);
            testModel.shapes.push(endModel);
            testModel.links = [];
            testModel.links.push(new ProcessLinkModel(null, 11, 22));
            testModel.links.push(new ProcessLinkModel(null, 22, 33));
            testModel.links.push(new ProcessLinkModel(null, 33, 44));
            testModel.links.push(new ProcessLinkModel(null, 44, 55));
            processModel = new ProcessViewModel(testModel, communicationManager);

            graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory);

            graph.render(false, null);
        });
    });

    describe("StatefulSubArtifact changes", () => {
        let viewModel: IProcessViewModel,
            statefulArtifact: StatefulProcessArtifact,
            node: ISystemTask,
            graph: ProcessGraph,
            statefulSubArtifact: StatefulProcessSubArtifact;

        const newPersonaReference = {
            id: 2,
            projectId: 1,
            name: "added persona",
            typePrefix: "PRO",
            baseItemTypePredefined: ItemTypePredefined.Actor,
            projectName: "test project",
            link: null,
            version: null
        };
        beforeEach(() => {
            // arrange
            const processModel = new ProcessModel();
            const mock = ShapeModelMock.instance().SystemTaskMock();
            const artifact: Models.IArtifact = ArtifactServiceMock.createArtifact(1);
            artifact.predefinedType = ItemTypePredefined.Process;
            processModel.shapes.push(mock);

            statefulArtifact = <StatefulProcessArtifact>statefulArtifactFactory.createStatefulArtifact(artifact);
            statefulArtifactFactory.populateStatefulProcessWithProcessModel(statefulArtifact, processModel);
            statefulSubArtifact = <StatefulProcessSubArtifact>statefulArtifact.subArtifactCollection.get(mock.id);

            node = new SystemTask(<ISystemTaskShape>statefulArtifact.shapes[0], rootScope, null, null, shapesFactory);

            viewModel = new ProcessViewModel(statefulArtifact, communicationManager);
            viewModel.userTaskPersonaReferenceList = [];
            viewModel.systemTaskPersonaReferenceList = [node.personaReference];

            graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory);
        });

        it("when modifying personaReference - personaReference matches and personaReference list updated", () => {

            // arrange
            spyOn(statefulArtifact, "refresh")();
            spyOn(statefulArtifact, "lock")();

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.personaReference = newPersonaReference;

            // assert
            expect(statefulSubArtifact.specialProperties.get(PropertyTypePredefined.PersonaReference).value).toBe(node.personaReference.id);
            expect(viewModel.systemTaskPersonaReferenceList.length).toBe(2);
        });

        it("when modifying personaReference - attempt lock is called", () => {

            // arrange
            spyOn(statefulArtifact, "refresh")();
            const lockSpy = spyOn(statefulArtifact, "lock");

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.personaReference = newPersonaReference;

            // assert
            expect(lockSpy).toHaveBeenCalled();
        });

        it("when modifying personaReference - artifact state is dirty", () => {

            // arrange
            spyOn(statefulArtifact, "refresh")();
            const lockSpy = spyOn(statefulArtifact, "lock");

            // act
            node.render(graph, 80, 120, false);
            node.renderLabels();

            node.personaReference = newPersonaReference;

            // assert
            expect(statefulArtifact.artifactState.dirty).toBeTruthy();
        });
    });
});
