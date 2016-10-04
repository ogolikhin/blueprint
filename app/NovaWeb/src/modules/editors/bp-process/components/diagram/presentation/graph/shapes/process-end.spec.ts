import * as angular from "angular";
import {ProcessEnd} from "./";
import {ShapesFactory} from "./shapes-factory";
import {ProcessGraph} from "../process-graph";
import {ProcessModel, ProcessShapeModel} from "../../../../../models/process-models";
import {ProcessShapeType} from "../../../../../models/enums";
import {ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {NodeType} from "../models/";
import {ICommunicationManager, CommunicationManager} from "../../../../../../bp-process"; 
import {LocalizationServiceMock} from "../../../../../../../core/localization/localization.mock";
import {DialogService} from "../../../../../../../shared/widgets/bp-dialog";
import { ModalServiceMock } from "../../../../../../../shell/login/mocks.spec";

describe("ProcessEnd test", () => {
    var shapesFactory: ShapesFactory;
    var localScope, rootScope, wrapper, container;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(inject((
        _$window_: ng.IWindowService, 
        $rootScope: ng.IRootScopeService, 
        _communicationManager_: ICommunicationManager,
        _dialogService_: DialogService,
        _localization_: LocalizationServiceMock) => {
        rootScope = $rootScope;
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {};
        $rootScope["config"].labels = {};
        shapesFactory = new ShapesFactory($rootScope);
        localScope = { graphContainer: container, graphWrapper: wrapper, isSpa: false };
    }));


    it("Test ProcessEnd class", () => {
        // Arrange
        var testModel = new ProcessShapeModel(30);
        testModel.propertyValues = shapesFactory.createPropertyValuesForSystemTaskShape();
        testModel.propertyValues["clientType"].value = ProcessShapeType.End;
        testModel.propertyValues["x"].value = 0;

        let processModel = new ProcessModel();
        let viewModel = new ProcessViewModel(processModel);
        viewModel.communicationManager = communicationManager;
        viewModel.isReadonly = false;

        // Act
        let graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization);

        var node = new ProcessEnd(testModel);
        node.render(graph, 30, 30, false);

        //Assert
        expect(graph.getNodeById("30").getNodeType()).toEqual(NodeType.ProcessEnd);
    });
});