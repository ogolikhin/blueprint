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
import {ErrorCode} from "../../../../../shell/error/error-code";
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

    describe("generate all", () => {
        it("is disabled if process is null", () => {
            // arrange
            const process = null;

            // act
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const generateAll = action.actions[1];

            // assert
            expect(generateAll.disabled).toBe(true);
        });

        it("is disabled if process.artifactState is null", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const generateAll = action.actions[1];

            // act
            process["state"] = null;

            // assert
            expect(generateAll.disabled).toBe(true);
        });

        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const generateAll = action.actions[1];

            // act
            process.artifactState.setState({readonly: true}, false);

            // assert
            expect(generateAll.disabled).toBe(true);
        });

        it("is enabled if process is not read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const generateAll = action.actions[1];

            // act
            process.artifactState.setState({readonly: false}, false);

            // assert
            expect(generateAll.disabled).toBe(false);
        });

        it("is disabled when multiple shapes are selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const generateAll = action.actions[1];
            const userTask1 = TestShapes.createUserTask(2, $rootScope);
            const userTask2 = TestShapes.createUserTask(3, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask1, userTask2]);

            // assert
            expect(generateAll.disabled).toBe(true);
        });

        it("is doesn't execute if disabled", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(false);
            const executeSpy = spyOn(action, "execute").and.callFake(() => {/* no op */});

            // act
            generateAll.execute();

            // assert
            expect(executeSpy).not.toHaveBeenCalled();
        });

        it("is executes if enabled", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const executeSpy = spyOn(action, "execute").and.callFake(() => {/* no op */});

            // act
            generateAll.execute();

            // assert
            expect(executeSpy).toHaveBeenCalled();
        });

        it("handles generic generate user task failure", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    return $q.reject();
                });
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            generateAll.execute();
            $rootScope.$digest();

            // assert
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_Generic_Failure_Message"));
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
        });

        it("handles generate user task failure due to lock by another user", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    return $q.reject({errorCode: ErrorCode.ArtifactNotPublished});
                });
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            generateAll.execute();
            $rootScope.$digest();

            // assert
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_LockedByOtherUser_Failure_Message"));
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
        });

        it("notifies about generated user stories if generation is successful", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const userStories = [ {}, {}, {} ];
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    return $q.resolve(userStories);
                });
            const refreshSpy = spyOn(process, "refresh").and.callFake(() => {
                    return $q.resolve();
                });
            const notifySpy = spyOn(processDiagramCommunication, "action");
            const successSpy = spyOn(messageService, "addInfo");
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            generateAll.execute();
            $rootScope.$digest();

            // assert
            expect(notifySpy).toHaveBeenCalledWith(ProcessEvents.UserStoriesGenerated, userStories);
            expect(successSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_All_Success_Message"));
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
        });

        it("refreshes project node if generation is successful", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication, projectExplorerService);
            const userStories = [ {}, {}, {} ];
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    return $q.resolve(userStories);
                });
            const refreshSpy = spyOn(process, "refresh").and.callFake(() => {
                    return $q.resolve();
                });
            const notifySpy = spyOn(processDiagramCommunication, "action");
            const successSpy = spyOn(messageService, "addInfo");
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");
            const projectManagerRefreshSpy = spyOn(projectExplorerService, "refresh").and.callThrough();
            const projectManagertriggerProjectCollectionRefreshSpy = spyOn(projectExplorerService, "triggerProjectCollectionRefresh");

            // act
            generateAll.execute();
            $rootScope.$digest();

            // assert
            expect(notifySpy).toHaveBeenCalledWith(ProcessEvents.UserStoriesGenerated, userStories);
            expect(successSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_All_Success_Message"));
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
            expect(projectManagerRefreshSpy).toHaveBeenCalledTimes(1);
            expect(projectManagertriggerProjectCollectionRefreshSpy).toHaveBeenCalledTimes(1);
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
