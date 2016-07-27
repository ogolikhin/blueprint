import "../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../util/component.test";
import { BPAttachmentsPanelController } from "./bp-attachments-panel";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ArtifactAttachmentsMock } from "./artifact-attachments.mock";
import { ProjectRepositoryMock } from "../../../main/services/project-repository.mock";
import { ProjectManager, Models } from "../../../main/services/project-manager";

describe("Component BP Attachments Panel", () => {

    let componentTest: ComponentTest<BPAttachmentsPanelController>;
    let template = `<bp-attachments-panel></bp-attachments-panel>`;
    let vm: BPAttachmentsPanelController;
    let bpAccordionPanelController = {
        isOpenObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
    }));

    beforeEach(inject((projectManager: ProjectManager) => {
        projectManager.initialize();
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
        inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {

        //Arrange
       projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
       $rootScope.$digest();

       //Act
       let artifact = projectManager.getArtifact(22);

       //Assert
       expect(artifact).toBeDefined();
       expect(vm.artifactAttachmentsList).toBeDefined();
       expect(vm.artifactAttachmentsList.attachments.length).toBe(7);
       expect(vm.artifactAttachmentsList.documentReferences.length).toBe(3);
       expect(componentTest.element.find("bp-artifact-attachment-item").length).toBe(7);
       expect(componentTest.element.find("bp-artifact-document-item").length).toBe(3);
       expect(componentTest.element.find(".empty-state").length).toBe(0);
    }));
});
