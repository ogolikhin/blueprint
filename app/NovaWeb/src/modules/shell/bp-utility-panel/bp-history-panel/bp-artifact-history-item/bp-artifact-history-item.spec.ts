﻿import "../../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPArtifactHistoryItemController} from "./bp-artifact-history-item";
import { LocalizationServiceMock } from "../../../../core/localization.mock";

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
        expect(directiveTest.element.find(".version")).toBeDefined();
        expect(directiveTest.element.find(".author")).toBeDefined();
        expect(directiveTest.element.find("bp-avatar")).toBeDefined();
    });
});