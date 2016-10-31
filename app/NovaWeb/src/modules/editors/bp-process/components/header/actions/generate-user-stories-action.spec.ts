import * as angular from "angular";
import "angular-mocks";
import {GenerateUserStoriesAction} from "./generate-user-stories-action";
import {IStatefulArtifact, StatefulArtifact} from "../../../../../managers/artifact-manager/artifact";
import {IProcess} from "../../../models/process-models";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {StatefulProcessSubArtifact} from "../../../process-subartifact";
import {UserStoryServiceMock} from "../../../services/user-story.svc.mock";
import {SelectionManager} from "../../../../../managers/selection-manager/selection-manager";
import {MessageServiceMock} from "../../../../../core/messages/message.mock";
import {LocalizationServiceMock} from "../../../../../core/localization/localization.mock";
import {DialogServiceMock} from "../../../../../shared/widgets/bp-dialog/bp-dialog";
import {CommunicationManager} from "../../../";
import {RolePermissions, LockedByEnum} from "../../../../../main/models/enums";
import {ErrorCode} from "../../../../../core/error";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import * as TestModels from "../../../models/test-model-factory";

describe("GenerateUserStoriesAction", () => {
    let $rootScope: ng.IRootScopeService;
    let $q: ng.IQService;
    let userStoryService: UserStoryServiceMock;
    let selectionManager: SelectionManager;
    let messageService: MessageServiceMock;
    let localization: LocalizationServiceMock;
    let dialogService: DialogServiceMock;
    let communicationManager: CommunicationManager;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("userStoryService", UserStoryServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("communicationManager", CommunicationManager);
    }));

    beforeEach(
        inject((
            _$rootScope_: ng.IRootScopeService,
            _$q_: ng.IQService,
            _userStoryService_: UserStoryServiceMock,
            _selectionManager_: SelectionManager,
            _messageService_: MessageServiceMock,
            _localization_: LocalizationServiceMock,
            _dialogService_: DialogServiceMock,
            _communicationManager_: CommunicationManager
        ) => {
            $rootScope = _$rootScope_;
            $q = _$q_;
            userStoryService = _userStoryService_;
            selectionManager = _selectionManager_;
            messageService = _messageService_;
            localization = _localization_;
            dialogService = _dialogService_;
            communicationManager = _communicationManager_;
        }));

    describe("constructor", () => {
        it("throws error if user story service is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(
                    process, 
                    null, 
                    selectionManager, 
                    messageService, 
                    localization, 
                    dialogService, 
                    communicationManager.processDiagramCommunication
                );
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("User story service is not provided or is null");
        });

        it("throws error if selection manager is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(
                    process, 
                    userStoryService, 
                    null, 
                    messageService, 
                    localization, 
                    dialogService, 
                    communicationManager.processDiagramCommunication
                );
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Selection manager is not provided or is null");
        });

        it("throws error if message service is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(
                    process, 
                    userStoryService, 
                    selectionManager, 
                    null, 
                    localization, 
                    dialogService, 
                    communicationManager.processDiagramCommunication
                );
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
                new GenerateUserStoriesAction(
                    process, 
                    userStoryService, 
                    selectionManager, 
                    messageService, 
                    null, 
                    dialogService, 
                    communicationManager.processDiagramCommunication
                );
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
                new GenerateUserStoriesAction(
                    process, 
                    userStoryService, 
                    selectionManager, 
                    messageService, 
                    localization, 
                    null, 
                    communicationManager.processDiagramCommunication
                );
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Dialog service is not provided or is null");
        });

        it("throws error if process diagram communication is not provided", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(
                    process, 
                    userStoryService, 
                    selectionManager, 
                    messageService, 
                    localization, 
                    dialogService, 
                    null
                );
            } catch (exception) {
                error = exception;
            }

            // assert
            expect(error).not.toBeNull();
            expect(error.message).toBe("Process diagram manager is not provided or is null");
        });
    });

    describe("generate", () => {
        it("is disabled if process is null", () => {
            // arrange
            const process = null;
            
            // act
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled if process.artifactState is null", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );

            // act
            process["state"] = null;

            // assert
            expect(action.disabled).toBe(true);
        });

        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );

            // act
            process.artifactState.setState({readonly: true }, false);

            // assert
            expect(action.disabled).toBe(true);
        });
    });

    describe("generate from user task", () => {
        it("is disabled if process is null", () => {
            // arrange
            const process = null;
            
            // act
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if process.artifactState is null", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            
            // act
            process["state"] = null;
            

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            process.artifactState.setState({readonly: true }, false);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if no process shape is selected", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            selectionManager.clearSubArtifact();

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if selected process shape is start", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            const processShape = TestModels.createStart(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if selected process shape is end", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            const processShape = TestModels.createEnd(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if selected process shape is pre-condition", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            const processShape = TestModels.createPrecondition(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if selected process shape is system task", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            const processShape = TestModels.createSystemTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if selected process shape is user decision", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            const processShape = TestModels.createUserDecision(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if selected process shape is system decision", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            const processShape = TestModels.createSystemDecision(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is disabled if selected user task is new", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            const processShape = TestModels.createUserTask(-1);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);

            // assert
            expect(generateFromTask.disabled).toBe(true);
        });

        it("is enabled if selecting a saved user task", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];

            // act
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);

            // assert
            expect(generateFromTask.disabled).toBe(false);
        });

        it("is doesn't execute if disabled", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
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
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
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
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callFake(() => { return { then: () => {/* no op*/} }; });
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
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callFake(() => { return { then: () => {/* no op*/} }; });
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {/* no op */});

            // act
            process.artifactState.setState({ lockedBy: LockedByEnum.CurrentUser }, false);
            generateFromTask.execute();

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
            expect(generateSpy).not.toHaveBeenCalled();
        });

        it("doesn't prompt user to publish changes for published process", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callFake(() => { return { then: () => {/* no op*/} }; });
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {/* no op */});

            // act
            generateFromTask.execute();

            // assert
            expect(openDialogSpy).not.toHaveBeenCalled();
            expect(generateSpy).toHaveBeenCalledWith(process, processShape.id);
        });

        it("handles generic publish failure", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callThrough();
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {/* no op */});
            spyOn(process, "publish").and.callFake(
                () => {
                    const deferred = $q.defer();
                    deferred.reject();
                    return deferred.promise;
                }
            );
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});

            // act
            process.artifactState.setState({ lockedBy: LockedByEnum.CurrentUser }, false);
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("Publish_Failure_Message"));
            expect(generateSpy).not.toHaveBeenCalled();
        });

        it("handles publish failure due to lock by other user", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callThrough();
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {/* no op */});
            spyOn(process, "publish").and.callFake(
                () => {
                    const deferred = $q.defer();
                    deferred.reject({ errorCode: ErrorCode.LockedByOtherUser });
                    return deferred.promise;
                }
            );
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});

            // act
            process.artifactState.setState({ lockedBy: LockedByEnum.CurrentUser }, false);
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("Publish_Failure_LockedByOtherUser_Message"));
            expect(generateSpy).not.toHaveBeenCalled();
        });

        it("generates user stories if publish is successful", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const openDialogSpy = spyOn(dialogService, "open").and.callThrough();
            const generateSpy = spyOn(action, "generateUserStories").and.callFake(() => {/* no op */});
            spyOn(process, "publish").and.callFake(
                () => {
                    const deferred = $q.defer();
                    deferred.resolve();
                    return deferred.promise;
                }
            );

            // act
            process.artifactState.setState({ lockedBy: LockedByEnum.CurrentUser }, false);
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(openDialogSpy).toHaveBeenCalled();
            expect(generateSpy).toHaveBeenCalledWith(process, processShape.id);
        });

        it("handles generic generate user task failure", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(
                () => {
                    const deferred = $q.defer();
                    deferred.reject();
                    return deferred.promise;
                }
            );
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});

            // act
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_Generic_Failure_Message"));
        });

        it("handles generate user task failure due to lock by another user", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(
                () => {
                    const deferred = $q.defer();
                    deferred.reject({ errorCode: ErrorCode.ArtifactNotPublished });
                    return deferred.promise;
                }
            );
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});

            // act
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_LockedByOtherUser_Failure_Message"));
        });

        it("notifies about generated user stories if generation is successful", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const userStories = [ {} ];
            const generateFromTask = action.actions[0];
            const processShape = TestModels.createUserTask(2);
            const processSubArtifact = new StatefulProcessSubArtifact(process, processShape, null);
            selectionManager.setSubArtifact(processSubArtifact);
            const canExecuteSpy = spyOn(action, "canExecuteGenerateFromTask").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(
                () => {
                    const deferred = $q.defer();
                    deferred.resolve(userStories);
                    return deferred.promise;
                }
            );
            const refreshSpy = spyOn(process, "refresh");
            const notifySpy = spyOn(communicationManager.processDiagramCommunication, "action");
            const successSpy = spyOn(messageService, "addInfo");

            // act
            generateFromTask.execute();
            $rootScope.$digest();

            // assert
            expect(notifySpy).toHaveBeenCalledWith(ProcessEvents.UserStoriesGenerated, userStories);
            expect(successSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_From_UserTask_Success_Message"));
            expect(refreshSpy).toHaveBeenCalledWith(false);
        });
    });

    describe("generate all", () => {
        it("is disabled if process is null", () => {
            // arrange
            const process = null;
            
            // act
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateAll = action.actions[1];

            // assert
            expect(generateAll.disabled).toBe(true);
        });

        it("is disabled if process.artifactState is null", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateAll = action.actions[1];

            // act
            process["state"] = null;

            // assert
            expect(generateAll.disabled).toBe(true);
        });

        it("is disabled if process is read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateAll = action.actions[1];

            // act
            process.artifactState.setState({ readonly: true }, false);

            // assert
            expect(generateAll.disabled).toBe(true);
        });

        it("is enabled if process is not read-only", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateAll = action.actions[1];

            // act
            process.artifactState.setState({ readonly: false }, false);

            // assert
            expect(generateAll.disabled).toBe(false);
        });

        it("is doesn't execute if disabled", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
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
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
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
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(
                () => {
                    const deferred = $q.defer();
                    deferred.reject();
                    return deferred.promise;
                }
            );
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});

            // act
            generateAll.execute();
            $rootScope.$digest();

            // assert
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_Generic_Failure_Message"));
        });

        it("handles generate user task failure due to lock by another user", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(
                () => {
                    const deferred = $q.defer();
                    deferred.reject({ errorCode: ErrorCode.ArtifactNotPublished });
                    return deferred.promise;
                }
            );
            const errorMessageSpy = spyOn(messageService, "addError").and.callFake(() => {/* no op */});

            // act
            generateAll.execute();
            $rootScope.$digest();

            // assert
            expect(errorMessageSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_LockedByOtherUser_Failure_Message"));
        });

        it("notifies about generated user stories if generation is successful", () => {
            // arrange
            const process = createStatefulProcessArtifact();
            const action = new GenerateUserStoriesAction(
                process, 
                userStoryService, 
                selectionManager, 
                messageService, 
                localization, 
                dialogService, 
                communicationManager.processDiagramCommunication
            );
            const userStories = [ {}, {}, {} ];
            const generateAll = action.actions[1];
            const canExecuteSpy = spyOn(action, "canExecuteGenerateAll").and.returnValue(true);
            const generateSpy = spyOn(userStoryService, "generateUserStories").and.callFake(
                () => {
                    const deferred = $q.defer();
                    deferred.resolve(userStories);
                    return deferred.promise;
                }
            );
            const refreshSpy = spyOn(process, "refresh");
            const notifySpy = spyOn(communicationManager.processDiagramCommunication, "action");
            const successSpy = spyOn(messageService, "addInfo");

            // act
            generateAll.execute();
            $rootScope.$digest();

            // assert
            expect(notifySpy).toHaveBeenCalledWith(ProcessEvents.UserStoriesGenerated, userStories);
            expect(successSpy).toHaveBeenCalledWith(localization.get("ST_US_Generate_All_Success_Message"));
            expect(refreshSpy).toHaveBeenCalledWith(false);
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