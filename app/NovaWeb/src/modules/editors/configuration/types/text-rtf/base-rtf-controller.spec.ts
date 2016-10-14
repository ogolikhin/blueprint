import * as angular from "angular";
import "angular-mocks";

import {BPFieldBaseRTFController} from "./base-rtf-controller";

describe("Formly Base RTF Controller", () => {
    let scope, rootScope;
    let controller: BPFieldBaseRTFController;

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
        inject(
            ($rootScope: ng.IRootScopeService, $controller: ng.IControllerService) => {
                rootScope = $rootScope;
                scope = rootScope.$new();
                controller = $controller(BPFieldBaseRTFController, {$scope: scope});
                angular.element("body").append(`<div class="container">` +
                    `<a class="added" href="http://www.yahoo.com/">Link #1</a>` +
                    `<a linkassemblyqualifiedname="BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink,` +
                    ` BluePrintSys.RC.Client.SL.RichText, Version=7.4.0.0, Culture=neutral, PublicKeyToken=null"` +
                    ` class="removed" canclick="True" isvalid="True"` +
                    ` href="http://localhost:9801/?ArtifactId=365" target="_blank" artifactid="365">Inline trace #1</a>` +
                    `<p class="added"><a href="http://www.google.com/">Link #2</a></p>` +
                    `<p class="removed"><a href="http://www.cnn.com/">Link #3</a></p>` +
                    `</div>`);
            }
        )
    );

    describe("handleClick", () => {
        it("click on a link opens a new window", () => {
            let aTag: HTMLElement = angular.element("a")[0];
            let mouseEvent: MouseEvent = createMouseEvent();
            aTag.addEventListener("click", controller.handleClick);

            spyOn(window, "open").and.callFake(function () {
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

            spyOn(console, "log").and.callFake(function () {
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
            spyOn(aTags[0], "addEventListener").and.callFake(function () {
            });
            spyOn(aTags[1], "addEventListener").and.callFake(function () {
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
            spyOn(aTags[0], "removeEventListener").and.callFake(function () {
            });
            spyOn(aTags[1], "removeEventListener").and.callFake(function () {
            });

            controller.handleLinks(aTags);
            controller.handleLinks(aTags, true);

            expect(aTags[0].removeEventListener).toHaveBeenCalled();
            expect(aTags[0].removeEventListener).toHaveBeenCalledWith("click", controller.handleClick);
            expect(aTags[1].removeEventListener).toHaveBeenCalled();
            expect(aTags[1].removeEventListener).toHaveBeenCalledWith("click", controller.handleClick);
        });
    });

    describe("handleMutation", () => {
        it("adds/removes event listners on mutated A tags", () => {
            let container = angular.element("body")[0].querySelector(".container");
            let mutationRecord: MutationRecord = {
                type: "childList",
                target: container,
                addedNodes: container.querySelectorAll(".added"),
                removedNodes: container.querySelectorAll(".removed"),
                previousSibling: null,
                nextSibling: null,
                attributeName: null,
                attributeNamespace: null,
                oldValue: null
            };
            spyOn(controller, "handleLinks").and.callFake(function () {
            });

            controller.handleMutation(mutationRecord);

            expect(controller.handleLinks).toHaveBeenCalled();
            expect(controller.handleLinks).toHaveBeenCalledTimes(4);
        });
    });

    afterEach(() => {
        angular.element("body").empty();
    });
});
