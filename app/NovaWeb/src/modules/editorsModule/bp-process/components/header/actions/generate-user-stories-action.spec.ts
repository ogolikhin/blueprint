import * as angular from "angular";
import "angular-mocks";
import "script!mxClient";
import {GenerateUserStoriesAction} from "./generate-user-stories-action";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {UserStoryServiceMock} from "../../../services/user-story.svc.mock";
import {LocalizationServiceMock} from "../../../../../commonModule/localization/localization.service.mock";
import {DialogServiceMock} from "../../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {CommunicationManager} from "../../../";
import {RolePermissions} from "../../../../../main/models/enums";
import {ProcessEvents, IProcessDiagramCommunication} from "../../diagram/process-diagram-communication";
import * as TestShapes from "../../../models/test-shape-factory";
import {LoadingOverlayService} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {MessageServiceMock} from "../../../../../main/components/messages/message.mock";
import {ProjectExplorerServiceMock} from "../../../../../main/components/bp-explorer/project-explorer.service.mock";
import {IProjectExplorerService} from "../../../../../main/components/bp-explorer/project-explorer.service";

describe("GenerateUserStoriesAction", () => {
    let $rootScope: ng.IRootScopeService;
    let $q: ng.IQService;
    let userStoryService: UserStoryServiceMock;
    let messageService: MessageServiceMock;
    let localization: LocalizationServiceMock;
    let dialogService: DialogServiceMock;
    let loadingOverlayService: LoadingOverlayService;
    let processDiagramCommunication: IProcessDiagramCommunication;
    let projectExplorerService: IProjectExplorerService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("userStoryService", UserStoryServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("projectExplorerService", ProjectExplorerServiceMock);
    }));

    beforeEach(
        inject((
            _$rootScope_: ng.IRootScopeService,
            _$q_: ng.IQService,
            _userStoryService_: UserStoryServiceMock,
            _messageService_: MessageServiceMock,
            _localization_: LocalizationServiceMock,
            _dialogService_: DialogServiceMock,
            _loadingOverlayService_: LoadingOverlayService,
            _communicationManager_: CommunicationManager,
            _projectExplorerService_: IProjectExplorerService
        ) => {
            $rootScope = _$rootScope_;
            $rootScope["config"] = {labels: []};
            $q = _$q_;
            userStoryService = _userStoryService_;
            messageService = _messageService_;
            localization = _localization_;
            dialogService = _dialogService_;
            loadingOverlayService = _loadingOverlayService_;
            processDiagramCommunication = _communicationManager_.processDiagramCommunication;
            projectExplorerService = _projectExplorerService_;
        }));

    describe("constructor", () => {
        it("throws error if user story service is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(process, null, messageService, localization,
                    dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("User story service is not provided or is null");
        });

        it("throws error if message service is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(process, userStoryService, null, localization,
                    dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Message service is not provided or is null");
        });

        it("throws error if localization service is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(process, userStoryService, messageService, null,
                    dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Localization service is not provided or is null");
        });

        it("throws error if dialog service is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(process, userStoryService, messageService, localization,
                    null, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Dialog service is not provided or is null");
        });

        it("throws error if loading overlay service is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(process, userStoryService, messageService, localization,
                    dialogService, null, processDiagramCommunication, projectExplorerService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Loading overlay service is not provided or is null");
        });

        it("throws error if process diagram communication is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(process, userStoryService, messageService, localization,
                    dialogService, loadingOverlayService, null, projectExplorerService);
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Process diagram manager is not provided or is null");
        });
    });

    it("returns correct icon", () => {
        // arrange
        const process = createStatefulProcessArtifact();
        const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
            localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
        const expectedIcon = "fonticon fonticon2-news";

        // act
        const actualIcon = action.icon;

        // assert
        expect(actualIcon).toEqual(expectedIcon);
    });

    it("returns correct tooltip", () => {
        // arrange
        const process = createStatefulProcessArtifact();
        const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
            localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
        const expectedTooltip = localization.get("ST_US_Generate_Dropdown_Tooltip");

        // act
        const actualTooltip = action.tooltip;

        // assert
        expect(actualTooltip).toEqual(expectedTooltip);
    });

    describe("generate", () => {
        it("is disabled if process is null", () => {
            // arrange
            const process = null;

            // act
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled if process.artifactState is null", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);

            // act
            process["state"] = null;

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);

            // act
            process.artifactState.setState({readonly: true}, false);

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled when multiple shapes are selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const userTask1 = TestShapes.createUserTask(2, $rootScope);
            const userTask2 = TestShapes.createUserTask(3, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask1, userTask2]);

            // assert
            expect(action.disabled).toBe(true);
        });
    });
});

function createStatefulProcessArtifact(version: number = 1): StatefulProcessArtifact {
    const artifactModel = {
        id: 1,
        permissions: RolePermissions.Edit,
        version: version
    };

    return new StatefulProcessArtifact(artifactModel, null);
}
