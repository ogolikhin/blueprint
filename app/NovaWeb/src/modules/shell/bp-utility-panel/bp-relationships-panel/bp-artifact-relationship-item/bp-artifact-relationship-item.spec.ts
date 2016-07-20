﻿import "../../../";
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

    it("should be visible by default", () => {
        //Assert
        expect(directiveTest.element.find(".details").length).toBeGreaterThan(0);   
    });

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
            vm.expand({});        
            $httpBackend.flush();

            //Assert
            expect(directiveTest.element.find(".wrappable-breadcrumbs").length).toBe(1); 
        }));

    it("limitChars, short text", () => {
        //Assert
        var result = vm.limitChars('<html><body>&#x200b;<div><span>ABC</span></div></body></html>');
       expect(result.length).toBe(4); //zero width space included
    });

    it("limitChars, no text", () => {
        //Assert
        var result = vm.limitChars('');
        expect(result.length).toBe(0);
    });

    it("limitChars, long text 110 characters", () => {
        //Assert
        var result = vm.limitChars('UiKXLAu2uZQzdnrqH1SlqDXyQ74hHy3kxVtSQowhCxf99llObZxr3Rj0eDX09aCB8NR0YJhMuqNbGczDTimrpGtU48fBeduOhvS1n98dPUrCHh');
        expect(result.length).toBe(103);
    });

    it("inArray, contains an artifact", () => {
        //Arange
        vm.artifact = <Relationships.IRelationship>{
            "artifactId": 1,
            "itemId": 1
        };
        let array = [];
        array.push({"itemId": 1});
        array.push({ "itemId": 2 });

        //Assert
        var result = vm.inArray(array);
        expect(result.found).toBe(true);
        expect(result.index).toBe(0);
    });
    it("inArray, doesn't contain an artifact", () => {
        //Arange
        vm.artifact = <Relationships.IRelationship>{
            "artifactId": 1,
            "itemId": 3
        };
        let array = [];
        array.push({ "itemId": 1 });
        array.push({ "itemId": 2 });

        //Assert
        var result = vm.inArray(array);
        expect(result.found).toBe(false);
        expect(result.index).toBe(-1);
    });

    it("inArray, null array", () => {
        //Arange
        vm.artifact = <Relationships.IRelationship>{
            "artifactId": 1,
            "itemId": 3
        };
        let array = null;
     
        //Assert
        var result = vm.inArray(array);
        expect(result.found).toBe(false);
        expect(result.index).toBe(-1);
    });

    it("inArray, empty array", () => {
        //Arange
        vm.artifact = <Relationships.IRelationship>{
            "artifactId": 1,
            "itemId": 3
        };
        let array = [];

        //Assert
        var result = vm.inArray(array);
        expect(result.found).toBe(false);
        expect(result.index).toBe(-1);
    });

    describe("select artifact", () => {

        beforeEach(() => {
            vm.selectable = true;
            vm.artifact = <Relationships.IRelationship>{
                "artifactId": 1,
                "itemId": 3
            };
            vm.selectedTraces = [];

        });

        it("select, select artifact", () => {
            //Arange
            vm.artifact.isSelected = false;        

            //Assert
            var result = vm.select();
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.artifact.isSelected).toBe(true);

        });

        it("select, select artifact that already in the array", () => {
            //Arange
            vm.artifact.isSelected = false;          
            vm.selectedTraces.push(vm.artifact);

            //Assert
            var result = vm.select();
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.artifact.isSelected).toBe(true);

        });

        it("select, selectable false", () => {
            //Arange
            vm.selectable = false;          
            vm.artifact.isSelected = false;           
            vm.selectedTraces.push(vm.artifact);

            //Assert
            var result = vm.select();
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.artifact.isSelected).toBe(false);

        });


        it("select, unselect artifact", () => {
            //Arange
            vm.artifact.isSelected = true;            
            vm.selectedTraces.push(vm.artifact);

            //Assert
            var result = vm.select();
            expect(vm.selectedTraces.length).toBe(0);
            expect(vm.artifact.isSelected).toBe(false);

        });

        it("select, unselect artifact that is not in the array", () => {
            //Arange          
            let artifact = <Relationships.IRelationship>{
                "artifactId": 1,
                "itemId": 4
            };
            vm.artifact.isSelected = true;        
            vm.selectedTraces.push(artifact);

            //Assert
            var result = vm.select();
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.artifact.isSelected).toBe(false);

        });


    });

   
});