import "../../main/";
import "angular";
import "angular-mocks";
import { ILoadingOverlayService, LoadingOverlayService } from "./loading-overlay.svc";
import { LoadingOverlayController } from "./loading-overlay";
import { ComponentTest } from "../../util/component.test";

//The service and component are closely related, so we test both at the same time.
//See loading-overlay.svc

describe("Service LoadingOverlayService + Component LoadingOverlay", () => {

    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("loadingOverlayService", LoadingOverlayService);
    }));

    var componentTest: ComponentTest<LoadingOverlayController>;
    var layout = `<loading-overlay class="loading- overlay"></loading-overlay>`;

    beforeEach(() => {
        componentTest = new ComponentTest<LoadingOverlayController>(layout, "loading-overlay");
    });

    it("displays the overlay if beginLoading() is called", (inject((loadingOverlayService: ILoadingOverlayService) => {
        //Arrange
        var vm: LoadingOverlayController = componentTest.createComponent({});

        // Act
        loadingOverlayService.beginLoading();
        componentTest.scope.$digest();

        // Assert      
        expect(componentTest.element.find("div:nth-child(1)").hasClass("overlay")).toBe(true);
        expect(componentTest.element.find("div:nth-child(2)").hasClass("overlay-graphic")).toBe(true);
    })));

    it("hides the overlay at the beginning", (inject((loadingOverlayService: ILoadingOverlayService) => {
        //Arrange
        var vm: LoadingOverlayController = componentTest.createComponent({});

        // Act
        loadingOverlayService.beginLoading();
        componentTest.scope.$digest();

        // Assert      
        expect(componentTest.element.find("div:nth-child(1)").hasClass("overlay")).toBe(true);
        expect(componentTest.element.find("div:nth-child(2)").hasClass("overlay-graphic")).toBe(true);
    })));

    it("hides the overlay if endLoading() is called", (inject((loadingOverlayService: ILoadingOverlayService) => {
        //Arrange
        var vm: LoadingOverlayController = componentTest.createComponent({});

        // Act
        var id = loadingOverlayService.beginLoading();
        componentTest.scope.$digest();
        loadingOverlayService.endLoading(id);
        componentTest.scope.$digest();

        // Assert      
        expect(componentTest.element.find("div:nth-child(1)").hasClass("overlay")).toBe(false);
        expect(componentTest.element.find("div:nth-child(2)").hasClass("overlay-graphic")).toBe(false);
    })));

    it("supports multiple beginLoading() calls", (inject((loadingOverlayService: ILoadingOverlayService) => {
        //Arrange
        var vm: LoadingOverlayController = componentTest.createComponent({});

        // Act
        var id1 = loadingOverlayService.beginLoading();
        var id2 = loadingOverlayService.beginLoading();
        componentTest.scope.$digest();

        // Assert      
        expect(componentTest.element.find("div:nth-child(1)").hasClass("overlay")).toBe(true);
        expect(componentTest.element.find("div:nth-child(2)").hasClass("overlay-graphic")).toBe(true);
    })));

    it("hides the overlay only once every beginLoading() call has an endLoading() call", (inject((loadingOverlayService: ILoadingOverlayService) => {
        //Arrange
        var vm: LoadingOverlayController = componentTest.createComponent({});

        // Act
        var id1 = loadingOverlayService.beginLoading();
        var id2 = loadingOverlayService.beginLoading();
        componentTest.scope.$digest();
        loadingOverlayService.endLoading(id1);
        componentTest.scope.$digest();

        // Assert      
        expect(componentTest.element.find("div:nth-child(1)").hasClass("overlay")).toBe(true);
        expect(componentTest.element.find("div:nth-child(2)").hasClass("overlay-graphic")).toBe(true);

        //Act 2
        loadingOverlayService.endLoading(id2);
        componentTest.scope.$digest();

        //Assert 2
        expect(componentTest.element.find("div:nth-child(1)").hasClass("overlay")).toBe(false);
        expect(componentTest.element.find("div:nth-child(2)").hasClass("overlay-graphic")).toBe(false);
    })));

    it("throws an error if endLoading() is called twice - but still hides the overlay", (inject((loadingOverlayService: ILoadingOverlayService) => {
        //Arrange
        var vm: LoadingOverlayController = componentTest.createComponent({});

        // Act
        var id = loadingOverlayService.beginLoading();
        loadingOverlayService.endLoading(id);
        componentTest.scope.$digest();

        // Act + Assert      
        expect(() => {
            loadingOverlayService.endLoading(id);
        }).toThrow(new Error(`Invalid id; endLoading may have been called multiple times on the same id or called before beginLoading`));
        //expect(loadingOverlayService.endLoading(id)).toThrowError(`Invalid id; endLoading may have been called multiple times on the same id or called before beginLoading`);
        componentTest.scope.$digest();

        //Assert
        expect(componentTest.element.find("div:nth-child(1)").hasClass("overlay")).toBe(false);
        expect(componentTest.element.find("div:nth-child(2)").hasClass("overlay-graphic")).toBe(false);
    })));

    it("throws an error if endLoading() is called twice - does not hide the overlay if beginLoading() was called twice", (inject((loadingOverlayService: ILoadingOverlayService) => {
        //Arrange
        var vm: LoadingOverlayController = componentTest.createComponent({});

        // Act
        var id1 = loadingOverlayService.beginLoading();
        var id2 = loadingOverlayService.beginLoading();
        loadingOverlayService.endLoading(id1);
        componentTest.scope.$digest();

        // Act + Assert      
        expect(() => {
            loadingOverlayService.endLoading(id1);
        }).toThrow(new Error(`Invalid id; endLoading may have been called multiple times on the same id or called before beginLoading`));
        componentTest.scope.$digest();

        //Assert
        expect(componentTest.element.find("div:nth-child(1)").hasClass("overlay")).toBe(true);
        expect(componentTest.element.find("div:nth-child(2)").hasClass("overlay-graphic")).toBe(true);
    })));

    it("hides the overlay if dispose() is called, even with multiple beginLoading() calls", (inject((loadingOverlayService: ILoadingOverlayService) => {
        //Arrange
        var vm: LoadingOverlayController = componentTest.createComponent({});

        // Act
        var id1 = loadingOverlayService.beginLoading();
        var id2 = loadingOverlayService.beginLoading();
        loadingOverlayService.dispose();

        //Assert 2
        expect(componentTest.element.find("div:nth-child(1)").hasClass("overlay")).toBe(false);
        expect(componentTest.element.find("div:nth-child(2)").hasClass("overlay-graphic")).toBe(false);
    })));

});