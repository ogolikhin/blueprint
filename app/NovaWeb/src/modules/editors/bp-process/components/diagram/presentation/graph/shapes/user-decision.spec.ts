import * as angular from "angular";
import { ShapesFactory } from "./shapes-factory";
import { ProcessGraph } from "../process-graph";
import { ProcessViewModel } from "../../../viewmodel/process-viewmodel";
import * as ProcessModels from "../../../../../models/process-models";
import { UserDecision } from "./";
import { NodeChange, IDecision } from "../models/";
import { ICommunicationManager, CommunicationManager } from "../../../../../../bp-process";
import { LocalizationServiceMock } from "../../../../../../../core/localization/localization.mock";
import { DialogService } from "../../../../../../../shared/widgets/bp-dialog";
import { ModalServiceMock } from "../../../../../../../shell/login/mocks.spec";
import { IStatefulArtifactFactory } from "../../../../../../../managers/artifact-manager/";
import { StatefulArtifactFactoryMock } from "../../../../../../../managers/artifact-manager/artifact/artifact.factory.mock";

describe("UserDecision", () => {

    let shapesFactory: ShapesFactory;
    let localScope, rootScope, wrapper, container;
    let communicationManager: ICommunicationManager,
        dialogService: DialogService,
        localization: LocalizationServiceMock,
        statefulArtifactFactory: IStatefulArtifactFactory;

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
        _statefulArtifactFactory_: IStatefulArtifactFactory) => {

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
        shapesFactory = new ShapesFactory($rootScope, statefulArtifactFactory);
        localScope = { graphContainer: container, graphWrapper: wrapper, isSpa: false };
    }));

    it("initializes details button", () => {
        // Arrange

        const id = 3;

        const testModel = new ProcessModels.ProcessModel();
        const model = shapesFactory.createModelUserDecisionShape(2, 1, id, 0, 0);
        testModel.shapes.push(model);
        const processViewModel = new ProcessViewModel(testModel, communicationManager);

        const graph = new ProcessGraph(rootScope, localScope, container, processViewModel, dialogService, localization, shapesFactory);

        // Act
        graph.render(false, null);

        // Assert
        expect(graph.getMxGraphModel().getCell(`DB${id}`)).not.toBeNull();
    });

});
