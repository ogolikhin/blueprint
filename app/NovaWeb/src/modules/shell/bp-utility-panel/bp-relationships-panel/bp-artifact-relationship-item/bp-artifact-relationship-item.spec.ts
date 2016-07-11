import "../../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../../util/component.test";
import { BPArtifactRelationshipItemController} from "./bp-artifact-relationship-item";
import { ProjectRepositoryMock } from "../../../../main/services/project-repository.mock";
import { LocalizationServiceMock } from "../../../../core/localization.mock";
import { ProjectManager, Models } from "../../../../main/services/project-manager";
import { ArtifactRelationshipsMock } from "../artifact-relationships.mock";
import {Relationships} from "../../../../main";

describe("Component BPDiscussionReplyItem", () => {

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("projectRepository", ProjectRepositoryMock);
        $provide.service("projectManager", ProjectManager);
    }));

    let directiveTest: ComponentTest<BPArtifactRelationshipItemController>;
    let template = `<bp-artifact-relationship-item artifact="::artifact">
        </bp-artifact-relationship-item>`;
    let vm: BPArtifactRelationshipItemController;


    beforeEach(inject(() => {
        let bindings: any = {
            artifact: {}
        };
       
        directiveTest = new ComponentTest<BPArtifactRelationshipItemController>(template, "bp-artifact-relationship-item");
        vm = directiveTest.createComponent({});       
    }));

    afterEach(() => {
        vm = null;
    });

    //it("should be visible by default", () => {
    //    //Assert
    //    expect(directiveTest.element.find(".details").length).toBe(1);   
    //});

    it("expanded view",
        inject(($httpBackend: ng.IHttpBackendService) => {

            // Arrange        
            $httpBackend.expectGET(`/svc/artifactstore/artifacts/1/relationshipdetails`)
                .respond(200, {
                    "artifactId": "1",
                    "description": "desc",
                    "pathToProject": [{ "itemId": 1, "itemName": "Item1", "parentId": 0 }]
                });

            vm.artifact = <Relationships.IRelationship>{
                "artifactId": 1
            };

            vm.expanded = false;

            // Act
            vm.expand();        
            $httpBackend.flush();

            //Assert
            expect(directiveTest.element.find(".wrappable-breadcrumbs").length).toBe(1); 
        }));

    //it("limitChars, short text", () => {
    //    //Assert
    //    var result = vm.limitChars('<html><body>&#x200b;<div><span>ABC</span></div></body></html>');
    //   expect(result.length).toBe(4); //zero width space included
    //});

    //it("limitChars, no text", () => {
    //    //Assert
    //    var result = vm.limitChars('');
    //    expect(result.length).toBe(0);
    //});

    //it("limitChars, long text 110 characters", () => {
    //    //Assert
    //    var result = vm.limitChars('UiKXLAu2uZQzdnrqH1SlqDXyQ74hHy3kxVtSQowhCxf99llObZxr3Rj0eDX09aCB8NR0YJhMuqNbGczDTimrpGtU48fBeduOhvS1n98dPUrCHh');
    //    expect(result.length).toBe(103);
    //});
});