import "../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../util/component.test";
import { BPAttachmentsPanelController } from "./bp-attachments-panel";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ArtifactAttachmentsMock } from "./artifact-attachments.mock";
import { Models } from "../../../main/services/project-manager";
import { SelectionManager, SelectionSource } from "../../../main/services/selection-manager";

describe("Component BP Attachments Panel", () => {

    let componentTest: ComponentTest<BPAttachmentsPanelController>;
    let template = `<bp-attachments-panel></bp-attachments-panel>`;
    let vm: BPAttachmentsPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManager);
    }));

    beforeEach(inject(() => {
        componentTest = new ComponentTest<BPAttachmentsPanelController>(template, "bp-attachments-panel");
        vm = componentTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
    }));
    
    afterEach( () => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(componentTest.element.find(".empty-state").length).toBe(1);
    });

    it("should load data and display it for a selected artifact", 
        inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager) => {
            
            //Arrange
            const artifact = { id: 22, name: "Artifact", prefix: "PRO" } as Models.IArtifact;
            
            //Act
            selectionManager.selection = { artifact: artifact, source:  SelectionSource.Explorer };
            $rootScope.$digest();
            const selectedArtifact = selectionManager.selection.artifact;

            //Assert
            expect(selectedArtifact).toBeDefined();
            expect(vm.artifactAttachmentsList).toBeDefined();
            expect(vm.artifactAttachmentsList.attachments.length).toBe(7);
            expect(vm.artifactAttachmentsList.documentReferences.length).toBe(3);
            expect(componentTest.element.find("bp-artifact-attachment-item").length).toBe(7);
            expect(componentTest.element.find("bp-artifact-document-item").length).toBe(3);
            expect(componentTest.element.find(".empty-state").length).toBe(0);
        }));

    it("should not load data for artifact without Prefix",
        inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager) => {

            //Arrange
            const artifact = { id: 22, name: "Artifact" } as Models.IArtifact;

            //Act
            selectionManager.selection = { artifact: artifact, source: SelectionSource.Explorer };
            $rootScope.$digest();
            const selectedArtifact = selectionManager.selection.artifact;

            //Assert
            expect(selectedArtifact).toBeDefined();
            expect(vm.artifactAttachmentsList).toBe(null);
        }));
});
