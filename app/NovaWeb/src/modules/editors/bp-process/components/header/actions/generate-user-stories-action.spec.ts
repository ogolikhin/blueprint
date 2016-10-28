import * as angular from "angular";
import "angular-mocks";
import {GenerateUserStoriesAction} from "./generate-user-stories-action";
import {IStatefulArtifact, StatefulArtifact} from "../../../../../managers/artifact-manager/artifact";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {UserStoryServiceMock} from "../../../services/user-story.svc.mock";
import {SelectionManager} from "../../../../../managers/selection-manager/selection-manager";
import {MessageServiceMock} from "../../../../../core/messages/message.mock";
import {LocalizationServiceMock} from "../../../../../core/localization/localization.mock";
import {DialogServiceMock} from "../../../../../shared/widgets/bp-dialog/bp-dialog";
import {CommunicationManager} from "../../../";

describe("GenerateUserStoriesAction", () => {
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
            _userStoryService_: UserStoryServiceMock,
            _selectionManager_: SelectionManager,
            _messageService_: MessageServiceMock,
            _localization_: LocalizationServiceMock,
            _dialogService_: DialogServiceMock,
            _communicationManager_: CommunicationManager
        ) => {
            userStoryService = _userStoryService_;
            selectionManager = _selectionManager_;
            messageService = _messageService_;
            localization = _localization_;
            dialogService = _dialogService_;
            communicationManager = _communicationManager_;
        }));

    describe("constructor", () => {
        it("throws error if process is not provided", () => {
            // arrange
            let error: Error;

            // act
            try {
                new GenerateUserStoriesAction(
                    null, 
                    userStoryService, 
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
            expect(error.message).toBe("Process is not provided or is null");
        });
        
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

    function createStatefulProcessArtifact(): StatefulProcessArtifact {
        const artifactModel = {
            id: 1,
            name: "New Process 1",
            prefix: "PRO",
            lockedByUser: {
                id: 1,
                displayName: "Default Instance Admin"
            }
        };
        
        return new StatefulProcessArtifact(artifactModel, null);
    }
});