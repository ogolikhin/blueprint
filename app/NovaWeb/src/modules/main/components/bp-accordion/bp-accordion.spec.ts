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
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});

            //Assert
            expect(accordion.getPanels().length).toBe(3, "not all the panels have been added");
        });

        it("1st panel is open by default", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();

            //Assert
            expect(panels[0].accordionPanelIsOpen).toBe(true, "1st panel is not open");
            expect(panels[1].accordionPanelIsOpen).toBe(false || undefined, "2nd panel is open");
            expect(panels[2].accordionPanelIsOpen).toBe(false || undefined, "3rd panel is open");
        });

        it("random id for panel 1 and 3, custom id for panel 2", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();

            //Assert
            expect(panels[0].accordionPanelId).toMatch(/bp-accordion-panel-\d{0,5}/, "1st panel doesn't have an id");
            expect(panels[1].accordionPanelId).toBe("my-panel", "2nd panel's id is not the custom one");
            expect(panels[2].accordionPanelId).toMatch(/bp-accordion-panel-\d{0,5}/, "3rd panel doesn't have an id");
            expect(panels[0].accordionPanelId).not.toBe(panels[2].accordionPanelId, "1st and 3rd panels have the same id");
        });

        it("default heading height for panel 1 and 2, custom height for panel 3", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();

            //Assert
            expect(panels[0].accordionPanelHeadingHeight).toBe("33", "1st panel's heading height is not the default one");
            expect(panels[1].accordionPanelHeadingHeight).toBe("33", "2nd panel's heading height is not the default one");
            expect(panels[2].accordionPanelHeadingHeight).toBe("66", "2nd panel's heading height is not the custom one");
        });
    });
});