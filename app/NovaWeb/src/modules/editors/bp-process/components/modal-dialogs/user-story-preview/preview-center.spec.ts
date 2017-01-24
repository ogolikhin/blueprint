﻿import {CommunicationManager} from "./../../../services/communication-manager";
import * as angular from "angular";
import "angular-ui-router";
import {PreviewCenterController, PreviewCenterComponent} from "./preview-center";
import {ShapeModelMock} from "../../diagram/presentation/graph/shapes/shape-model.mock";
import {IStatefulArtifactFactory} from "../../../../../managers/artifact-manager/";
import {StatefulArtifactFactoryMock} from "../../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {LocalizationServiceMock} from "../../../../../commonModule/localization/localization.service.mock";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {MessageServiceMock} from "../../../../../main/components/messages/message.mock";
import {SelectionManagerMock} from "../../../../../managers/selection-manager/selection-manager.mock";

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

    beforeEach(angular.mock.module(($compileProvider: ng.ICompileProvider) => {
        $compileProvider.component("previewCenter", new PreviewCenterComponent());
    }));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("communicationManager", CommunicationManager);
    }));

    beforeEach(
        inject(($compile: ng.ICompileService,
            $rootScope: ng.IRootScopeService,
            $templateCache: ng.ITemplateCacheService,
            selectionManager: SelectionManagerMock,
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
