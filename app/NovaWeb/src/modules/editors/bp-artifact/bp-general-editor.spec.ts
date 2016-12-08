import "angular";
import "angular-mocks";
import {ComponentTest} from "../../util/component.test";
import {BpGeneralArtifactEditorController} from "./bp-general-editor";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {ArtifactManagerMock} from "./../../managers/artifact-manager/artifact-manager.mock";
import {WindowManagerMock} from "./../../main/services/window-manager.mock";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {PropertyDescriptorBuilderMock} from "./../configuration/property-descriptor-builder.mock";

describe("Component BpGeneralEditorInfo", () => {
    let componentTest: ComponentTest<BpGeneralArtifactEditorController>;
    let template = `<bp-artifact-general-editor context="artifact"></bp-artifact-general-editor>`;
    let bindings: any;
    let ctrl: BpGeneralArtifactEditorController;


    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("windowManager", WindowManagerMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);

        // $provide.service("session", SessionSvcMock);
        // $provide.service("stateManager", StateManager);
        // $provide.service("projectRepository", ProjectRepositoryMock);
        // $provide.service("projectManager", ProjectManager);
        // $provide.service("selectionManager", SelectionManager);
        // $provide.service("windowResize", WindowResize);

    }));

    beforeEach(() => {
        componentTest = new ComponentTest<BpGeneralArtifactEditorController>(template, "bpGeneralEditor");
    });

    afterEach(() => {
        ctrl = null;
    });

     it("should be visible by default", () => {
        // Arrange
        bindings = {
            artifact: {
                id: 1
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
