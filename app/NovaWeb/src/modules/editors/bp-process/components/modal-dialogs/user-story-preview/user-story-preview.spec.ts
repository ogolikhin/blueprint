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

});
