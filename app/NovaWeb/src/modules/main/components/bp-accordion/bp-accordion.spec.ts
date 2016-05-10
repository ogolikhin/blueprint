import "../../main.module";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../../util/component.test";
import {BpAccordionCtrl} from "./bp-accordion";

describe("Component BpAccordion", () => {

    beforeEach(angular.mock.module("app.main"));

    var directiveTest: ComponentTest<BpAccordionCtrl>;
    /* tslint:disable */
    var layout = `
        <bp-accordion accordion-heading-height="33">
            <bp-accordion-panel accordion-panel-heading="Discussions" accordion-panel-class="utility-panel-discussions">Lorem ipsum dolor sit amet.</bp-accordion-panel>
            <bp-accordion-panel accordion-panel-heading="Properties" accordion-panel-id="my-panel">Mauris aliquet feugiat vulputate.</bp-accordion-panel>
            <bp-accordion-panel accordion-panel-heading="Relationships" accordion-panel-heading-height="66">Etiam eget urna ullamcorper.</bp-accordion-panel>
        </bp-accordion>
    `;
    /* tslint:enable */

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

        it("all 3 panels have been added with default height", () => {

            //Arrange
            var layoutDefaultHeading = `
                <bp-accordion>
                    <bp-accordion-panel accordion-panel-heading="Discussions">Lorem ipsum dolor sit amet.</bp-accordion-panel>
                    <bp-accordion-panel accordion-panel-heading="Properties">Mauris aliquet feugiat vulputate.</bp-accordion-panel>
                    <bp-accordion-panel accordion-panel-heading="Relationships"Etiam eget urna ullamcorper.</bp-accordion-panel>
                </bp-accordion>
            `;
            directiveTest = new ComponentTest<BpAccordionCtrl>(layoutDefaultHeading, "bp-accordion");
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel2 = panels[1];
            var panel3 = panels[2];

            //Act
            accordion.redistributeHeight(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panel2.$element[0].style.height).toBe(panel3.$element[0].style.height, "2nd and 3rd panels don't match");
        });

        it("1st panel is open by default", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            //Act
            accordion.redistributeHeight(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panel1.$element[0].className).toContain("bp-accordion-panel-open", "1st panel is not open");
            expect(panel2.$element[0].className).not.toContain("bp-accordion-panel-open", "2nd panel is open");
            expect(panel3.$element[0].className).not.toContain("bp-accordion-panel-open", "3rd panel is open");
        });

        it("random id for panel 1 and 3, custom id for panel 2", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            //Act
            accordion.redistributeHeight(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panel1.accordionPanelId).toMatch(/bp-accordion-panel-\d{0,5}/, "1st panel doesn't have an id");
            expect(panel2.accordionPanelId).toBe("my-panel", "2nd panel's id is not the custom one");
            expect(panel3.accordionPanelId).toMatch(/bp-accordion-panel-\d{0,5}/, "3rd panel doesn't have an id");
            expect(panel1.accordionPanelId).not.toBe(panel3.accordionPanelId, "1st and 3rd panels have the same id");
        });

        it("default heading height for panel 1 and 2, custom height for panel 3", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            //Act
            accordion.redistributeHeight(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panel1.accordionPanelHeadingHeight).toBe("33", "1st panel's heading height is not the default one");
            expect(panel2.accordionPanelHeadingHeight).toBe("33", "2nd panel's heading height is not the default one");
            expect(panel3.accordionPanelHeadingHeight).toBe("66", "2nd panel's heading height is not the custom one");
        });

        it("open 2nd panel", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            var panel1Trigger = panel1.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel2Trigger = panel2.$element[0].querySelector("input.bp-accordion-panel-state");

            //Act
            //Jasmine doesn't seem to handle radio buttons properly, forcing the checked state
            panel1Trigger.removeAttribute("checked");
            panel2Trigger.setAttribute("checked", "checked");
            panel2Trigger.click(); //opening 2nd panel

            //Assert
            expect(panel1.$element[0].className).not.toContain("bp-accordion-panel-open", "1st panel is open");
            expect(panel2.$element[0].className).toContain("bp-accordion-panel-open", "2nd panel has not been opened");
            expect(panel3.$element[0].className).not.toContain("bp-accordion-panel-open", "3rd panel is open");
        });

        it("open 2nd panel (on top variant)", () => {

            //Arrange
            /* tslint:disable */
            var layoutOpenTop = `
                <bp-accordion accordion-heading-height="33" accordion-open-top>
                    <bp-accordion-panel accordion-panel-heading="Discussions">Lorem ipsum dolor sit amet.</bp-accordion-panel>
                    <bp-accordion-panel accordion-panel-heading="Properties" accordion-panel-id="my-panel">Mauris aliquet feugiat vulputate.</bp-accordion-panel>
                    <bp-accordion-panel accordion-panel-heading="Relationships" accordion-panel-heading-height="66">Etiam eget urna ullamcorper.</bp-accordion-panel>
                </bp-accordion>
            `;
            /* tslint:enable */
            directiveTest = new ComponentTest<BpAccordionCtrl>(layoutOpenTop, "bp-accordion");
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            var panel1Trigger = panel1.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel2Trigger = panel2.$element[0].querySelector("input.bp-accordion-panel-state");

            //Act
            //Jasmine doesn't seem to handle radio buttons properly, forcing the checked state
            panel1Trigger.removeAttribute("checked");
            panel2Trigger.setAttribute("checked", "checked");
            panel2Trigger.click(); //opening 2nd panel

            //Assert
            expect(panel1.$element[0].className).not.toContain("bp-accordion-panel-open", "1st panel is open");
            expect(panel2.$element[0].className).toContain("bp-accordion-panel-open", "2nd panel has not been opened");
            expect(panel3.$element[0].className).not.toContain("bp-accordion-panel-open", "3rd panel is open");
        });

        it("open 2nd panel and then try to close it", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            var panel1Trigger = panel1.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel2Trigger = panel2.$element[0].querySelector("input.bp-accordion-panel-state");

            //Act
            //Jasmine doesn't seem to handle radio buttons properly, forcing the checked state
            panel1Trigger.removeAttribute("checked");
            panel2Trigger.setAttribute("checked", "checked");
            panel2Trigger.click(); //opening 2nd panel
            panel2Trigger.click(); //trying to toggle the panel, no other panel is open therefore nothing should happen

            //Assert
            expect(panel1.$element[0].className).not.toContain("bp-accordion-panel-open", "1st panel is open");
            expect(panel2.$element[0].className).toContain("bp-accordion-panel-open", "2nd panel has not been opened");
            expect(panel3.$element[0].className).not.toContain("bp-accordion-panel-open", "3rd panel is open");
        });

        it("open 2nd panel, pin it, and then open the 3rd panel", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            var panel1Trigger = panel1.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel2Trigger = panel2.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel2Pin = panel2.$element[0].querySelector("input.bp-accordion-panel-pin");
            var panel3Trigger = panel3.$element[0].querySelector("input.bp-accordion-panel-state");

            //Act
            //Jasmine doesn't seem to handle radio buttons properly, forcing the checked state
            panel1Trigger.removeAttribute("checked");
            panel2Trigger.setAttribute("checked", "checked");
            panel2Trigger.click(); //opening 2nd panel
            panel2Pin.click(); //pinning 2nd panel
            panel2Trigger.removeAttribute("checked");
            panel3Trigger.setAttribute("checked", "checked");
            panel3Trigger.click(); //opening 3rd panel

            //Assert
            expect(panel1.$element[0].className).not.toContain("bp-accordion-panel-open", "1st panel is open");
            expect(panel2.$element[0].className).toContain("bp-accordion-panel-open", "2nd panel is closed");
            expect(panel3.$element[0].className).toContain("bp-accordion-panel-open", "3rd panel has not been opened");
        });

        it("open 2nd panel, pin it, open the 3rd panel, pin it, unpin 2nd panel", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            var panel1Trigger = panel1.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel2Trigger = panel2.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel2Pin = panel2.$element[0].querySelector("input.bp-accordion-panel-pin");
            var panel3Trigger = panel3.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel3Pin = panel3.$element[0].querySelector("input.bp-accordion-panel-pin");

            //Act
            //Jasmine doesn't seem to handle radio buttons properly, forcing the checked state
            panel1Trigger.removeAttribute("checked");
            panel2Trigger.setAttribute("checked", "checked");
            panel2Trigger.click(); //opening 2nd panel
            panel2Pin.click(); //pinning 2nd panel
            panel2Trigger.removeAttribute("checked");
            panel3Trigger.setAttribute("checked", "checked");
            panel3Trigger.click(); //opening 3rd panel
            panel3Pin.click(); //pinning 3rd panel
            panel2Pin.click(); //pinning 2nd panel

            //Assert
            expect(panel1.$element[0].className).not.toContain("bp-accordion-panel-open", "1st panel is open");
            expect(panel2.$element[0].className).not.toContain("bp-accordion-panel-open", "2nd panel has not been closed");
            expect(panel3.$element[0].className).toContain("bp-accordion-panel-open", "3rd panel has not been opened");
        });

        it("redistribute height: 1st panel open", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            //Act
            accordion.redistributeHeight(); //this is called on $timeout(0) in the component, explicitly calling it here

            //Assert
            expect(panel1.$element[0].style.height).toBe("calc(100% - 99px)", "1st panel's height is wrong");
            expect(panel2.$element[0].style.height).toBe("33px", "2nd panel's height is wrong");
            expect(panel3.$element[0].style.height).toBe("66px", "3rd panel's height is wrong");
        });

        it("redistribute height: panel 1 and 2 open", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            var panel1Trigger = panel1.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel1Pin = panel1.$element[0].querySelector("input.bp-accordion-panel-pin");
            var panel2Trigger = panel2.$element[0].querySelector("input.bp-accordion-panel-state");

            //Act
            //Jasmine doesn't seem to handle radio buttons properly, forcing the checked state
            panel1Pin.click(); //pinning 1st panel
            panel1Trigger.removeAttribute("checked");
            panel2Trigger.setAttribute("checked", "checked");
            panel2Trigger.click(); //opening 2nd panel

            //Assert
            expect(panel1.$element[0].style.height).toBe("calc(50% - 33px)", "1st panel's height is wrong");
            expect(panel2.$element[0].style.height).toBe("calc(50% - 33px)", "2nd panel's height is wrong");
            expect(panel3.$element[0].style.height).toBe("66px", "3rd panel's height is wrong");
        });

        it("redistribute height: panel 1 and 3 open", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            var panel1Trigger = panel1.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel1Pin = panel1.$element[0].querySelector("input.bp-accordion-panel-pin");
            var panel3Trigger = panel3.$element[0].querySelector("input.bp-accordion-panel-state");

            //Act
            //Jasmine doesn't seem to handle radio buttons properly, forcing the checked state
            panel1Pin.click(); //pinning 1st panel
            panel1Trigger.removeAttribute("checked");
            panel3Trigger.setAttribute("checked", "checked");
            panel3Trigger.click(); //opening 3rd panel

            //Assert
            expect(panel1.$element[0].style.height).toBe("calc(50% - 16.5px)", "1st panel's height is wrong");
            expect(panel2.$element[0].style.height).toBe("33px", "2nd panel's height is wrong");
            expect(panel3.$element[0].style.height).toBe("calc(50% - 16.5px)", "3rd panel's height is wrong");
        });

        it("redistribute height: all 3 panels open", () => {

            //Arrange
            var accordion: BpAccordionCtrl = directiveTest.createComponent({});
            var panels = accordion.getPanels();
            var panel1 = panels[0];
            var panel2 = panels[1];
            var panel3 = panels[2];

            var panel1Trigger = panel1.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel1Pin = panel1.$element[0].querySelector("input.bp-accordion-panel-pin");
            var panel2Trigger = panel2.$element[0].querySelector("input.bp-accordion-panel-state");
            var panel2Pin = panel2.$element[0].querySelector("input.bp-accordion-panel-pin");
            var panel3Trigger = panel3.$element[0].querySelector("input.bp-accordion-panel-state");

            //Act
            //Jasmine doesn't seem to handle radio buttons properly, forcing the checked state
            panel1Pin.click(); //pinning 1st panel
            panel1Trigger.removeAttribute("checked");
            panel2Trigger.setAttribute("checked", "checked");
            panel2Trigger.click(); //opening 2nd panel
            panel2Pin.click(); //pinning 2nd panel
            panel2Trigger.removeAttribute("checked");
            panel3Trigger.setAttribute("checked", "checked");
            panel3Trigger.click(); //opening 3rd panel

            //Assert
            expect(panel1.$element[0].style.height).toBe(panel2.$element[0].style.height, "1st and 2nd panels don't match");
            expect(panel2.$element[0].style.height).toBe(panel3.$element[0].style.height, "2nd and 3rd panels don't match");
        });
    });
});