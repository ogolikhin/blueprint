import "../../"; // app.core
import * as angular from "angular";
import "angular-mocks";
// import "angular-sanitize";
import {ComponentTest} from "../../../util/component.test";
import {BPSelectController} from "./bp-select";

describe("Component BPSelect", () => {
    beforeEach(angular.mock.module("app.shared"));

    let directiveTest: ComponentTest<BPSelectController>;
    let bindings = {
        sortAscending: false,
        sortOptions: [
            {value: false, label: "sort by latest"},
            {value: true, label: "sort by earliest"}
        ]
    };
    let template = `
        <bp-select
            ng-model="sortAscending"
            ng-change="changeSortOrder()"
            options="sortOptions">
        </bp-select>
    `;

    beforeEach(() => {
        directiveTest = new ComponentTest<BPSelectController>(template, "bpSelect");
    });

    it("should be visible by default", () => {
        // Arrange
        // Act
        directiveTest.createComponent(bindings);

        //Assert
        expect(directiveTest.element.find(".dropdown-menu")).toBeDefined();
        expect(directiveTest.element.find("ul")).toBeDefined();
    });

    it("should update model when option is selected", () => {
        // Arrange
        let selectedOption = {value: true, label: "sort by earliest"};
        let ctrl: BPSelectController = directiveTest.createComponent(bindings);

        // Act
        ctrl.onOptionSelect(selectedOption);

        // Assert
        expect(ctrl.ngModel).toBe(true);
    });
});
