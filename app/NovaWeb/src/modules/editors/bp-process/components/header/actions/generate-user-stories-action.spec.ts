import * as angular from "angular";
import "angular-mocks";
import "script!mxClient";
import {GenerateUserStoriesAction} from "./generate-user-stories-action";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {StatefulProcessSubArtifact} from "../../../process-subartifact";
import {UserStoryServiceMock} from "../../../services/user-story.svc.mock";
import {MessageServiceMock} from "../../../../../core/messages/message.mock";
import {LocalizationServiceMock} from "../../../../../core/localization/localization.mock";
import {DialogServiceMock} from "../../../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {CommunicationManager} from "../../../";
import {RolePermissions, LockedByEnum} from "../../../../../main/models/enums";
import {ProcessEvents, IProcessDiagramCommunication} from "../../diagram/process-diagram-communication";
import * as TestShapes from "../../../models/test-shape-factory";
import {ErrorCode} from "../../../../../core/error/error-code";
import {LoadingOverlayService} from "../../../../../core/loading-overlay/loading-overlay.svc";
import {IDiagramNode} from "../../diagram/presentation/graph/models/process-graph-interfaces";

describe("GenerateUserStoriesAction", () => {
    let $rootScope: ng.IRootScopeService;
    let $q: ng.IQService;
    let userStoryService: UserStoryServiceMock;
    let messageService: MessageServiceMock;
    let localization: LocalizationServiceMock;
    let dialogService: DialogServiceMock;
    let loadingOverlayService: LoadingOverlayService;
    let processDiagramCommunication: IProcessDiagramCommunication;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("userStoryService", UserStoryServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayService);
        $provide.service("communicationManager", CommunicationManager);
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
            _communicationManager_: CommunicationManager
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
        }));

    describe("constructor", () => {
        it("throws error if user story service is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(process, null, messageService, localization,
                    dialogService, loadingOverlayService, processDiagramCommunication);
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
                    dialogService, loadingOverlayService, processDiagramCommunication);
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
                    dialogService, loadingOverlayService, processDiagramCommunication);
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
                    null, loadingOverlayService, processDiagramCommunication);
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
                    dialogService, null, processDiagramCommunication);
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
                    dialogService, loadingOverlayService, null);
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
            localization, dialogService, loadingOverlayService, processDiagramCommunication);
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
            localization, dialogService, loadingOverlayService, processDiagramCommunication);
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled if process.artifactState is null", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);

            // act
            process["state"] = null;

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);

            // act
            process.artifactState.setState({readonly: true}, false);

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled when multiple shapes are selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const userTask1 = TestShapes.createUserTask(2, $rootScope);
            const userTask2 = TestShapes.createUserTask(3, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask1, userTask2]);

            // assert
            expect(action.disabled).toBe(true);
        });
    });

    describe("generate from user task", () => {
        it("is disabled if process is null", () => {
            // arrange
            const process = null;

            // act
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if process.artifactState is null", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];

            // act
            process["state"] = null;


            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];

            // act
            process.artifactState.setState({readonly: true}, false);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled when no shape is selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, []);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled when a start shape is selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const start = TestShapes.createStart(2);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [start]);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled when an end shape is selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const end = TestShapes.createEnd(2);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [end]);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled when a system task is selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const precondition = TestShapes.createSystemTask(2, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [precondition]);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled when a user decision is selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userDecision = TestShapes.createUserDecision(2, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userDecision]);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled when a system decision is selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const systemDecision = TestShapes.createSystemDecision(2, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [systemDecision]);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled when a new user task is selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const newUserTask = TestShapes.createUserTask(-1, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [newUserTask]);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is enabled when a saved user task is selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const savedUserTask = TestShapes.createUserTask(2, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [savedUserTask]);

            // assert
            expect(generateFromTask.disabled).toBe(false);
        });

        it("is disabled when multiple shapes are selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTask1 = TestShapes.createUserTask(2, $rootScope);
            const userTask2 = TestShapes.createUserTask(3, $rootScope);

            // act
            processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask1, userTask2]);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is doesn't execute if disabled", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(false);
            const executeSpy = spyOn(action, "execute").and.callFake(() => {/* no op */});

            // act
            generateFromTask.execute();

            // assert
            expect(executeSpy).not.toHaveBeenCalled();
        });

        it("is executes if enabled", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const executeSpy = spyOn(action, "execute").and.callFake(() => {/* no op */});

            // act
            generateFromTask.execute();

            // assert
            expect(executeSpy).toHaveBeenCalled();
        });

        it("prompts user to publish changes for unpublished process", () => {
            // arrange
            const version = -1; // unpublished draft
            const process = createStatefulProcessArtifact(version);
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callFake(() => ({then: () => {/* no op*/}}));
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {/* no op */});

            // act
            generateFromTask.execute();

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
            expect(generateSpy).not.toHaveBeenCalled();
        });

        it("prompts user to publish changes for published draft process", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callFake(() => ({then: () => {/* no op*/}}));
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {/* no op */});

            // act
            process.artifactState.setState({lockedBy: LockedByEnum.CurrentUser}, false);
            generateFromTask.execute();

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
            expect(generateSpy).not.toHaveBeenCalled();
        });

        it("doesn't prompt user to publish changes for published process", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callFake(() => ({then: () => {/* no op*/}}));
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(openDialogSpy).not.toHaveBeenCalled();
            expect(generateSpy).toHaveBeenCalledWith(process, userTaskId);
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
        });

        it("handles generic publish failure", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callThrough();
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });
            spyOn(process, "publish").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject();
                    return deferred.promise;
                });
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            process.artifactState.setState({lockedBy: LockedByEnum.CurrentUser}, false);
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("Publish_Failure_Message"));
            expect(generateSpy).not.toHaveBeenCalled();
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
        });

        it("handles publish failure due to lock by other user", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callThrough();
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });
            spyOn(process, "publish").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject({errorCode: ErrorCode.LockedByOtherUser});
                    return deferred.promise;
                });
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            process.artifactState.setState({lockedBy: LockedByEnum.CurrentUser}, false);
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("Publish_Failure_LockedByOtherUser_Message"));
            expect(generateSpy).not.toHaveBeenCalled();
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
        });

        it("generates user stories if publish is successful", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callThrough();
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                } );
            spyOn(process, "publish").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            process.artifactState.setState({lockedBy: LockedByEnum.CurrentUser}, false);
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
            expect(generateSpy).toHaveBeenCalledWith(process, userTaskId);
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
        });

        it("handles generic generate user task failure", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject();
                    return deferred.promise;
                });
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            generateFromTask.execute();
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject({errorCode: ErrorCode.ArtifactNotPublished});
                    return deferred.promise;
                });
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            generateFromTask.execute();
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const userStories = [ {} ];
            const generateFromTask = action.actions[0];
            const userTaskId = 2;
            createAndSelectUserTask(userTaskId);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve(userStories);
                    return deferred.promise;
                });
            const refreshSpy = spyOn(process, "refresh").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                });
            const notifySpy = spyOn(processDiagramCommunication, "action");
            const successSpy = spyOn(messageService, "addInfo");
            const beginLoadingSpy = spyOn(loadingOverlayService, "beginLoading");
            const endLoadingSpy = spyOn(loadingOverlayService, "endLoading");

            // act
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(notifySpy).toHaveBeenCalledWith(ProcessEvents.UserStoriesGenerated, userStories);
            expect(successSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_From_UserTask_Success_Message"));
            expect(refreshSpy).toHaveBeenCalledWith(false);
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
        });
    });

    describe("generate all", () => {
        it("is disabled if process is null", () => {
            // arrange
            const process = null;

            // act
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateAll = action.actions[1];

            // assert
            expect(generateAll.disabled).toBe(true);
        });

        it("is disabled if process.artifactState is null", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject();
                    return deferred.promise;
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.reject({errorCode: ErrorCode.ArtifactNotPublished});
                    return deferred.promise;
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
                localization, dialogService, loadingOverlayService, processDiagramCommunication);
            const userStories = [ {}, {}, {} ];
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve(userStories);
                    return deferred.promise;
                });
            const refreshSpy = spyOn(process, "refresh").and.callFake(() => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
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
            expect(refreshSpy).toHaveBeenCalledWith(false);
            expect(beginLoadingSpy).toHaveBeenCalledTimes(1);
            expect(endLoadingSpy).toHaveBeenCalledTimes(1);
        });
    });

    describe("dispose", () => {
        it("calls dispose on the event observer", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const handle = "handle";
            const registerSpy = spyOn(processDiagramCommunication, "register").and.callFake(() => handle);
            const spy = spyOn(processDiagramCommunication, "unregister");
            const action = new GenerateUserStoriesAction(process, userStoryService, messageService,
                localization, dialogService, loadingOverlayService, processDiagramCommunication);

            // act
            action.dispose();

            // assert
            expect(spy).toHaveBeenCalledWith(ProcessEvents.SelectionChanged, handle);
        });
    });

    function createAndSelectUserTask(id: number): void {
        const userTask = TestShapes.createUserTask(id, $rootScope);
        processDiagramCommunication.action(ProcessEvents.SelectionChanged, [userTask]);
    }
});

function createStatefulProcessArtifact(version: number = 1): StatefulProcessArtifact {
    const artifactModel = {
        id: 1,
        permissions: RolePermissions.Edit,
        version: version
    };

    return new StatefulProcessArtifact(artifactModel, null);
}
