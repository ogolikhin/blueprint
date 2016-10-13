import {BPTourController} from "./bp-tour";

describe("Tour Dialog", () => {
    let dialogMock: any;
    let controller: BPTourController;

    beforeEach(() => {
        dialogMock = {
            close: function () {
            }
        };
        controller = new BPTourController(dialogMock);
    });

    afterEach(() => {
        controller = null;
        dialogMock = null;
    });

    it("created images correctly", () => {
        expect(controller.images).not.toBeNull();
        expect(controller.images.length).toBe(21);
        expect(controller.images.filter(i => !i.src).length).toBe(0);
    });

    describe("when close method is called", () => {
        beforeEach(() => {
            spyOn(dialogMock, "close");
            controller.close();
        });

        it("closes Bootstrap UI dialog", () => {
            expect(dialogMock.close).toHaveBeenCalled();
        });
    });
});
