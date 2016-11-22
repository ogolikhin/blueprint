﻿import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "../../../";
import {ComponentTest} from "../../../../util/component.test";
import {BPArtifactRelationshipItemController} from "./bp-artifact-relationship-item";
import {ProcessServiceMock} from "../../../../editors/bp-process/services/process.svc.mock";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {SelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {DialogServiceMock} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {ArtifactRelationshipsMock} from "../../../../managers/artifact-manager/relationships/relationships.svc.mock";
import {NavigationServiceMock} from "../../../../core/navigation/navigation.svc.mock";
import {Helper} from "../../../../shared";
import {HttpStatusCode} from "../../../../core/http/http-status-code";
import {ValidationServiceMock} from "../../../../managers/artifact-manager/validation/validation.mock";
import {ArtifactManager} from "../../../../managers/artifact-manager/artifact-manager";
import {MetaDataService} from "../../../../managers/artifact-manager/metadata/metadata.svc";
import {StatefulArtifactFactory} from "../../../../managers/artifact-manager/artifact/artifact.factory";
import {ArtifactService} from "../../../../managers/artifact-manager/artifact/artifact.svc";
import {ArtifactAttachmentsService} from "../../../../managers/artifact-manager/attachments/attachments.svc";
import {IRelationship} from "../../../../main/models/relationshipModels";

describe("BPArtifactRelationshipItem", () => {

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
        $provide.service("validationService", ValidationServiceMock);
    }));

    let directiveTest: ComponentTest<BPArtifactRelationshipItemController>;
    let vm: BPArtifactRelationshipItemController;


    beforeEach(inject(() => {
        let template = `<bp-artifact-relationship-item relationship="::artifact"></bp-artifact-relationship-item>`;
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

    it("action panel should be hidden if item is not manual trace or user has no access to it", () => {
        //Assert
        expect(directiveTest.element.find(".icons").length).toBe(0);
    });

    it("action panel should be visible if item is manual trace and user has access to it", () => {
        inject(($rootScope: ng.IRootScopeService) => {
            //Arrange

            let component = `<bp-artifact-relationship-item relationship="::artifact" selectable="true"></bp-artifact-relationship-item>`;
            let directiveTest2: ComponentTest<BPArtifactRelationshipItemController> =
                new ComponentTest<BPArtifactRelationshipItemController>(component, "bp-artifact-relationship-item");

            let vm2: BPArtifactRelationshipItemController = directiveTest2.createComponent({});

            vm2.showActionsPanel = true;

            $rootScope.$digest();

            //Assert
            expect(directiveTest2.element.find(".icons").length).toBe(1);
        });
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

            vm.relationship = <IRelationship>{
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

    describe("select artifact", () => {

        beforeEach(() => {
            vm.relationship = <IRelationship>{
                "artifactId": 1,
                "itemId": 3
            };
            vm.selectedTraces = [];

        });

        it("select, select artifact", () => {
            //Arrange
            vm.relationship.isSelected = false;

            //Act
            vm.selectTrace();

            //Assert
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.relationship.isSelected).toBe(true);

        });

        it("select, select artifact that already in the array", () => {
            //Arrange
            vm.relationship.isSelected = false;
            vm.selectedTraces.push(vm.relationship);

            //Act
            vm.selectTrace();

            //Assert
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.relationship.isSelected).toBe(true);

        });

        it("select, unselect artifact", () => {
            //Arrange
            vm.relationship.isSelected = true;
            vm.selectedTraces.push(vm.relationship);

            //Act
            vm.selectTrace();

            //Assert
            expect(vm.selectedTraces.length).toBe(0);
            expect(vm.relationship.isSelected).toBe(false);

        });

        it("select, unselect artifact that is not in the array", () => {
            //Arrange
            let artifact = <IRelationship>{
                "artifactId": 1,
                "itemId": 4
            };
            vm.relationship.isSelected = true;

            //Act
            vm.selectedTraces.push(artifact);
            vm.selectTrace();

            //Assert
            expect(vm.selectedTraces.length).toBe(1);
            expect(vm.relationship.isSelected).toBe(false);

        });
    });
});
