import "../../../";
import "angular";
import {ComponentTest} from "../../../../util/component.test";
import { BPArtifactHistoryItemController} from "./bp-artifact-history-item";

describe("Component BPArtifactHistoryItem", () => {

    beforeEach(angular.mock.module("app.shell"));

    let directiveTest: ComponentTest<BPArtifactHistoryItemController>;
    let template = `
        <bp-artifact-history-item 
            artifact-info="null">
        </bp-artifact-history-item>
    `;

    beforeEach(() => {
        directiveTest = new ComponentTest<BPArtifactHistoryItemController>(template, "bp-artifact-history-item");
    });

    describe("the component is created", () => {
        it("should be visible by default", () => {

            //Arrange
            directiveTest.createComponent({});

            //Assert
            expect(directiveTest.element.find(".version")).toBeDefined();
            expect(directiveTest.element.find(".author")).toBeDefined();
            expect(directiveTest.element.find("bp-avatar")).toBeDefined();
        });
    });
});