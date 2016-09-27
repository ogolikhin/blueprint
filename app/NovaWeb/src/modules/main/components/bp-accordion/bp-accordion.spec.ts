import "../../";
import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../../util/component.test";
import {BpAccordionCtrl} from "./bp-accordion";

describe("Component BpAccordion", () => {

    beforeEach(angular.mock.module("app.main"));

    let directiveTest: ComponentTest<BpAccordionCtrl>;
    let $timeout: ng.ITimeoutService;
    /* tslint:disable */
    let layout = `
        <bp-accordion accordion-heading-height="33">
            <bp-accordion-panel accordion-panel-heading="Discussions" accordion-panel-class="utility-panel-discussions">Lorem ipsum dolor sit amet.</bp-accordion-panel>
            <bp-accordion-panel accordion-panel-heading="Properties" accordion-panel-id="my-panel">Mauris aliquet feugiat vulputate.</bp-accordion-panel>
            <bp-accordion-panel accordion-panel-heading="Relationships" accordion-panel-heading-height="66">Etiam eget urna ullamcorper.</bp-accordion-panel>
        </bp-accordion>`;
    /* tslint:enable */


    beforeEach(inject((_$timeout_: ng.ITimeoutService) => {
        directiveTest = new ComponentTest<BpAccordionCtrl>(layout, "bp-accordion");
        $timeout = _$timeout_;
    }));

    describe("the component is created", () => {
        it("all 3 panels have been added", () => {

            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});

            //Assert
            expect(accordion.getPanels().length).toBe(3, "not all the panels have been added");
        });

        it("all 3 panels have been added with default height", () => {

            //Arrange
            let layoutDefaultHeading = `
                <bp-accordion>
                    <bp-accordion-panel accordion-panel-heading="Discussions">Lorem ipsum dolor sit amet.</bp-accordion-panel>
                    <bp-accordion-panel accordion-panel-heading="Properties">Mauris aliquet feugiat vulputate.</bp-accordion-panel>
                    <bp-accordion-panel accordion-panel-heading="Relationships"Etiam eget urna ullamcorper.</bp-accordion-panel>
                </bp-accordion>
            `;
            directiveTest = new ComponentTest<BpAccordionCtrl>(layoutDefaultHeading, "bp-accordion");
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            let panels = accordion.getPanels();

            //Act
            accordion.recalculateLayout(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panels[1].getElement().style.height).toBe(panels[2].getElement().style.height, "2nd and 3rd panels don't match");
        });

        it("1st panel is open by default", () => {

            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            $timeout.flush();

            let panels = accordion.getPanels();

            //Act
            accordion.recalculateLayout(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panels[0].getElement().className).toContain("bp-accordion-panel-open", "1st panel is not open");
            expect(panels[1].getElement().className).not.toContain("bp-accordion-panel-open", "2nd panel is open");
            expect(panels[2].getElement().className).not.toContain("bp-accordion-panel-open", "3rd panel is open");
        });

        it("random id for panel 1 and 3, custom id for panel 2", () => {

            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            let panels = accordion.getPanels();

            //Act
            accordion.recalculateLayout(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panels[0].accordionPanelId).toMatch(/bp-accordion-panel-\d{0,5}/, "1st panel doesn't have an id");
            expect(panels[1].accordionPanelId).toBe("my-panel", "2nd panel's id is not the custom one");
            expect(panels[2].accordionPanelId).toMatch(/bp-accordion-panel-\d{0,5}/, "3rd panel doesn't have an id");
            expect(panels[0].accordionPanelId).not.toBe(panels[2].accordionPanelId, "1st and 3rd panels have the same id");
        });

        it("default heading height for panel 1 and 2, custom height for panel 3", () => {

            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            let panels = accordion.getPanels();

            //Act
            accordion.recalculateLayout(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panels[0].accordionPanelHeadingHeight).toBe("33", "1st panel's heading height is not the default one");
            expect(panels[1].accordionPanelHeadingHeight).toBe("33", "2nd panel's heading height is not the default one");
            expect(panels[2].accordionPanelHeadingHeight).toBe("66", "2nd panel's heading height is not the custom one");
        });

        it("open 2nd panel", () => {
            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            $timeout.flush();
            let panels = accordion.getPanels();

            //Act
            panels[1].openPanel();

            //Assert
            expect(panels[0].getElement().className).not.toContain("bp-accordion-panel-open", "1st panel is open");
            expect(panels[1].getElement().className).toContain("bp-accordion-panel-open", "2nd panel has not been opened");
            expect(panels[2].getElement().className).not.toContain("bp-accordion-panel-open", "3rd panel is open");
        });

        it("open 2nd panel and then try to open it again by clicking on it again", () => {

            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            let panels = accordion.getPanels();

            //Act
            panels[1].openPanel();

            //Assert
            expect(panels[0].getElement().className).not.toContain("bp-accordion-panel-open", "1st panel is open");
            expect(panels[1].getElement().className).toContain("bp-accordion-panel-open", "2nd panel has not been opened");
            expect(panels[2].getElement().className).not.toContain("bp-accordion-panel-open", "3rd panel is open");
        });

        it("open 2nd panel, pin it, and then open the 3rd panel", () => {
            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            $timeout.flush();
            let panels = accordion.getPanels();
            let panel2Pin = panels[1].getElement().querySelector("input.bp-accordion-panel-pin");

            //Act
            panels[1].openPanel();
            panel2Pin.click();
            panels[2].openPanel();

            //Assert
            expect(panels[0].getElement().className).not.toContain("bp-accordion-panel-open", "1st panel is closed");
            expect(panels[1].getElement().className).toContain("bp-accordion-panel-open", "2nd panel is open");
            expect(panels[2].getElement().className).toContain("bp-accordion-panel-open", "3rd panel is open");
        });

        it("open 2nd panel, pin it, open the 3rd panel, pin it, unpin 2nd panel", () => {

            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            let panels = accordion.getPanels();
            let panel2Pin = panels[1].getElement().querySelector("input.bp-accordion-panel-pin");
            let panel3Pin = panels[2].getElement().querySelector("input.bp-accordion-panel-pin");

            //Act
            //Jasmine doesn't seem to handle radio buttons properly, forcing the checked state
            panels[1].openPanel();
            panel2Pin.click();
            panels[2].openPanel();
            panel3Pin.click();
            panel2Pin.click();

            //Assert
            expect(panels[0].getElement().className).not.toContain("bp-accordion-panel-open", "1st panel is open");
            expect(panels[1].getElement().className).not.toContain("bp-accordion-panel-open", "2nd panel has not been closed");
            expect(panels[2].getElement().className).toContain("bp-accordion-panel-open", "3rd panel has not been opened");
        });

        it("redistribute height: 1st panel open", () => {

            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            $timeout.flush();
            let panels = accordion.getPanels();

            //Act
            accordion.recalculateLayout(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panels[0].getElement().style.height).toBe("calc(100% - 99px)", "1st panel's height is wrong");
            expect(panels[1].getElement().style.height).toBe("33px", "2nd panel's height is wrong");
            expect(panels[2].getElement().style.height).toBe("66px", "3rd panel's height is wrong");
        });

        it("redistribute height: panel 1 and 2 open", () => {

            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            $timeout.flush();
            let panels = accordion.getPanels();
            let panel1Pin = panels[0].getElement().querySelector("input.bp-accordion-panel-pin");

            //Act
            panels[0].openPanel();
            panel1Pin.click(); //pinning 1st panel
            panels[1].openPanel();

            //Assert
            expect(panels[0].getElement().style.height).toBe("calc(50% - 33px)", "1st panel's height is wrong");
            expect(panels[1].getElement().style.height).toBe("calc(50% - 33px)", "2nd panel's height is wrong");
            expect(panels[2].getElement().style.height).toBe("66px", "3rd panel's height is wrong");
        });

        it("redistribute height: panel 1 and 3 open", () => {

            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            let panels = accordion.getPanels();
            let panel1Pin = panels[0].getElement().querySelector("input.bp-accordion-panel-pin");

            //Act
            panels[0].openPanel();
            panel1Pin.click(); //pinning 1st panel
            panels[2].openPanel();

            //Assert
            expect(panels[0].getElement().style.height).toBe("calc(50% - 16.5px)", "1st panel's height is wrong");
            expect(panels[1].getElement().style.height).toBe("33px", "2nd panel's height is wrong");
            expect(panels[2].getElement().style.height).toBe("calc(50% - 16.5px)", "3rd panel's height is wrong");
        });

        it("redistribute height: all 3 panels open", () => {
            //Arrange
            let accordion: BpAccordionCtrl = directiveTest.createComponent({});
            let panels = accordion.getPanels();
            let panel1Pin = panels[0].getElement().querySelector("input.bp-accordion-panel-pin");
            let panel2Pin = panels[1].getElement().querySelector("input.bp-accordion-panel-pin");

            //Act
            panels[0].openPanel();
            panel1Pin.click(); //pinning 1st panel
            panels[1].openPanel();
            panel2Pin.click(); //pinning 2nd panel
            panels[2].openPanel();

            //Assert
            expect(panels[0].getElement().style.height).toBe(panels[1].getElement().style.height, "1st and 2nd panels don't match");
            expect(panels[1].getElement().style.height).toBe(panels[2].getElement().style.height, "2nd and 3rd panels don't match");
        });
    });
});
