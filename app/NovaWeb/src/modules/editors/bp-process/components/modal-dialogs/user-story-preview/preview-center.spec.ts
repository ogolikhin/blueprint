import * as angular from "angular";
import "angular-ui-router";
import {PreviewCenterController, PreviewCenterComponent} from "./preview-center";
import {ShapeModelMock} from "../../diagram/presentation/graph/shapes/shape-model.mock";
import {SystemTask} from "../../diagram/presentation/graph/shapes/";
import {IArtifactManager} from "../../../../../managers/artifact-manager/artifact-manager";
import {ArtifactManagerMock} from "../../../../../managers/artifact-manager/artifact-manager.mock";
import {ISelectionManager} from "../../../../../managers/selection-manager/selection-manager";
import {IStatefulArtifactFactory} from "../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {MessageServiceMock} from "../../../../../core/messages/message.mock";
import {IMessageService} from "../../../../../core/messages/message.svc";
import {LocalizationServiceMock} from "../../../../../core/localization/localization.mock";

describe("PreviewCenter Directive", () => {

    let element: ng.IAugmentedJQuery;
    let controller: PreviewCenterController;
    let scope: ng.IScope;
    let directiveWrapper: string;
    let statefulArtifactFactory: IStatefulArtifactFactory;
    let msgService: IMessageService;
    let localization: LocalizationServiceMock;
    let noop = () => {/*noop*/ };

    beforeEach(angular.mock.module("ui.router"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.component("previewCenter", new PreviewCenterComponent());
    }));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(
        inject(($compile: ng.ICompileService,
            $rootScope: ng.IRootScopeService,
            $templateCache: ng.ITemplateCacheService,
            artifactManager: ArtifactManagerMock,
            _statefulArtifactFactory_: IStatefulArtifactFactory,
            _localization_: LocalizationServiceMock,
            messageService: IMessageService) => {
            $rootScope["config"] = {
                settings: {
                    "StorytellerIsSMB": "false"
                }
            };
            scope = $rootScope.$new();
            scope.$parent["vm"] = {
                isReadonly: false
            };

            artifactManager.selection = {
                getArtifact: () => {
                    return;
                },
                clearAll: () => {
                    return;
                }
            } as ISelectionManager;
            
            scope["systemTaskModel"] = ShapeModelMock.instance().SystemTaskMock();
            scope["userTaskModel"] = ShapeModelMock.instance().UserTaskMock();
            // tslint:disable-next-line: max-line-length
            directiveWrapper = "<div><preview-center user-task-model='true' previous-system-task=\"systemTaskModel\" next-system-task=\"systemTaskModel\" is-user-system-process='true' user-task-model=\"userTaskModel\" /></div>";
            element = $compile(directiveWrapper)(scope);
            scope.$digest();
            controller = element.find("preview-center").isolateScope()["centerCtrl"];
            statefulArtifactFactory = _statefulArtifactFactory_;
            localization = _localization_;
            msgService = messageService;
        }));

    it("Controller Constructor",
        inject(($controller: ng.IControllerService,
                $rootScope: ng.IRootScopeService,
                $window: ng.IWindowService) => {
            // Arrange
            let controllerScope = $rootScope.$new();
            (controllerScope.$parent)["vm"] = {
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

    it("showMore Label", (inject(() => {
        //Arrange
        let event = jQuery.Event("keydown", {
            keyCode: 13
        });
        spyOn(controller, "resizeContentAreas");
        const refreshSpy = spyOn(controller, "refreshView").and.callFake(noop);

        // Act
        controller.showMore("label", event);

        //Assert
        expect(controller.resizeContentAreas).toHaveBeenCalled();
        expect(refreshSpy).toHaveBeenCalled();
    })));

    it("showMore br", (inject(() => {
        //Arrange
        const event = jQuery.Event("keydown", {
            keyCode: 13
        });
        spyOn(controller, "resizeContentAreas");
        const refreshSpy = spyOn(controller, "refreshView").and.callFake(noop);

        // Act
        controller.showMore("br", event);

        //Assert
        expect(controller.resizeContentAreas).toHaveBeenCalled();
        expect(refreshSpy).toHaveBeenCalled();
    })));

    it("showMore nfr", (inject(() => {
        //Arrange
        const event = jQuery.Event("keydown", {
            keyCode: 13
        });
        spyOn(controller, "resizeContentAreas");
        const refreshSpy = spyOn(controller, "refreshView").and.callFake(noop);

        // Act
        controller.showMore("nfr", event);
        expect(refreshSpy).toHaveBeenCalled();

        //Assert
        expect(controller.resizeContentAreas).toHaveBeenCalled();
    })));
});
