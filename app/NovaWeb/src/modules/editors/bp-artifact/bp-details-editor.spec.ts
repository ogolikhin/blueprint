import "angular";
import "angular-mocks";
import "../";
import {ComponentTest} from "../../util/component.test";
import {BpGeneralArtifactEditorController} from "./bp-general-editor";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {ArtifactManagerMock} from "./../../managers/artifact-manager/artifact-manager.mock";
import {WindowManagerMock} from "./../../main/services/window-manager.mock";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {PropertyDescriptorBuilderMock} from "./../configuration/property-descriptor-builder.mock";
import {IArtifactManager} from "./../../managers/artifact-manager";
import {ISelectionManager} from "./../../managers/selection-manager/selection-manager";
import {IStatefulArtifact} from "./../../managers/artifact-manager/artifact/artifact";
import {ValidationServiceMock} from "./../../managers/artifact-manager/validation/validation.mock";
import {BpArtifactDetailsEditorController} from "./bp-details-editor";
import {Models} from "../../main";

describe("Component BpDetailsEditor", () => {
    let componentTest: ComponentTest<BpArtifactDetailsEditorController>;
    let template = `<bp-artifact-details-editor></bp-artifact-details-editor>`;
    let ctrl: BpArtifactDetailsEditorController;

    beforeEach(angular.mock.module("bp.editors.details"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("windowManager", WindowManagerMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
        $provide.service("validationService", ValidationServiceMock);

        // $provide.service("session", SessionSvcMock);
        // $provide.service("stateManager", StateManager);
        // $provide.service("projectRepository", ProjectRepositoryMock);
        // $provide.service("projectManager", ProjectManager);
        // $provide.service("selectionManager", SelectionManager);
        // $provide.service("windowResize", WindowResize);
    }));

    beforeEach(inject((artifactManager: IArtifactManager) => {
        const subject = new Rx.BehaviorSubject<IStatefulArtifact>(this);
        artifactManager.selection = {
            getArtifact: () => {
                return {
                    getObservable: () => subject.asObservable(),
                    artifactState: {readonly: false} as any
                };
            }
        } as ISelectionManager;
    }));

    beforeEach(() => {
        componentTest = new ComponentTest<BpArtifactDetailsEditorController>(template, "bp-artifact-details-editor");
    });

    afterEach(() => {
        ctrl = null;
    });

     it("should be visible by default", () => {
        // Arrange
        ctrl = componentTest.createComponent({});

        //Assert
        expect(componentTest.element.find(".artifact-overview").length).toBe(1);
        expect(componentTest.element.find(".readonly-indicator").length).toBe(0);
        expect(componentTest.element.find(".lock-indicator").length).toBe(0);
        expect(componentTest.element.find(".dirty-indicator").length).toBe(0);
     });

    it("should properly update with a rich text field", () => {
        // Arrange
        ctrl = componentTest.createComponent({});
    });

});
