
import {UserStoryDialogModel} from "../models/user-story-dialog-model";
import {PreviewWingController, PreviewWingDirective} from "./preview-wing";
import { ShapeModelMock } from "../../diagram/presentation/graph/shapes/shape-model.mock";

describe("PreviewWing Directive", () => {

    var element: ng.IAugmentedJQuery;
    var scope: ng.IScope;
    var directiveTemplate: string;
    var directiveWrapper: string;

    it("constructor, cloneModel",
        inject(($controller: ng.IControllerService, $rootScope: ng.IRootScopeService) => {
            // Arrange
            var o = {};
            o["isLeftWing"] = "true";
            o["systemTaskModel"] = {};
            var controllerScope = $rootScope.$new();
            (controllerScope.$parent)["dialogModel"] = new UserStoryDialogModel();
            controllerScope["wingCtrl"] = o;

            // Act
            var controller: PreviewWingController = $controller(PreviewWingController, { $scope: controllerScope, $rootScope });

            // Assert
            expect(controller.wingTask).not.toBeNull();
        }));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.directive("previewWing", PreviewWingDirective.directive);
    }));

    beforeEach(
        inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
            scope = $rootScope.$new();
            scope.$parent["vm"] = {
                isReadonly: false
            };
            scope["systemTaskModel"] = ShapeModelMock.instance().SystemTaskMock();
            directiveWrapper = "<div><preview-wing is-left-wing='true' system-task-model=\"systemTaskModel\" /></div>";
            element = $compile(directiveWrapper)(scope);
            scope.$digest();
        }));


    it("can show a directive", (inject(() => {
        //Assert
        expect(element.find("preview-wing").length).toBeGreaterThan(0);
    })));
});