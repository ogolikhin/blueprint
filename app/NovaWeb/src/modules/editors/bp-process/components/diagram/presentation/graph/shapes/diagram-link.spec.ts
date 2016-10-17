import * as angular from "angular";
import {ProcessViewModel} from "../../../viewmodel/process-viewmodel";
import {ProcessGraph} from "../process-graph";
import {DiagramLink} from "./";
import {IDiagramNode} from "../models/";
import {Connector} from "./connector";
import {Label} from "../labels/label";
import {createUserDecisionWithoutUserTaskInFirstConditionModel} from "../../../../../models/test-model-factory";
import {ICommunicationManager, CommunicationManager} from "../../../../../../bp-process";
import {LocalizationServiceMock} from "../../../../../../../core/localization/localization.mock";
import {DialogService} from "../../../../../../../shared/widgets/bp-dialog";
import {ModalServiceMock} from "../../../../../../../shell/login/mocks.spec";

describe("DiagramLink unit tests", () => {
    let rootScope;
    let localScope;
    let container: HTMLElement;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
                       $rootScope: ng.IRootScopeService,
                       _communicationManager_: ICommunicationManager,
                       _dialogService_: DialogService,
                       _localization_: LocalizationServiceMock) => {

        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        let wrapper = document.createElement("DIV");
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
        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};
    }));
    describe("Label locations", () => {

        it("User Decision with no-op with label, correct location", () => {
            // arrange
            let userDecisionWidth = 120;
            let ud = 40;
            let testModel = createUserDecisionWithoutUserTaskInFirstConditionModel("Condition1", "Condition2");
            let processModel = new ProcessViewModel(testModel, communicationManager);

            let processGraph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization);

            // act
            processGraph.layout.render(true, null);

            let udNode: IDiagramNode = processGraph.layout.getNodeById(ud.toString());

            let firstLink: DiagramLink = <DiagramLink>udNode.getOutgoingLinks(processGraph.getMxGraphModel())[0];

            let linkLabel: Label = <Label>firstLink.textLabel;

            let x = Number(linkLabel.wrapperDiv.style.left.replace("px", ""));
            let y = Number(linkLabel.wrapperDiv.style.top.replace("px", ""));

            let heightOfString = mxUtils.getSizeForString("Condition1", Connector.LABEL_SIZE, Connector.LABEL_FONT, null).height;

            // assert
            expect(x).toBe(udNode.getCenter().x + userDecisionWidth / 2);
            expect(y).toBe(udNode.getCenter().y + 10 + heightOfString / 2);

        });
        it("User Decision with no-op with label, width label correct", () => {
            // arrange
            let ud = 40;
            let testModel = createUserDecisionWithoutUserTaskInFirstConditionModel();
            let processModel = new ProcessViewModel(testModel, communicationManager);
            let processGraph = new ProcessGraph(rootScope, localScope, container, processModel, dialogService, localization);

            // act
            processGraph.layout.render(true, null);

            let udNode: IDiagramNode = processGraph.layout.getNodeById(ud.toString());

            let firstLink: DiagramLink = <DiagramLink> udNode.getOutgoingLinks(processGraph.getMxGraphModel())[0];

            let nextNode: IDiagramNode = processGraph.layout.getNodeById(firstLink.targetNode.model.id.toString());

            let linkLabel: Label = <Label>firstLink.textLabel;

            let width = Number(linkLabel.wrapperDiv.style.width.replace("px", ""));
            let spaceBetweenNode = nextNode.getCenter().x - nextNode.getWidth() / 2 - (udNode.getCenter().x + udNode.getWidth() / 2);

            // assert
            expect(width).toBe(spaceBetweenNode);

        });
    });


});
