import "../../../";
import "angular";
import "angular-mocks";
import { ComponentTest } from "../../../../util/component.test";
import { BPArtifactAttachmentItemController} from "./bp-artifact-attachment-item";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";
import { ProjectRepositoryMock } from "../../../../main/services/project-repository.mock";
import { ProjectManager, Models } from "../../../../main/services/project-manager";

describe("Component BP Artifact Attachment Item", () => {


    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
    }));

    let componentTest: ComponentTest<BPArtifactAttachmentItemController>;
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
        componentTest = new ComponentTest<BPArtifactAttachmentItemController>(template, "bp-artifact-attachment-item");
        vm = componentTest.createComponent(bindings);
    }));

    it("should be visible by default", () => {
        //Assert
        expect(componentTest.element.find(".author").length).toBe(1);
        expect(componentTest.element.find(".button-bar").length).toBe(1);
        expect(componentTest.element.find("h6").length).toBe(1);
        expect(componentTest.element.find(".ext-image").length).toBe(1);
    });

    it("should try to download an attachment", 
        inject(($rootScope: ng.IRootScopeService, $window: ng.IWindowService, projectManager: ProjectManager) => {
        
        // Arrange
        projectManager.loadProject({ id: 2, name: "Project 2" } as Models.IProject);
        $rootScope.$digest();
        projectManager.getArtifact(22);

        spyOn($window, "open").and.callFake(() => true);

        // Act
        vm.downloadItem();
        
        //Assert
        expect($window.open).toHaveBeenCalled();
        expect($window.open).toHaveBeenCalledWith("/svc/components/RapidReview/artifacts/2/files/1093?includeDraft=true", "_blank");
    }));

    it("should try to delete an attachment", 
        inject(($window: ng.IWindowService) => {
        
        // Arrange
        spyOn($window, "alert").and.callFake(() => true);

        // Act
        vm.deleteItem();
        
        //Assert
        expect($window.alert).toHaveBeenCalled();
    }));
});
