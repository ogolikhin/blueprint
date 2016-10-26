import * as angular from "angular";
import { ProcessViewModel } from "../../../viewmodel/process-viewmodel";
import { ProcessGraph } from "../process-graph";
import { DiagramLink } from "./";
import { IDiagramNode } from "../models/";
import { Connector } from "./connector";
import { Label } from "../labels/label";
import { createUserDecisionWithoutUserTaskInFirstConditionModel } from "../../../../../models/test-model-factory";
import { ICommunicationManager, CommunicationManager } from "../../../../../../bp-process";
import { LocalizationServiceMock } from "../../../../../../../core/localization/localization.mock";
import { DialogService } from "../../../../../../../shared/widgets/bp-dialog";
import { ModalServiceMock } from "../../../../../../../shell/login/mocks.spec";
import { IStatefulArtifactFactory } from "../../../../../../../managers/artifact-manager/";
import { StatefulArtifactFactoryMock, IStatefulArtifactFactoryMock } from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import { ArtifactServiceMock } from "../../../../../../../managers/artifact-manager/artifact/artifact.svc.mock";
import { StatefulProcessArtifact } from "../../../../../process-artifact";
import { Models } from "../../../../../../../main/models/";
import { ShapesFactory } from "./shapes-factory";

describe("DiagramLink unit tests", () => {
    let rootScope;
    let localScope;
    let container: HTMLElement;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock,
        statefulArtifactFactory: IStatefulArtifactFactoryMock,
        shapesFactory: ShapesFactory;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("shapesFactory", ShapesFactory);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
        $rootScope: ng.IRootScopeService,
        _communicationManager_: ICommunicationManager,
        _dialogService_: DialogService,
        _localization_: LocalizationServiceMock,
        _statefulArtifactFactory_: IStatefulArtifactFactoryMock,
        _shapesFactory_: ShapesFactory) => {

        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        statefulArtifactFactory = _statefulArtifactFactory_;
        shapesFactory = _shapesFactory_;

        const wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);

        $rootScope["config"] = {
            labels: {
                "ST_Persona_Label": "Persona",
                "ST_Colors_Label": "Color",
                "ST_Comments_Label": "Comments",
                "ST_New_User_Task_Label": "New User Task",
                "ST_New_User_Task_Persona": "User",
                "ST_New_User_Decision_Label": "New Decision",
                "ST_New_System_Task_Label": "New System Task",
                "ST_New_System_Task_Persona": "System"
            }
        };

        rootScope = $rootScope;
        localScope = { graphContainer: container, graphWrapper: wrapper, isSpa: false };
    }));
    describe("Label locations", () => {

        it("User Decision with no-op with label, correct location", () => {
            // arrange
            const userDecisionWidth = 120;
            const ud = 40;
            const testModel = createUserDecisionWithoutUserTaskInFirstConditionModel("Condition1", "Condition2");
            const processModel = new ProcessViewModel(testModel, communicationManager);

            const processGraph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            // act
            processGraph.layout.render(true, null);

            const udNode: IDiagramNode = processGraph.layout.getNodeById(ud.toString());

            const firstLink: DiagramLink = <DiagramLink>udNode.getOutgoingLinks(processGraph.getMxGraphModel())[0];

            const linkLabel: Label = <Label>firstLink.textLabel;

            const x = Number(linkLabel.wrapperDiv.style.left.replace("px", ""));
            const y = Number(linkLabel.wrapperDiv.style.top.replace("px", ""));

            const heightOfString = mxUtils.getSizeForString("Condition1", Connector.LABEL_SIZE, Connector.LABEL_FONT, null).height;

            // assert
            expect(x).toBe(udNode.getCenter().x + userDecisionWidth / 2);
            expect(y).toBe(udNode.getCenter().y + 10 + heightOfString / 2);

        });
        it("User Decision with no-op with label, width label correct", () => {
            // arrange
            const ud = 40;
            const testModel = createUserDecisionWithoutUserTaskInFirstConditionModel();
            const processModel = new ProcessViewModel(testModel, communicationManager);
            const processGraph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            // act
            processGraph.layout.render(true, null);

            const udNode: IDiagramNode = processGraph.layout.getNodeById(ud.toString());

            const firstLink: DiagramLink = <DiagramLink>udNode.getOutgoingLinks(processGraph.getMxGraphModel())[0];

            const nextNode: IDiagramNode = processGraph.layout.getNodeById(firstLink.targetNode.model.id.toString());

            const linkLabel: Label = <Label>firstLink.textLabel;

            const width = Number(linkLabel.wrapperDiv.style.width.replace("px", ""));
            const spaceBetweenNode = nextNode.getCenter().x - nextNode.getWidth() / 2 - (udNode.getCenter().x + udNode.getWidth() / 2);

            // assert
            expect(width).toBe(spaceBetweenNode);

        });
    });

    describe("Label modification", () => {
        it("statefulArtifact's lock, should have been called", () => {
            const userDecisionWidth = 120;
            const ud = 40;
            const testModel = createUserDecisionWithoutUserTaskInFirstConditionModel("Condition1", "Condition2");
            const artifact: Models.IArtifact = ArtifactServiceMock.createArtifact(1);
            artifact.predefinedType = Models.ItemTypePredefined.Process;
            const statefulArtifact = statefulArtifactFactory.createStatefulArtifact(artifact);
            statefulArtifactFactory.populateStatefulProcessWithProcessModel(<StatefulProcessArtifact>statefulArtifact, testModel);
            const processModel = new ProcessViewModel(statefulArtifact, communicationManager);

            const processGraph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);
            processGraph.layout.render(true, null);

            const udNode: IDiagramNode = processGraph.layout.getNodeById(ud.toString());
            const firstLink: DiagramLink = <DiagramLink>udNode.getOutgoingLinks(processGraph.getMxGraphModel())[0];
            const spy = spyOn(statefulArtifact, "lock");
            spyOn(statefulArtifact, "refresh")();

            // act
            firstLink.label = "Testing 123";

            // arrange
            expect(spy).toHaveBeenCalled();
        });
        it("statefulArtifact's lock, artifact state is dirty", () => {
            const userDecisionWidth = 120;
            const ud = 40;
            const testModel = createUserDecisionWithoutUserTaskInFirstConditionModel("Condition1", "Condition2");
            const artifact: Models.IArtifact = ArtifactServiceMock.createArtifact(1);
            artifact.predefinedType = Models.ItemTypePredefined.Process;
            const statefulArtifact = statefulArtifactFactory.createStatefulArtifact(artifact);
            statefulArtifactFactory.populateStatefulProcessWithProcessModel(<StatefulProcessArtifact>statefulArtifact, testModel);
            const processModel = new ProcessViewModel(statefulArtifact, communicationManager);

            const processGraph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization, shapesFactory, null, null, null);

            // act
            processGraph.layout.render(true, null);

            const udNode: IDiagramNode = processGraph.layout.getNodeById(ud.toString());
            const firstLink: DiagramLink = <DiagramLink>udNode.getOutgoingLinks(processGraph.getMxGraphModel())[0];
            const spy = spyOn(statefulArtifact, "lock");
            spyOn(statefulArtifact, "refresh")();

            // act
            firstLink.label = "Testing 123";

            // arrange
            expect(statefulArtifact.artifactState.dirty).toBeTruthy();
        });
    });

});
