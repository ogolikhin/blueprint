import "../../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPArtifactDocumentItemController} from "./bp-artifact-document-item";
import { LocalizationServiceMock } from "../../../../core/localization.mock";
import { ArtifactAttachmentsMock } from "../artifact-attachments.mock";
import { ProjectRepositoryMock } from "../../../../main/services/project-repository.mock";
import { ProjectManager, Models } from "../../../../main/services/project-manager";

describe("Component BP Artifact Document Item", () => {
    let directiveTest: ComponentTest<BPArtifactDocumentItemController>;
    let template = `
        <bp-artifact-document-item 
            doc-ref-info="document">
        </bp-artifact-document-item>
    `;
    let vm: BPArtifactDocumentItemController;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
    }));

    beforeEach(inject((projectManager: ProjectManager) => {
        let bindings: any = { 
            document: {
                artifactName: "document with no attachment",
                artifactId: 357,
                userId: 1,
                userName: "admin",
                referencedDate: "2016-06-27T21:27:57.67Z"
            }
        };

        projectManager.initialize();
        directiveTest = new ComponentTest<BPArtifactDocumentItemController>(template, "bp-artifact-document-item");
        vm = directiveTest.createComponent(bindings);
    }));

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".author").length).toBe(1);
        expect(directiveTest.element.find(".button-bar").length).toBe(1);
        expect(directiveTest.element.find("h6").length).toBe(1);
    });

    it("should try to delete an item", 
        inject(($rootScope: ng.IRootScopeService, $window: ng.IWindowService, projectManager: ProjectManager) => {
        
        // Arrange
        projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
        $rootScope.$digest();
        projectManager.getArtifact(22);

        spyOn($window, "alert").and.callFake(() => {
            return true;
        });

        // Act
        vm.deleteItem();
        
        //Assert
        expect($window.alert).toHaveBeenCalled();
    }));
});