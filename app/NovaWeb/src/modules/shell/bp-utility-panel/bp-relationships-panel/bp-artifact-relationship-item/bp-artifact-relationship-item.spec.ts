import "../../../";
import "angular-mocks";
import "angular-sanitize";
import {ComponentTest} from "../../../../util/component.test";
import {BPArtifactRelationshipItemController} from "./bp-artifact-relationship-item";
import {ProcessServiceMock} from "../../../../editors/bp-process/services/process.svc.mock";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {SelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ArtifactRelationshipsMock} from "../../../../managers/artifact-manager/relationships/relationships.svc.mock";
import {NavigationServiceMock} from "../../../../core/navigation/navigation.svc.mock";
import {Relationships} from "../../../../main";
import {Helper} from "../../../../shared";

import {
    ArtifactManager,
    StatefulArtifactFactory,
    MetaDataService,
    ArtifactService,
    ArtifactAttachmentsService
} from "../../../../managers/artifact-manager";
import {HttpStatusCode} from "../../../../core/http/http-status-code";

describe("Component BPDiscussionReplyItem", () => {

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("artifactService", ArtifactService);
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("processService", ProcessServiceMock);
        $provide.service("navigationService", NavigationServiceMock);
    }));

    let directiveTest: ComponentTest<BPArtifactRelationshipItemController>;
    let vm: BPArtifactRelationshipItemController;


    beforeEach(inject(() => {
        let template = `<bp-artifact-relationship-item artifact="::artifact"></bp-artifact-relationship-item>`;
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
                .respond(HttpStatusCode.Success, {
                    "artifactId": "1",
                    "description": "desc",
                    "pathToProject": [{"itemId": 1, "itemName": "Item1", "parentId": 0}]
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
        let result = vm.limitChars("<html><body>&#x200b;<div><span>ABC</span></div></body></html>");
        expect(result.length).toBe(4); //zero width space included
    });

    it("limitChars, no text", () => {
        //Assert
        let result = vm.limitChars("");
        expect(result.length).toBe(0);
    });

    it("limitChars, long text 100 characters", () => {
        //Arrange
        const expectedResult = "UiKXLAu2uZQzdnrqH1SlqDXyQ74hHy3kxVtSQowhCxf99llObZxr3Rj0eDX09aCB8NR0YJhMuqNbGczDTimrpGtU48fBeduOhvS" + Helper.ELLIPSIS_SYMBOL;

        //Act
        let result = vm.limitChars("UiKXLAu2uZQzdnrqH1SlqDXyQ74hHy3kxVtSQowhCxf99llObZxr3Rj0eDX09aCB8NR0YJhMuqNbGczDTimrpGtU48fBeduOhvS1n98dPUrCHh");

        //Assert
        expect(result).toBe(expectedResult);
    });

    it("inArray, contains an artifact", () => {
        //Arrange
        vm.artifact = <Relationships.IRelationship>{
            "artifactId": 1,
            "itemId": 1
        };
        let array = [];
        array.push({"itemId": 1});
        array.push({"itemId": 2});

        //Act
        let result = vm.inArray(array);

        //Assert
        expect(result.found).toBe(true);
        expect(result.index).toBe(0);
    });
    it("inArray, doesn't contain an artifact", () => {
        //Arrange
        vm.artifact = <Relationships.IRelationship>{
            "artifactId": 1,
            "itemId": 3
        };
        let array = [];
        array.push({"itemId": 1});
        array.push({"itemId": 2});

        //Act
        let result = vm.inArray(array);

        //Assert
        expect(result.found).toBe(false);
        expect(result.index).toBe(-1);
    });

    it("inArray, null array", () => {
        //Arrange
        vm.artifact = <Relationships.IRelationship>{
            "artifactId": 1,
            "itemId": 3
        };
        let array = null;

        //Act
        let result = vm.inArray(array);

        //Assert
        expect(result.found).toBe(false);
        expect(result.index).toBe(-1);
    });

    it("inArray, empty array", () => {
        //Arrange
        vm.artifact = <Relationships.IRelationship>{
            "artifactId": 1,
            "itemId": 3
        };
        let array = [];

        //Act
        let result = vm.inArray(array);

        //Assert
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
            //Arrange
            vm.artifact.isSelected = false;

            //Act
            vm.selectTrace();

            //Assert
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.artifact.isSelected).toBe(true);

        });

        it("select, select artifact that already in the array", () => {
            //Arrange
            vm.artifact.isSelected = false;
            vm.selectedTraces.push(vm.artifact);

            //Act
            vm.selectTrace();

            //Assert
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.artifact.isSelected).toBe(true);

        });

        it("select, unselect artifact", () => {
            //Arrange
            vm.artifact.isSelected = true;
            vm.selectedTraces.push(vm.artifact);

            //Act
            vm.selectTrace();

            //Assert
            expect(vm.selectedTraces.length).toBe(0);
            expect(vm.artifact.isSelected).toBe(false);

        });

        it("select, unselect artifact that is not in the array", () => {
            //Arrange
            let artifact = <Relationships.IRelationship>{
                "artifactId": 1,
                "itemId": 4
            };
            vm.artifact.isSelected = true;

            //Act
            vm.selectedTraces.push(artifact);
            vm.selectTrace();

            //Assert
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.artifact.isSelected).toBe(false);

        });
    });
});
