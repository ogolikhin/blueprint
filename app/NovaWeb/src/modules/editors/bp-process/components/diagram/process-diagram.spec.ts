import {ProcessServiceMock} from "../../services/process/process.svc.mock";
import {IProcessService} from "../../services/process/process.svc";
import {MessageServiceMock} from "../../../../core/messages/message.mock";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ProcessDiagram} from "./process-diagram";
import {CommunicationManager} from "../../../../main/services";
import * as TestModels from "../../models/test-model-factory";

describe("ProcessDiagram Tests", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("processModelService", ProcessServiceMock);
        $provide.service("messageService", MessageServiceMock);
    }));
    let rootScope: ng.IRootScopeService,
        scope,
        timeout: ng.ITimeoutService,
        q: ng.IQService,
        log: ng.ILogService,
        processModelService: IProcessService,
        messageService: IMessageService;

    let container: HTMLElement,
        wrapper: HTMLElement;

    beforeEach(inject((
        $rootScope: ng.IRootScopeService,
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        $log: ng.ILogService,
        _processModelService_: IProcessService,
        _messageService_: IMessageService) => {

        $rootScope["config"] = {
            settings: {
                ProcessShapeLimit: 100,
                StorytellerIsSMB: "false"
            }
        };

        rootScope = $rootScope;
        scope = {};
        timeout = $timeout;
        q = $q;
        log = $log;
        processModelService = _processModelService_;
        messageService = _messageService_;


        wrapper = document.createElement("DIV");
        container = document.createElement("DIV");
        wrapper.appendChild(container);

    }));

    it("Load process - Success", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            processModelService,
            messageService,
            new CommunicationManager());

        let model = TestModels.createDefaultProcessModel();

        let loadSpy = spyOn(processModelService, "load");
        loadSpy.and.returnValue(q.when(model));

        // act
        diagram.createDiagram(1, container);
        rootScope.$apply();

        // assert
        expect(diagram.processViewModel).not.toBeNull(null);
        expect(diagram.processViewModel.shapes.length).toBe(5);
    });
    it("Load process - Destroy ", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            processModelService,
            messageService,
            new CommunicationManager());

        let model = TestModels.createDefaultProcessModel();

        let loadSpy = spyOn(processModelService, "load");
        loadSpy.and.returnValue(q.when(model));

        // act
        diagram.createDiagram(1, container);
        rootScope.$apply();

        diagram.destroy();

        // assert
        expect(diagram.processViewModel).toBeNull(null);
        expect(container.childElementCount).toBe(0);
    });

    it("creatediagram - Invalid Process Id", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            processModelService,
            messageService,
            new CommunicationManager());
        

        let error: Error;
        // act
        try {
            diagram.createDiagram(-1, container);
        } catch (err) {
            error = err;
        }

        // assert 
        expect(error.message).toBe("Process id '-1' is invalid.");
    });
    it("creatediagram - Null element", () => {
        // arrange
        let diagram = new ProcessDiagram(
            rootScope,
            scope,
            timeout,
            q,
            log,
            processModelService,
            messageService,
            new CommunicationManager());


        let error: Error;

        // act
        try {
            diagram.createDiagram(1, null);
        } catch (err) {
            error = err;
        }

        // assert 
        expect(error.message).toBe("There is no html element for the diagram");
    });
});