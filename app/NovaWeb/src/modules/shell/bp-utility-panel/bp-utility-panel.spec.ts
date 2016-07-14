// import "../../main";
// import "../../shell";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../util/component.test";
import { BPUtilityPanelController} from "./bp-utility-panel";
import { LocalizationServiceMock } from "../../core/localization.mock";
import { ArtifactHistoryMock } from "./bp-history-panel/artifact-history.mock";
import { ArtifactRelationshipsMock } from "./bp-relationships-panel/artifact-relationships.mock";
import { ArtifactAttachmentsMock } from "./bp-attachments-panel/artifact-attachments.mock";
import { ProjectRepositoryMock } from "../../main/services/project-repository.mock";
import { ProjectManager, Models } from "../../main/services/project-manager";

xdescribe("Component BPUtilityPanel", () => {

    let directiveTest: ComponentTest<BPUtilityPanelController>;
    let template = `<bp-utility-panel></bp-utility-panel>`;
    let vm: BPUtilityPanelController;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactHistory", ArtifactHistoryMock);
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
    }));

    beforeEach(inject((projectManager: ProjectManager) => {
        projectManager.initialize();
        directiveTest = new ComponentTest<BPUtilityPanelController>(template, "bp-utility-panel");
        vm = directiveTest.createComponent({});
    }));
    
    afterEach( () => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".utility-panel-title").length).toBe(1);
        expect(directiveTest.element.find("bp-history-panel").length).toBe(1);
        expect(directiveTest.element.find("bp-discussion-panel").length).toBe(1);
        expect(directiveTest.element.find("bp-attachments-panel").length).toBe(1);
    });

    it("should load data for a selected artifact", 
        inject(($rootScope: ng.IRootScopeService, projectManager: ProjectManager) => {

        // Arrange
        projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
        $rootScope.$digest();

        // Act
        let artifact = projectManager.getArtifact(22);

        // Assert
        expect(artifact).toBeDefined();
        expect(artifact.id).toBe(22);
        expect(vm.currentArtifact).toBe("2: Project 2");
    }));
});
