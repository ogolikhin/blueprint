import "../../../";
import "angular";
import "angular-mocks";
import { ComponentTest } from "../../../../util/component.test";
import { BPArtifactDocumentItemController} from "./bp-artifact-document-item";
import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";
import { ArtifactAttachmentsService } from "../artifact-attachments.svc";
import { IMessageService } from "../../../../shell";


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
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
    }));

    beforeEach(inject(() => {
        let bindings: any = { 
            document: {
                artifactName: "doc",
                artifactId: 357,
                userId: 1,
                userName: "admin",
                referencedDate: "2016-06-27T21:27:57.67Z"
            }
        };

        directiveTest = new ComponentTest<BPArtifactDocumentItemController>(template, "bp-artifact-document-item");
        vm = directiveTest.createComponent(bindings);
    }));

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".author").length).toBe(1);
        expect(directiveTest.element.find(".button-bar").length).toBe(1);
        expect(directiveTest.element.find("h6").length).toBe(1);
    });

    it("should try to download a document which has an attachment", 
        inject((
            $httpBackend: ng.IHttpBackendService, 
            $window: ng.IWindowService) => {
        
        // Arrange
        spyOn($window, "open").and.callFake(function() {
            return true;
        });

        $httpBackend.expectGET(`/svc/artifactstore/artifacts/357/attachment?addDrafts=true`)
            .respond(200, {
                artifactId: 357,
                subartifactId: null,
                attachments: [
                    {
                        userId: 1,
                        userName: "admin",
                        fileName: "acc-wizard.d.ts",
                        attachmentId: 1102,
                        uploadedDate: "2016-06-27T21:26:24.24Z"
                    }
                ],
                documentReferences: []
            });

        // Act
        vm.downloadItem();
        $httpBackend.flush();
        
        //Assert
        expect($window.open).toHaveBeenCalled();
        expect($window.open).toHaveBeenCalledWith("/svc/components/RapidReview/artifacts/357/files/1102?includeDraft=true", "_blank");
    }));

    it("should try to download a document which has no attachment", 
        inject((
            $httpBackend: ng.IHttpBackendService, 
            $window: ng.IWindowService,
            messageService: IMessageService) => {
        
        // Arrange
        spyOn($window, "open").and.callFake(() => true);
        spyOn(messageService, "addError").and.callFake(() => true);

        $httpBackend.expectGET(`/svc/artifactstore/artifacts/357/attachment?addDrafts=true`)
            .respond(200, {
                artifactId: 357,
                subartifactId: null,
                attachments: [],
                documentReferences: []
            });

        // Act
        vm.downloadItem();
        $httpBackend.flush();
        
        //Assert
        expect($window.open).not.toHaveBeenCalled();
        expect(messageService.addError).toHaveBeenCalled();
    }));

    it("should try to delete an item", 
        inject(($rootScope: ng.IRootScopeService, $window: ng.IWindowService) => {
        
        // Arrange
        spyOn($window, "alert").and.callFake(() => true);

        // Act
        vm.deleteItem();
        
        //Assert
        expect($window.alert).toHaveBeenCalled();
    }));
});