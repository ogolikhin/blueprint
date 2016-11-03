import * as angular from "angular";
import "angular-mocks";

import {BPFieldBaseRTFController} from "./base-rtf-controller";
import {NavigationServiceMock} from "../../../../core/navigation/navigation.svc.mock";

describe("Formly Base RTF Controller", () => {
    let scope, rootScope;
    let controller: BPFieldBaseRTFController;

    function createMouseEvent(eventType: string = "click"): MouseEvent {
        const mouseEvent = document.createEvent("MouseEvent");
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

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("navigationService", NavigationServiceMock);
    }));

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
            const aTag: HTMLElement = angular.element("a")[0];
            const mouseEvent: MouseEvent = createMouseEvent();
            aTag.addEventListener("click", controller.handleClick);

            spyOn(window, "open");

            aTag.dispatchEvent(mouseEvent);

            expect(window.open).toHaveBeenCalled();
            expect(window.open).toHaveBeenCalledWith("http://www.yahoo.com/", "_blank");

            aTag.removeEventListener("click", controller.handleClick);
        });

        it("click on an inline trace goes to the artifact", () => {
            const aTag: HTMLElement = angular.element("a")[1];
            const mouseEvent: MouseEvent = createMouseEvent();
            aTag.addEventListener("click", controller.handleClick);

            spyOn(controller.navigationService, "navigateTo");

            aTag.dispatchEvent(mouseEvent);

            expect(controller.navigationService.navigateTo).toHaveBeenCalled();
            expect(controller.navigationService.navigateTo).toHaveBeenCalledWith({ id: 365 });

            aTag.removeEventListener("click", controller.handleClick);
        });
    });

    describe("handleLinks", () => {
        it("adds event listners", () => {
            const container = angular.element(".container")[0];
            const aTags = container.querySelectorAll("a");
            spyOn(aTags[0], "addEventListener");
            spyOn(aTags[1], "addEventListener");

            controller.handleLinks(aTags);

            expect(aTags[0].addEventListener).toHaveBeenCalled();
            expect(aTags[1].addEventListener).toHaveBeenCalled();
        });

        it("removes event listners", () => {
            const container = angular.element(".container")[0];
            const aTags = container.querySelectorAll("a");
            spyOn(aTags[0], "removeEventListener");
            spyOn(aTags[1], "removeEventListener");

            controller.handleLinks(aTags);
            controller.handleLinks(aTags, true);

            expect(aTags[0].removeEventListener).toHaveBeenCalled();
            expect(aTags[1].removeEventListener).toHaveBeenCalled();
        });

        it("adds mouseover/out listners in IE", () => {
            const body = angular.element("body")[0];
            body.classList.add("is-msie");
            const container = angular.element(".container")[0];
            const aTags = container.querySelectorAll("a");
            spyOn(aTags[0], "addEventListener");
            spyOn(aTags[1], "addEventListener");

            controller.handleLinks(aTags);

            expect(aTags[0].addEventListener).toHaveBeenCalled();
            expect(aTags[0].addEventListener).toHaveBeenCalledWith("mouseover", controller.disableEditability);
            expect(aTags[0].addEventListener).toHaveBeenCalledWith("mouseout", controller.enableEditability);
            expect(aTags[1].addEventListener).toHaveBeenCalled();
            expect(aTags[1].addEventListener).toHaveBeenCalledWith("mouseover", controller.disableEditability);
            expect(aTags[1].addEventListener).toHaveBeenCalledWith("mouseout", controller.enableEditability);
        });

        it("removes mouseover/out listners in IE", () => {
            const body = angular.element("body")[0];
            body.classList.add("is-msie");
            const container = angular.element(".container")[0];
            const aTags = container.querySelectorAll("a");
            spyOn(aTags[0], "removeEventListener");
            spyOn(aTags[1], "removeEventListener");

            controller.handleLinks(aTags);
            controller.handleLinks(aTags, true);

            expect(aTags[0].removeEventListener).toHaveBeenCalled();
            expect(aTags[0].removeEventListener).toHaveBeenCalledWith("mouseover", controller.disableEditability);
            expect(aTags[0].removeEventListener).toHaveBeenCalledWith("mouseout", controller.enableEditability);
            expect(aTags[1].removeEventListener).toHaveBeenCalled();
            expect(aTags[1].removeEventListener).toHaveBeenCalledWith("mouseover", controller.disableEditability);
            expect(aTags[1].removeEventListener).toHaveBeenCalledWith("mouseout", controller.enableEditability);
        });
    });

    describe("dis/enableEditability", () => {
        it("contenteditable=true", () => {
            const body = angular.element("body")[0];
            controller.editorBody = body;

            controller.enableEditability(null);

            expect(body.getAttribute("contenteditable")).toBe("true");
        });

        it("contenteditable=false", () => {
            const body = angular.element("body")[0];
            controller.editorBody = body;

            controller.enableEditability(null);
            controller.disableEditability(null);

            expect(body.getAttribute("contenteditable")).toBe("false");
        });
    });

    describe("handleMutation", () => {
        it("adds/removes event listners on mutated A tags", () => {
            const container = angular.element("body")[0].querySelector(".container");
            const mutationRecord: MutationRecord = {
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

            spyOn(controller, "handleLinks");

            controller.handleMutation(mutationRecord);

            expect(controller.handleLinks).toHaveBeenCalled();
            expect(controller.handleLinks).toHaveBeenCalledTimes(4);
        });
    });

    afterEach(() => {
        angular.element("body").empty();
    });
});
