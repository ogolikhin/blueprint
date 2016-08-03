import "../../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPArtifactDiscussionItemController } from "./bp-artifact-discussion-item";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";

describe("Component BPArtifactDiscussionItem", () => {

    beforeEach(angular.mock.module("app.shell"));

    let directiveTest: ComponentTest<BPArtifactDiscussionItemController>;
    let template = `<bp-artifact-discussion-item 
            discussion-info="null"
            artifact-id="1">
        </bp-artifact-discussion-item>`;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));

    beforeEach(() => {
        directiveTest = new ComponentTest<BPArtifactDiscussionItemController>(template, "bp-artifact-discussion-item");
    });

    it("should be visible by default", () => {

        //Arrange
        directiveTest.createComponent({});

        //Assert
        expect(directiveTest.element.find(".button-bar").length).toBe(1);
        expect(directiveTest.element.find("bp-avatar").length).toBe(1);
    });
});