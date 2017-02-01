import * as angular from "angular";
import {UserStoryPreviewController} from "./user-story-preview";
import {UserStoryDialogModel} from "../models/user-story-dialog-model";
import {ModalServiceInstanceMock} from "../../../../../shell/login/mocks.spec";
import {ShapeModelMock} from "../../diagram/presentation/graph/shapes/shape-model.mock";
import {UserTask} from "../../diagram/presentation/graph/shapes/";
import {CommunicationManager, ICommunicationManager} from "../../../services/communication-manager";

describe("UserStoryPreviewController", () => {
    let controller: UserStoryPreviewController;
    let dialogModel = new UserStoryDialogModel();
    let communicationManager: ICommunicationManager;
    let $rootScope: ng.IRootScopeService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("communicationManager", CommunicationManager);
    }));

    beforeEach(
        inject(($controller: ng.IControllerService,
                _$rootScope_: ng.IRootScopeService,
                $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
                _communicationManager_: ICommunicationManager) => {
                dialogModel = new UserStoryDialogModel();
                communicationManager = _communicationManager_;
                $rootScope = _$rootScope_;
                const controllerScope = $rootScope.$new();
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
    describe("isUserStoryLoaded -", () => {

        it("successfully sets through communication manager", () => {
            //arrange
            communicationManager.modalDialogManager.notifyUserStoryLoaded(true);

            //act/assert
            expect(controller["isUserStoryLoaded"]).toBe(true);
        });
    });
    describe("showWings -", () => {
        it("successfully shows wings when everything is true", () => {
            //arrange
            dialogModel.isUserSystemProcess = true;
            communicationManager.modalDialogManager.notifyUserStoryLoaded(true);

            //act/assert
            expect(controller.showWings()).toBe(true);
        });
        it("does not show wings when is not user system process", () => {
            //arrange
            dialogModel.isUserSystemProcess = false;
            communicationManager.modalDialogManager.notifyUserStoryLoaded(true);

            //act/assert
            expect(controller.showWings()).toBe(false);
        });
        it("does not show wings when user story is not loaded", () => {
            //arrange
            dialogModel.isUserSystemProcess = true;
            communicationManager.modalDialogManager.notifyUserStoryLoaded(false);

            //act/assert
            expect(controller.showWings()).toBe(false);
        });
    });
});
