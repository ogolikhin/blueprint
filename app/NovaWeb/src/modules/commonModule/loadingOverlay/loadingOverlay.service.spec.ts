import "./";
import "angular-mocks";
import {ILoadingOverlayService} from "./loadingOverlay.service";

describe("Service LoadingOverlayService + Component LoadingOverlay", () => {
    beforeEach(angular.mock.module("loadingOverlay"));

    it("displays the overlay if beginLoading() is called", (inject((loadingOverlayService: ILoadingOverlayService) => {
        // Act
        loadingOverlayService.beginLoading();

        // Assert
        expect(loadingOverlayService.displayOverlay).toBe(true);
    })));

    it("hides the overlay if endLoading() is called", (inject((loadingOverlayService: ILoadingOverlayService) => {
        // Act
        const id = loadingOverlayService.beginLoading();
        loadingOverlayService.endLoading(id);

        // Assert
        expect(loadingOverlayService.displayOverlay).toBe(false);
    })));

    it("supports multiple beginLoading() calls", (inject((loadingOverlayService: ILoadingOverlayService) => {
        // Act
        loadingOverlayService.beginLoading();
        loadingOverlayService.beginLoading();

        // Assert
        expect(loadingOverlayService.displayOverlay).toBe(true);
    })));

    it("hides the overlay only once every beginLoading() call has an endLoading() call", (inject((loadingOverlayService: ILoadingOverlayService) => {
        // Act
        const id1 = loadingOverlayService.beginLoading();
        const id2 = loadingOverlayService.beginLoading();
        loadingOverlayService.endLoading(id1);

        // Assert
        expect(loadingOverlayService.displayOverlay).toBe(true);

        //Act 2
        loadingOverlayService.endLoading(id2);

        //Assert 2
        expect(loadingOverlayService.displayOverlay).toBe(false);
    })));


    it("throws an error if endLoading() is called twice - but still hides the overlay", (inject((loadingOverlayService: ILoadingOverlayService) => {
        // Act
        const id = loadingOverlayService.beginLoading();
        loadingOverlayService.endLoading(id);

        // Act + Assert
        expect(() => {
            loadingOverlayService.endLoading(id);
        }).toThrow(new Error(`Invalid id; endLoading may have been called multiple times on the same id or called before beginLoading`));

        //Assert
        expect(loadingOverlayService.displayOverlay).toBe(false);
    })));

    it("throws an error if endLoading() is called twice - does not hide the overlay if beginLoading() was called twice",
        (inject((loadingOverlayService: ILoadingOverlayService) => {
            // Act
            const id1 = loadingOverlayService.beginLoading();
            loadingOverlayService.beginLoading();
            loadingOverlayService.endLoading(id1);

            // Act + Assert
            expect(() => {
                loadingOverlayService.endLoading(id1);
            }).toThrow(new Error(`Invalid id; endLoading may have been called multiple times on the same id or called before beginLoading`));

            //Assert
            expect(loadingOverlayService.displayOverlay).toBe(true);
        })));

    it("hides the overlay if dispose() is called, even with multiple beginLoading() calls", (inject((loadingOverlayService: ILoadingOverlayService) => {
        // Act
        loadingOverlayService.beginLoading();
        loadingOverlayService.beginLoading();
        loadingOverlayService.dispose();

        //Assert 2
        expect(loadingOverlayService.displayOverlay).toBe(false);
    })));
});
