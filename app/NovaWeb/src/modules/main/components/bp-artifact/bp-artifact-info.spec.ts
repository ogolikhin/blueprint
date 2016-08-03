import "../../";
import "../../../shell";
import "angular";
import "angular-mocks";
import "angular-sanitize";

import { ComponentTest } from "../../../util/component.test";
import { BpArtifactInfoController} from "./bp-artifact-info";



describe("Component BpArtifactInfo", () => {

    let componentTest: ComponentTest<BpArtifactInfoController>;
    let template = `<bp-artifact-info context="context"></bp-artifact-info>`;
    let vm: BpArtifactInfoController;

    beforeEach(angular.mock.module("app.shell"));
    beforeEach(angular.mock.module("app.main"));

    beforeEach(() => {
        componentTest = new ComponentTest<BpArtifactInfoController>(template, "bp-artifact-info");
        vm = componentTest.createComponent({});
        
    });

    afterEach(() => {
        vm = null;
    });

    it("the context is not set, default value ", () => {
        
        //Assert
        expect(componentTest.element.find(".icon").length).toBe(1);
        expect(componentTest.element.find(".type-id").length).toBe(1);
        expect(componentTest.element.find(".readonly-indicator").length).toBe(0);
        expect(componentTest.element.find(".lock-indicator").length).toBe(0);
        expect(componentTest.element.find(".dirty-indicator").length).toBe(0);
        expect(vm.artifactType).toBeNull();
        expect(vm.artifactName).toBeNull();
        expect(vm.artifactClass).toBeNull();
        expect(vm.artifactTypeDescription).toBeNull();

    });

    it("the context is set (no type). see context value", () => {
        vm = componentTest.createComponent({
            context: {
                artifact: {
                    id: 1,
                    name: "Simple",
                    predefinedType: 4101,
                    prefix: "TR_"
                }
            }
        });
        
        //Assert
        expect(vm.artifactType).toBe("TextualRequirement");
        expect(vm.artifactClass).toBe("icon-textual-requirement");
        expect(vm.artifactTypeDescription).toBe("TextualRequirement - TR_1");

    });

    it("the context is set. see context value", () => {
        vm = componentTest.createComponent({
            context: {
                artifact: {
                    id: 1,
                    name: "Simple",
                    predefinedType: 4101,
                    prefix: "TR_"
                },
                type : {
                    id: 4444,
                    name: "Textual Requirement",
                    predefinedType: 4101,
                }
            } 
        });
        
        //Assert
        expect(vm.artifactType).toBe("Textual Requirement");
        expect(vm.artifactClass).toBe("icon-textual-requirement");
        expect(vm.artifactTypeDescription).toBe("Textual Requirement - TR_1");
        
    });

});