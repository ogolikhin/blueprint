import * as angular from "angular";
import {UserStoryPreviewController} from "./user-story-preview";
import {UserStoryDialogModel} from "../models/user-story-dialog-model";
import {ModalServiceInstanceMock} from "../../../../../shell/login/mocks.spec";
import {ShapeModelMock} from "../../diagram/presentation/graph/shapes/shape-model.mock";
import {UserTask} from "../../diagram/presentation/graph/shapes/";
import {CommunicationManager, ICommunicationManager} from "../../../services/communication-manager";

describe("UserStoryPreviewController", () => {
    var controller: UserStoryPreviewController;
    var dialogModel = new UserStoryDialogModel();

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("communicationManager", CommunicationManager);
    }));

    beforeEach(
        inject(($controller: ng.IControllerService,
                $rootScope: ng.IRootScopeService,
                $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
                communicationManager: ICommunicationManager) => {
                dialogModel = new UserStoryDialogModel();
                var controllerScope = $rootScope.$new();
                controller = $controller(UserStoryPreviewController, {
                    $scope: controllerScope,
                    $uibModalInstance,
                    dialogModel,
                    communicationManager
                });
            }
        ));

    it("controller is defined",
        inject(() => {
            //Act/Assert
            expect(controller).toBeDefined();
        }));

    it("saveData has been called.",
        inject(() => {
            // Arrange
            dialogModel.originalUserTask = new UserTask(ShapeModelMock.instance().UserTaskMock(), ShapeModelMock.instance().RootscopeMock(), null, null);

            // Act
            dialogModel.clonedUserTask = angular.copy(dialogModel.originalUserTask);
            spyOn(controller, "saveData").and.callThrough();
            controller.ok();

            // Assert
            expect(controller.saveData).toHaveBeenCalled();
        }));

    // TODO: Need to update this test when the rich text control for editing gets implemented.
    xit("cancel function",
        inject(() => {
            // Arrange
            dialogModel.originalUserTask = new UserTask(ShapeModelMock.instance().UserTaskMock(), ShapeModelMock.instance().RootscopeMock(), null, null);

            //Act
            dialogModel.clonedUserTask = angular.copy(dialogModel.originalUserTask);
            dialogModel.clonedUserTask.userStoryProperties.nfr.value = "newValue";
            controller.cancel();

            // Assert
            expect(dialogModel.originalUserTask.userStoryProperties.nfr.value).toEqual("sampleValue");
        }));
});
