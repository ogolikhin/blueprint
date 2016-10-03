import * as angular from "angular";
import {ZoomableImageDirective, ZoomableImageModalController} from "./zoomable-image";
import { ModalServiceInstanceMock, ModalServiceMock } from "../../../../../shell/login/mocks.spec";

describe("Zoomable Image Directive", () => {

    /* tslint:disable:max-line-length */
    const directiveTemplate = `<zoomable-image class="img-responsive preview-image-placeholder" enable-zoom="true" image-src="image.png" image-alt="test alt text"></zoomable-image>`;
    /* tslint:enable:max-line-length */
    let element: ng.IAugmentedJQuery;
    let scope;

/*
    let $uibModal: angular.ui.bootstrap.IModalService;
    let $uibModalInstance = {
        close() { },
        dismiss() { },
        cancel() { }
    };
*/  

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $provide.service("$uibModalInstance", ModalServiceInstanceMock);
        $provide.service("$uibModal", ModalServiceMock);
        $compileProvider.directive("zoomableImage", ZoomableImageDirective.directive);
    }));

    beforeEach(inject(($compile: ng.ICompileService,
        $rootScope: ng.IRootScopeService) => {

        scope = $rootScope.$new();

        element = $compile(directiveTemplate)(scope);
        scope.$digest();
    }));


    it("can show a directive", () => {
        expect(element.find("img").length).toEqual(1);
        expect(element.find("i").length).toEqual(1);
    });

    it("should have proper attributes passed in", () => {
        const directiveScope: any = element.isolateScope();

        expect(directiveScope.imageSrc).toEqual("image.png");
        expect(directiveScope.imageAlt).toEqual("test alt text");
        expect(directiveScope.enableZoom).toEqual(true);
    });

    it("should call zoomImage()", () => {

        // Arrange
        const directiveScope: any = element.isolateScope();

        // Act
        directiveScope.zoomImage();

        // Asset
        expect(directiveScope.zoomImage).not.toThrowError();
    });

    it("should open a new modal", inject(($controller: ng.IControllerService,
        $uibModal: angular.ui.bootstrap.IModalService,
        $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance) => {

        // Arrange
        const directiveScope: any = element.isolateScope();
        const model = {
            image: directiveScope.imageSrc,
            alt: directiveScope.imageAlt
        };

        // Act
        const controller = $controller(ZoomableImageModalController, {
            $scope: scope.$new(),
            $uibModalInstance,
            imageAttributes: model
        });

        // Asset
        expect(controller.cancel).toBeDefined();
    }));

    it("should close a new modal", inject(($controller: ng.IControllerService,
        $uibModal: angular.ui.bootstrap.IModalService,
        $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance) => {

        // Arrange
        const directiveScope: any = element.isolateScope();
        const model = {
            image: directiveScope.imageSrc,
            alt: directiveScope.imageAlt
        };
        const controller = $controller(ZoomableImageModalController, {
            $scope: scope.$new(),
            $uibModalInstance,
            imageAttributes: model
        });
        const dismissSpy = spyOn($uibModalInstance, "dismiss").and.callThrough();

        // Act
        controller.cancel();

        // Asset
        expect(dismissSpy).toHaveBeenCalled();
    }));

});
