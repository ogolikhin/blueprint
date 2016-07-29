import "../../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPDiscussionReplyItemController } from "./bp-discussion-reply-item";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";

describe("Component BPDiscussionReplyItem", () => {

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));

    let directiveTest: ComponentTest<BPDiscussionReplyItemController>;
    let template = `<bp-discussion-reply-item 
            reply-info="null">
        </bp-discussion-reply-item>`;

    beforeEach(() => {
        directiveTest = new ComponentTest<BPDiscussionReplyItemController>(template, "bp-discussion-reply-item");
    });

    it("should be visible by default", () => {

        //Arrange
        directiveTest.createComponent({});

        //Assert
        expect(directiveTest.element.find(".button-bar").length).toBe(1);
        expect(directiveTest.element.find("bp-avatar").length).toBe(1);
    });
});