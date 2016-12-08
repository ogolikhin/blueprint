import {ProcessGraph} from "./process-graph";
import {IProcessGraph, IDiagramNode} from "./models/";
import {ShapesFactory} from "./shapes/shapes-factory";
import {IProcessViewModel, ProcessViewModel} from "../../viewmodel/process-viewmodel";
import {IProcessGraphModel, ProcessGraphModel} from "../../viewmodel/process-graph-model";
import {ICommunicationManager, CommunicationManager} from "../../../../../bp-process";
import {FileUploadService} from "../../../../../../core/file-upload/file-upload.svc.mock";
import {MessageServiceMock} from "../../../../../../core/messages/message.mock";
import {IMessageService} from "../../../../../../core/messages/message.svc";
import {DialogService} from "../../../../../../shared/widgets/bp-dialog";
import {LocalizationServiceMock} from "../../../../../../core/localization/localization.mock";
import {IStatefulArtifactFactory} from "../../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ModalServiceMock} from "../../../../../../shell/login/mocks.spec";

import * as ProcessModels from "../../../../models/process-models";
import * as TestModels from "../../../../models/test-model-factory";

describe("ProcessCopyPasteHelper tests", () => {
    let shapesFactory: ShapesFactory;
    let localScope, rootScope, timeout, wrapper, container, statefulArtifactFactory: IStatefulArtifactFactory;
    let communicationManager: ICommunicationManager;
    let dialogService: DialogService;
    let localization: LocalizationServiceMock;        
    let process: ProcessModels.IProcess;
    let clientModel: IProcessGraphModel;
    let viewModel: IProcessViewModel;
    let graph: IProcessGraph;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("$uibModal", ModalServiceMock);
        $provide.service("dialogService", DialogService);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("fileUploadService", FileUploadService);
        $provide.service("shapesFactory", ShapesFactory);
    }));

    beforeEach(inject((_$window_: ng.IWindowService,
                       $rootScope: ng.IRootScopeService,
                       $timeout: ng.ITimeoutService,
                       _communicationManager_: ICommunicationManager,
                       _dialogService_: DialogService,
                       _localization_: LocalizationServiceMock,
                       _statefulArtifactFactory_: IStatefulArtifactFactory,
                       _shapesFactory_: ShapesFactory) => {
        rootScope = $rootScope;
        timeout = $timeout;
        communicationManager = _communicationManager_;
        dialogService = _dialogService_;
        localization = _localization_;
        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);
        document.body.appendChild(wrapper);
        statefulArtifactFactory = _statefulArtifactFactory_;
        shapesFactory = _shapesFactory_;

        $rootScope["config"] = {};
        $rootScope["config"].labels = {
            "ST_Persona_Label": "Persona",
            "ST_Colors_Label": "Color",
            "ST_Comments_Label": "Comments",
            "ST_New_User_Task_Label": "New User Task",
            "ST_New_User_Task_Persona": "User",
            "ST_New_User_Decision_Label": "New User Decision",
            "ST_New_System_Task_Label": "New System Task",
            "ST_New_System_Task_Persona": "System",
            "ST_Delete_CannotDelete_UD_AtleastTwoConditions": "Decision points should have at least two conditions",
            "ST_Add_CannotAdd_MaximumConditionsReached": "Cannot add any more conditions because the maximum number of conditions has been reached.",
            "ST_Auto_Insert_Task": "The task and its associated shapes have been moved. Another task has been created at the old location."
        };
        localScope = {graphContainer: container, graphWrapper: wrapper, isSpa: false};
        shapesFactory = new ShapesFactory(rootScope, _statefulArtifactFactory_);
    }));

    beforeEach(() => {
        process = TestModels.createDefaultProcessModel();
        clientModel = new ProcessGraphModel(process);
        viewModel = new ProcessViewModel(clientModel, communicationManager);
        graph = new ProcessGraph(rootScope, localScope, container, viewModel, dialogService, localization, shapesFactory);
    });
}); 