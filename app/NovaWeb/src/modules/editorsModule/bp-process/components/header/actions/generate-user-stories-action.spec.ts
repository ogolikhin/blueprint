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
import {AnalyticsServiceMock} from "../../../../../main/components/analytics/analytics.mock";
import {IExtendedAnalyticsService} from "../../../../../main/components/analytics/analytics";

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
    let analytics: IExtendedAnalyticsService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("userStoryService", UserStoryServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("communicationManager", CommunicationManager);
        $provide.service("projectExplorerService", ProjectExplorerServiceMock);
        $provide.service("analytics", AnalyticsServiceMock);
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
            _projectExplorerService_: IProjectExplorerService,
            _analytics_: AnalyticsServiceMock
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
            analytics = _analytics_;
        }));

    it("returns correct icon", () => {
        // arrange
        const process = createStatefulProcessArtifact();
        const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
            localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService, analytics);
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
            localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService, analytics);
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService, analytics);

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled if process.artifactState is null", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService, analytics);

            // act
            process["state"] = null;

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService, analytics);

            // act
            process.artifactState.setState({readonly: true}, false);

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled when multiple shapes are selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService, analytics);
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
