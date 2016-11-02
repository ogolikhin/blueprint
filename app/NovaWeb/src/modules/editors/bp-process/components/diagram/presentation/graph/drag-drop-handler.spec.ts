import * as angular from "angular";
require("script!mxClient");
import {ProcessGraph} from "./process-graph";
import {MessageServiceMock} from "../../../../../../core/messages/message.mock";
import {IMessageService} from "../../../../../../core/messages/message.svc";
import {ProcessViewModel} from "../../viewmodel/process-viewmodel";
import * as TestModels from "../../../../models/test-model-factory";
import {ICommunicationManager, CommunicationManager} from "../../../../../bp-process";
import {LocalizationServiceMock} from "../../../../../../core/localization/localization.mock";
import {DialogService} from "../../../../../../shared/widgets/bp-dialog";
import {ModalServiceMock} from "../../../../../../shell/login/mocks.spec";
import {ShapesFactory} from "./shapes/shapes-factory";
import {IStatefulArtifactFactory} from "../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {StatefulSubArtifactCollection} from "../../../../../../managers/artifact-manager/sub-artifact";
import {ChangeSetCollector} from "../../../../../../managers/artifact-manager/changeset";

class ExecutionEnvironmentDetectorMock {
    private browserInfo: any;

    constructor() {
        this.browserInfo = { msie: false, firefox: false, version: 0 };
    }

    public getBrowserInfo(): any {
        return this.browserInfo;
    }
}

describe("Drag-drop test", () => {
    let msgService: IMessageService,
        localScope,
        rootScope,
        wrapper,
        container,
        communicationManager: ICommunicationManager,
        shapesFactoryService: ShapesFactory,
        dialogService: DialogService,
        localization: LocalizationServiceMock,
        statefulArtifactFactory: IStatefulArtifactFactory,
        shapesFactory: ShapesFactory;

    let w: any = window;
    w.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("shapesFactory", ShapesFactory);
    }));

    let setProcessViewModel = function (model) {
        model.subArtifactCollection = new StatefulSubArtifactCollection(model, null);
        model.changeset = new ChangeSetCollector(model);
        model.artifactState = {dirty: false};
        model.lock = function (){};
        const processModel = new ProcessViewModel(model, communicationManager);
        return processModel;
    };

    beforeEach(inject((_$window_: ng.IWindowService,
                       $rootScope: ng.IRootScopeService,
                       messageService: IMessageService,
                       _communicationManager_: ICommunicationManager,
                       _dialogService_: DialogService,
                       _localization_: LocalizationServiceMock,
                       _statefulArtifactFactory_: IStatefulArtifactFactory,
                       _shapesFactory_: ShapesFactory
    ) => {
        rootScope = $rootScope;
        msgService = messageService;
        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        statefulArtifactFactory = _statefulArtifactFactory_;
        shapesFactory = _shapesFactory_;

        $rootScope["config"] = {
            labels: {
                "ST_Persona_Label": "Persona",
                "ST_Colors_Label": "Color",
                "ST_Comments_Label": "Comments",
                "ST_New_User_Task_Label": "New User Task",
                "ST_New_User_Task_Persona": "User",
                "ST_New_User_Decision_Label": "New Decision",
                "ST_New_System_Task_Label": "New System Task",
                "ST_New_System_Task_Persona": "System",
                "ST_Eighty_Percent_of_Shape_Limit_Reached": "The Process now has {0} of the maximum {1} shapes",
                "ST_Shape_Limit_Exceeded": "The Process will exceed the maximum {0} shapes",
                "ST_Shape_Limit_Exceeded_Initial_Load": "The Process will exceed the maximum {0} shapes"
            },
            settings: {
                "StorytellerShapeLimit": 100,
                "StorytellerIsSMB": "false"
            }
        };

        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};

        localScope["vm"] = {
            "$rootScope": rootScope
        };

        shapesFactoryService = new ShapesFactory(rootScope, statefulArtifactFactory);
    }));

    it("Test isValidDropSource method.", () => {
        // Arrange
        let testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

        // Act
        graph.render(false, null);

        //Assert
        expect(graph.dragDropHandler.isValidDropSource(graph.getNodeById("36"))).toBeTruthy();
        expect(graph.dragDropHandler.isValidDropSource(graph.getNodeById("37"))).not.toBeTruthy();
    });

    it("Test createDragPreview method.", () => {
        // Arrange
        let testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

        // Act
        graph.render(true, null);
        let preview: HTMLDivElement = graph.dragDropHandler.createDragPreview();

        //Assert
        expect(preview.style.width).toEqual("60px");
        expect(preview.style.height).toEqual("75px");
    });

    it("Test highlightDropTarget method.", () => {
        // Arrange
        let testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);
        let pt = {
            getX() { return 130; },
            getY() { return 120; }
        };

        graph.dragDropHandler.moveCell = graph.getNodeById("20");
        graph.dragDropHandler.createDragPreview();
        let getDropEdgeStateSpy = spyOn(graph.layout, "getDropEdgeState");

        // Act
        graph.dragDropHandler.highlightDropTarget(pt);

        // Assert
        expect(getDropEdgeStateSpy).toHaveBeenCalled();
    });

    it("Test reset method.", () => {
        // Arrange
        let testModel = TestModels.createLargeTestModel();
        const processModel = setProcessViewModel(testModel);
        const graph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
        graph.render(true, null);
        graph.dragDropHandler.moveCell = graph.getNodeById("20");

        // Act
        graph.dragDropHandler.reset();

        // Assert
        expect(graph.dragDropHandler.moveCell).toBeNull();
    });

});
