import "angular";
import "angular-mocks";

import { IBPFieldBaseController, BPFieldBaseController } from "./base-controller";

describe("Formly Base Controller", () => {
    let controller: BPFieldBaseController,
        $controller: ng.IControllerService,
        scope;

    function createKeyEvent(keyCode: number, eventType: string = "keydown"): KeyboardEvent {
        let keyEvent = document.createEvent("Event");
        keyEvent.initEvent(eventType, true, true);
        keyEvent["which"] = keyCode;
        keyEvent["keyCode"] = keyCode;

        return keyEvent as KeyboardEvent;
    }

    beforeEach(
        inject((
            _$controller_
            ) => {
                $controller = _$controller_
            }
        )
    );

    beforeEach(() => {
        scope = {};
        controller = $controller(BPFieldBaseController, { $scope: scope });
        angular.element("body").append(`
            <div class="container" style="height:100px;width:100px;overflow:auto;">
                <div class="spacer" style="height:200px;width:100px;"></div>
                <input type="text" style="width:100px;" />
            </div>`);
    });

    describe("blurOnKey", () => {
        it("no key, defaults to Enter", () => {
            const key = 13; // Enter
            let input: HTMLElement = angular.element("input")[0];
            let event: KeyboardEvent = createKeyEvent(key);

            input.focus();
            expect(document.activeElement).toBe(input);

            input.dispatchEvent(event);

            controller.blurOnKey(event);
            expect(document.activeElement).not.toBe(input);
        });

        it("custom key", () => {
            const key = 65; // "A"
            let input: HTMLElement = angular.element("input")[0];
            let event: KeyboardEvent = createKeyEvent(key);

            input.focus();
            expect(document.activeElement).toBe(input);

            input.dispatchEvent(event);

            controller.blurOnKey(event, key);
            expect(document.activeElement).not.toBe(input);
        });

        it("set of custom keys", () => {
            const key = [65, 13]; // "A", Enter
            let input: HTMLElement = angular.element("input")[0];
            let event: KeyboardEvent = createKeyEvent(13); // Enter

            input.focus();
            expect(document.activeElement).toBe(input);

            input.dispatchEvent(event);

            controller.blurOnKey(event, key);
            expect(document.activeElement).not.toBe(input);
        });
    });

    describe("closeDropdownOnTab", () => {
        it("tab key", () => {
            const tabKey = 9; // Tab key code
            let input: HTMLElement = angular.element("input")[0];
            let event: KeyboardEvent = createKeyEvent(tabKey);
            let onBlurSpy = spyOn(controller, "blurOnKey").and.callThrough();

            input.focus();
            expect(document.activeElement).toBe(input);

            input.dispatchEvent(event);

            controller.closeDropdownOnTab(event);
            expect(document.activeElement).not.toBe(input);

            expect(onBlurSpy).toHaveBeenCalled();
        });
    });

    describe("scrollIntoView", () => {
        it("click", () => {
            let hasBeenClicked = false;
            let container: HTMLElement = angular.element("div.container")[0];
            let spacer: HTMLElement = angular.element("div.spacer")[0];
            let input: HTMLElement = angular.element("input")[0];
            angular.element(input).on("click", () => {
                hasBeenClicked = true;
            });
            let event: MouseEvent = new MouseEvent("click");

            container.dispatchEvent(event);
            controller.scrollIntoView(event);

            expect(document.activeElement).toBe(input);
            expect(container.scrollTop).toBeGreaterThan(100);
            expect(hasBeenClicked).toBe(true);
        });
    });

    afterEach(() => {
        angular.element("body").empty();
    });
});