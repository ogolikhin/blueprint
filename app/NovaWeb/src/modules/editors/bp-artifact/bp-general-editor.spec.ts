import "angular";
import "angular-mocks";
import "./";
import { IStateManager } from "./";
import { MessageServiceMock } from "../../shell/messages/message.mock";
import { ComponentTest } from "../../util/component.test";
import { BpGeneralEditorController} from "./bp-general-editor";

export class StateManagerMock implements IStateManager {
}


describe("Component BpGeneralEditorInfo", () => {

    beforeEach(angular.mock.module("bp.editors.details"));

    let componentTest: ComponentTest<BpGeneralEditorController>;
    let template = `<bp-general-editor context="artifact"></bp-general-editor>`;
    let bindings: any;
    let ctrl: BpGeneralEditorController;


    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("stateManager", StateManagerMock);
        
    }));

    beforeEach(() => {
        componentTest = new ComponentTest<BpGeneralEditorController>(template, "bpGeneralEditor");
    });

    afterEach(() => {
        ctrl = null;
    });

    it("should be visible by default", () => {
        // Arrange
        bindings = {
            artifact: {
                id: 1,
            }
        };

        ctrl = componentTest.createComponent(bindings);
        //Assert
        expect(componentTest.element.find(".artifact-overview").length).toBe(0);
        expect(componentTest.element.find(".readonly-indicator").length).toBe(0);
        expect(componentTest.element.find(".lock-indicator").length).toBe(0);
        expect(componentTest.element.find(".dirty-indicator").length).toBe(0);
    });

});