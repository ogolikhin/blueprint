import * as angular from "angular";
import "angular-mocks";

import { BPFieldBaseRTFController } from "./base-rtf-controller";

describe("Formly Base RTF Controller", () => {
    let controller: BPFieldBaseRTFController,
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
        inject((
            _$controller_
            ) => {
                $controller = _$controller_;
            }
        )
    );

    beforeEach(() => {
        scope = {};
        controller = $controller(BPFieldBaseRTFController, { $scope: scope });
        angular.element("body").append(`<div class="container">` +
            `<a href="http://www.yahoo.com/">Link to website</a>` +
            `<a linkassemblyqualifiedname="BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink,` +
            ` BluePrintSys.RC.Client.SL.RichText, Version=7.4.0.0, Culture=neutral, PublicKeyToken=null" canclick="True" isvalid="True"` +
            ` href="http://localhost:9801/?ArtifactId=365" target="_blank" artifactid="365">Inline trace</a>` +
            `</div>`);
    });

    describe("handleClick", () => {
        it("click on a link opens a new window", () => {
            let aTag: HTMLElement = angular.element("a")[0];
            let mouseEvent: MouseEvent = createMouseEvent();
            aTag.addEventListener("click", controller.handleClick);

            spyOn(window, "open").and.callFake(function() {
                return true;
            });

            aTag.dispatchEvent(mouseEvent);

            expect(window.open).toHaveBeenCalled();
            expect(window.open).toHaveBeenCalledWith("http://www.yahoo.com/", "_blank");

            aTag.removeEventListener("click", controller.handleClick);
        });

        it("click on an inline trace goes to the artifact", () => {
            let aTag: HTMLElement = angular.element("a")[1];
            let mouseEvent: MouseEvent = createMouseEvent();
            aTag.addEventListener("click", controller.handleClick);

            spyOn(console, "log").and.callFake(function() {
                return true;
            });

            aTag.dispatchEvent(mouseEvent);

            expect(console.log).toHaveBeenCalled();

            aTag.removeEventListener("click", controller.handleClick);
        });
    });

    describe("handleLinks", () => {
        it("adds event listners and attributes", () => {
            let container = angular.element(".container")[0];
            let aTags = container.querySelectorAll("a");
            spyOn(aTags[0], "addEventListener").and.callFake(function() {
                return true;
            });
            spyOn(aTags[1], "addEventListener").and.callFake(function() {
                return true;
            });

            controller.handleLinks(aTags);

            expect(aTags[0].getAttribute("contenteditable")).toBe("false");
            expect(aTags[0].addEventListener).toHaveBeenCalled();
            expect(aTags[0].addEventListener).toHaveBeenCalledWith("click", controller.handleClick);
            expect(aTags[1].getAttribute("contenteditable")).toBe("false");
            expect(aTags[1].addEventListener).toHaveBeenCalled();
            expect(aTags[1].addEventListener).toHaveBeenCalledWith("click", controller.handleClick);
        });

        it("removes event listners", () => {
            let container = angular.element(".container")[0];
            let aTags = container.querySelectorAll("a");
            spyOn(aTags[0], "removeEventListener").and.callFake(function() {
                return true;
            });
            spyOn(aTags[1], "removeEventListener").and.callFake(function() {
                return true;
            });

            controller.handleLinks(aTags);
            controller.handleLinks(aTags, true);

            expect(aTags[0].removeEventListener).toHaveBeenCalled();
            expect(aTags[0].removeEventListener).toHaveBeenCalledWith("click", controller.handleClick);
            expect(aTags[1].removeEventListener).toHaveBeenCalled();
            expect(aTags[1].removeEventListener).toHaveBeenCalledWith("click", controller.handleClick);
        });
    });

    afterEach(() => {
        angular.element("body").empty();
    });
});