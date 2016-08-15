import "angular";
import "angular-mocks";
import "./";
import { MessageServiceMock } from "../../core/messages/message.mock";
import { LocalizationServiceMock} from "../../core/localization/localization.mock";
import { ComponentTest } from "../../util/component.test";
import { BpGeneralEditorController } from "./bp-general-editor";
import { StateManager } from "../../core/services/state-manager";
import { WindowResize } from "../../core/services/window-resize";
import { WindowManager } from "../../main/services/window-manager";
import { ProjectRepositoryMock } from "../../main/services/project-repository.mock";
import { ProjectManager } from "../../main/services/project-manager";
import { SelectionManager } from "../../main/services/selection-manager";




describe("Component BpGeneralEditorInfo", () => {

    beforeEach(angular.mock.module("bp.editors.details"));

    let componentTest: ComponentTest<BpGeneralEditorController>;
    let template = `<bp-general-editor context="artifact"></bp-general-editor>`;
    let bindings: any;
    let ctrl: BpGeneralEditorController;


    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        
        $provide.service("messageService", MessageServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("stateManager", StateManager);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("windowResize", WindowResize);
        $provide.service("windowManager", WindowManager);

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