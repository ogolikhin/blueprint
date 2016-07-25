// import "../../../main";
// import "../../../shell";

import "angular";
import "angular-mocks";
import "angular-sanitize";

import { ComponentTest } from "../../../util/component.test";
import { BpArtifactInfoController} from "./bp-artifact-info";



describe("Component BpArtifactInfo", () => {

    let componentTest: ComponentTest<BpArtifactInfoController>;
    let template = `<bp-artifact-info></bp-artifact-info>`;
    let vm: BpArtifactInfoController;

    beforeEach(angular.mock.module("app.shell"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {

    }));

    beforeEach(() => {
        componentTest = new ComponentTest<BpArtifactInfoController>(template, "bp-artifact-info");
        vm = componentTest.createComponent({});
    });

    afterEach(() => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(componentTest.element.find(".artifact-icon").length).toBe(1);
        expect(componentTest.element.find(".readonly-indicator").length).toBe(1);
        expect(componentTest.element.find(".lock-indicator").length).toBe(1);
        expect(componentTest.element.find(".dirty-indicator").length).toBe(1);
    });

});