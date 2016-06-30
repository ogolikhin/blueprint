import "../../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPArtifactAttachmentItemController} from "./bp-artifact-attachment-item";
import { LocalizationServiceMock } from "../../../../core/localization.mock";
import { ProjectRepositoryMock } from "../../../../main/services/project-repository.mock";
import { ProjectManager, Models } from "../../../../main/services/project-manager";

describe("Component BP Artifact Attachment Item", () => {


    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
    }));

    let directiveTest: ComponentTest<BPArtifactAttachmentItemController>;
    let template = `
        <bp-artifact-attachment-item 
            attachment-info="attachment">
        </bp-artifact-attachment-item>
    `;
    let vm: BPArtifactAttachmentItemController;

    beforeEach(inject((projectManager: ProjectManager) => {
        let bindings: any = { 
            attachment: {
                userId: 1,
                userName: "admin",
                fileName: "test.png",
                attachmentId: 1093,
                uploadedDate: "2016-06-23T14:54:27.273Z"
            }
        };

        projectManager.initialize();
        directiveTest = new ComponentTest<BPArtifactAttachmentItemController>(template, "bp-artifact-attachment-item");
        vm = directiveTest.createComponent(bindings);
    }));

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".author").length).toBe(1);
        expect(directiveTest.element.find(".button-bar").length).toBe(1);
        expect(directiveTest.element.find("h6").length).toBe(1);
    });

    it("should try to download an attachment", 
        inject(($rootScope: ng.IRootScopeService, $window: ng.IWindowService, projectManager: ProjectManager) => {
        
        // Arrange
        projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
        $rootScope.$digest();
        projectManager.getArtifact(22);

        spyOn($window, "open").and.callFake(() => {
            return "`/svc/components/RapidReview/artifacts/22/files/1093?includeDrafts=true";
        });

        // Act
        vm.downloadItem();
        
        //Assert
        expect($window.open).toHaveBeenCalled();
    }));

    it("should try to delete an attachment", 
        inject(($rootScope: ng.IRootScopeService, $window: ng.IWindowService, projectManager: ProjectManager) => {
        
        // Arrange
        projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
        $rootScope.$digest();
        projectManager.getArtifact(22);

        spyOn($window, "alert").and.callFake(function() {
            return true;
        });

        // Act
        vm.deleteItem();
        
        //Assert
        expect($window.alert).toHaveBeenCalled();
    }));
});
