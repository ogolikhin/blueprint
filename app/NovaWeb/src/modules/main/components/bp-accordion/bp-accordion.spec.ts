import "../../main.module";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../../util/component.test";
import {BpAccordionCtrl} from "./bp-accordion";

describe("Component BpAccordion", () => {

    beforeEach(angular.mock.module('app.main'));

    var directiveTest: ComponentTest<BpAccordionCtrl>;
    var layout = `
        <bp-accordion accordion-heading-height="33">
            <bp-accordion-panel accordion-panel-heading="Discussions">Lorem ipsum dolor sit amet.</bp-accordion-panel>
            <bp-accordion-panel accordion-panel-heading="Properties" accordion-panel-id="my-panel">Mauris aliquet feugiat vulputate.</bp-accordion-panel>
            <bp-accordion-panel accordion-panel-heading="Relationships" accordion-panel-heading-height="66">Etiam eget urna ullamcorper.</bp-accordion-panel>
        </bp-accordion>
    `;

    beforeEach(() => {
        directiveTest = new ComponentTest<BpAccordionCtrl>(layout, "bp-accordion");
    });

    describe("the component is created", () => {
        it("all 3 panels have been added", () => {

            //Arrange
            var vm: BpAccordionCtrl = directiveTest.createComponent({});

            //Assert
            expect(vm.accordionPanels.length).toBe(3, "not all the panels have been added");
        });
    });
});