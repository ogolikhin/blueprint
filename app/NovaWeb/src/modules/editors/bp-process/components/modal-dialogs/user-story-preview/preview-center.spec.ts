import * as angular from "angular";
import {PreviewCenterController, PreviewCenterComponent} from "./preview-center";
import {ShapeModelMock} from "../../diagram/presentation/graph/shapes/shape-model.mock";
import {SystemTask} from "../../diagram/presentation/graph/shapes/";
//import {IArtifactManager} from "../../../../../managers/artifact-manager/artifact-manager";


//TODO: Need to wait for ArtifactManagerMock to be done and then we can include PreviewCenter tests back
xdescribe("PreviewCenter Directive", () => {

    var element: ng.IAugmentedJQuery;
    var controller: PreviewCenterController;
    var scope: ng.IScope;
    var directiveWrapper: string;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.component("previewCenter", new PreviewCenterComponent());
    }));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        //$provide.service("artifactManager", ArtifactManagerDummyMock);
    }));

    beforeEach(
        inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
            $rootScope["config"] = {
                settings: {
                    "StorytellerIsSMB": "false"
                }
            };
            scope = $rootScope.$new();
            scope.$parent["vm"] = {
                isReadonly: false
            };
            scope["systemTaskModel"] = ShapeModelMock.instance().SystemTaskMock();
            scope["userTaskModel"] = ShapeModelMock.instance().UserTaskMock();
            /* tslint:disable:max-line-length */
            directiveWrapper = "<div><preview-center user-task-model='true' previous-system-task=\"systemTaskModel\" next-system-task=\"systemTaskModel\" is-user-system-process='true' user-task-model=\"userTaskModel\" /></div>";
            /* tslint:enable:max-line-length */
            element = $compile(directiveWrapper)(scope);
            scope.$digest();
            controller = element.find("preview-center").isolateScope()["centerCtrl"];
        }));

    it("Controller Constructor",
        inject(($controller: ng.IControllerService,
                $rootScope: ng.IRootScopeService,
                $window: ng.IWindowService) => {
            // Arrange
            let controllerScope = $rootScope.$new();
            controllerScope.$parent["vm"] = {
                isReadonly: false
            };

            let sampleUserTask = ShapeModelMock.instance().UserTaskMock();
            let sampleSystemTask = ShapeModelMock.instance().SystemTaskMock();
            controllerScope["centerCtrl"] = {
                userTaskModel: sampleUserTask,
                previousSystemTask: sampleSystemTask,
                nextSystemTask: sampleSystemTask
            };

            // Act
            let constructorTestController: PreviewCenterController = $controller(PreviewCenterController, {
                $window,
                $scope: controllerScope,
                $rootScope
            });

            // Assert
            expect(constructorTestController).not.toBeNull();
        }));


    it("can show a directive", (inject(() => {
        //Assert
        expect(element.find("preview-center").length).toBeGreaterThan(0);
    })));

    it("test on destroy", (inject(() => {
        //Arrange
        controller.previousSystemTask = new SystemTask(ShapeModelMock.instance().SystemTaskMock(), ShapeModelMock.instance().RootscopeMock(), "", null, null);

        // Act
        scope.$destroy();

        //Assert
        expect(controller.previousSystemTask).toBeNull();
    })));

    it("showMore Label", (inject(() => {
        //Arrange
        let event = jQuery.Event("keydown", {
            keyCode: 13
        });
        spyOn(controller, "resizeContentAreas");

        // Act
        controller.showMore("label", event);

        //Assert
        expect(controller.resizeContentAreas).toHaveBeenCalled();
    })));

    it("showMore br", (inject(() => {
        //Arrange
        var event = jQuery.Event("keydown", {
            keyCode: 13
        });
        spyOn(controller, "resizeContentAreas");

        // Act
        controller.showMore("br", event);

        //Assert
        expect(controller.resizeContentAreas).toHaveBeenCalled();
    })));

    it("showMore nfr", (inject(() => {
        //Arrange
        var event = jQuery.Event("keydown", {
            keyCode: 13
        });
        spyOn(controller, "resizeContentAreas");

        // Act
        controller.showMore("nfr", event);

        //Assert
        expect(controller.resizeContentAreas).toHaveBeenCalled();
    })));
});
