import * as angular from "angular";
import "angular-mocks";

import {BPFieldBaseController} from "./base-controller";

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

    function createMouseEvent(eventType: string = "click"): MouseEvent {
        let mouseEvent = document.createEvent("MouseEvent");
        mouseEvent.initMouseEvent(
            eventType,
            /*bubble*/true, /*cancelable*/true,
            window, null,
            0, 0, 0, 0, /*coordinates*/
            false, false, false, false, /*modifier keys*/
            0/*button=left*/, null
        );

        return mouseEvent;
    }

    beforeEach(
        inject((_$controller_) => {
                $controller = _$controller_;
            }
        )
    );

    beforeEach(() => {
        scope = {};
        controller = $controller(BPFieldBaseController, {$scope: scope});
        angular.element("body").append(`
            <div class="container" style="height:100px;width:100px;overflow:auto;">
                <div class="spacer" style="height:200px;width:100px;"></div>
                <input type="text" style="width:100px;" />
                <iframe id="iframe1"></iframe>
                <iframe id="iframe2"></iframe>
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

        it("focus on button", () => {
            const key = 13; // Enter
            let input: HTMLElement = angular.element("input")[0];
            let event: KeyboardEvent = createKeyEvent(key);
            angular.element(input.parentElement).append("<span><button></button></span>");
            let button: HTMLElement = angular.element("button")[0];

            input.focus();
            expect(document.activeElement).toBe(input);
            expect(document.activeElement).not.toBe(button);

            input.dispatchEvent(event);

            controller.blurOnKey(event);
            expect(document.activeElement).not.toBe(input);
            expect(document.activeElement).toBe(button);
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

    describe("catchClick", () => {
        const catcherId = "123";
        const catcherIdOther = "456";
        const catcherClass = "ui-select__click-catcher";

        it("creates the catcher(s) on dropdown open", () => {
            // Arrange

            // Act
            controller.catchClick(true, catcherId);

            // Assert
            expect(document.querySelectorAll(`.${catcherClass}--${catcherId}`).length).toBe(2);
            expect(document.querySelectorAll(`.${catcherClass}--${catcherIdOther}`).length).toBe(0);
        });

        it("removes the related catcher(s) on dropdown close", () => {
            // Arrange
            const iframe1: HTMLElement = angular.element("#iframe1")[0];
            const catcher1: HTMLElement = angular.element(`<div class="${catcherClass}--${catcherId}"/>`)[0];
            iframe1.parentElement.insertBefore(catcher1, iframe1);
            const catcher1Other: HTMLElement = angular.element(`<div class="${catcherClass}--${catcherIdOther}"/>`)[0];
            iframe1.parentElement.insertBefore(catcher1Other, iframe1);

            const iframe2: HTMLElement = angular.element("#iframe2")[0];
            const catcher2: HTMLElement = angular.element(`<div class="${catcherClass}--${catcherId}"/>`)[0];
            iframe2.parentElement.insertBefore(catcher2, iframe2);
            const catcher2Other: HTMLElement = angular.element(`<div class="${catcherClass}--${catcherIdOther}"/>`)[0];
            iframe2.parentElement.insertBefore(catcher2Other, iframe2);

            // Act
            controller.catchClick(false, catcherId);

            // Assert
            expect(document.querySelectorAll(`.${catcherClass}--${catcherId}`).length).toBe(0);
            expect(document.querySelectorAll(`.${catcherClass}--${catcherIdOther}`).length).toBe(2);
        });
    });

    describe("scrollIntoView", () => {
        it("click", () => {
            let hasBeenClicked = false;
            let container: HTMLElement = angular.element("div.container")[0];
            let input: HTMLElement = angular.element("input")[0];
            angular.element(input).on("click", () => {
                hasBeenClicked = true;
            });
            let event: MouseEvent = createMouseEvent();

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
