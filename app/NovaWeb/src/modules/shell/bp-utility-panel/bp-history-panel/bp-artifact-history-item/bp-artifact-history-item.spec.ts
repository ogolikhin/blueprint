import "../../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPArtifactHistoryItemController } from "./bp-artifact-history-item";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";

describe("Component BPArtifactHistoryItem", () => {

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));

    let directiveTest: ComponentTest<BPArtifactHistoryItemController>;
    let template = `
        <bp-artifact-history-item 
            artifact-info="null">
        </bp-artifact-history-item>
    `;

    beforeEach(() => {
        directiveTest = new ComponentTest<BPArtifactHistoryItemController>(template, "bp-artifact-history-item");
    });

    it("should be visible by default", () => {

        //Arrange
        directiveTest.createComponent({});

        //Assert
        expect(directiveTest.element.find(".author").length).toBe(1);
        expect(directiveTest.element.find("bp-avatar").length).toBe(1);
    });
});