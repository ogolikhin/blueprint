import * as angular from "angular";
import "angular-mocks";
import "Rx";
import { LocalizationServiceMock } from "../../core//localization/localization.mock";
import { IGlossaryService, GlossaryService } from "./glossary.svc";
import { IArtifact } from "../../main/models/models";

describe("Glossary Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("glossaryService", GlossaryService);
        $provide.service("localization", LocalizationServiceMock);
    })); 

    it("get glossary details", 
        inject(($httpBackend: ng.IHttpBackendService, glossaryService: IGlossaryService) => {

        // Arrange
        /* tslint:disable:max-line-length */
        $httpBackend.expectGET("/svc/bpartifactstore/glossary/263")
            .respond(200, {
                    "id": 263,
                    "subArtifacts": [
                        {
                            "id": 264,
                            "name": "fleek",
                            "definition": "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">on point</p></div></body></html>",
                            "typePrefix": "TR",
                            "predefined": 8217
                        },
                        {
                            "id": 386,
                            "name": "google",
                            "definition": "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">&#x200b;<a href=\"http://www.google.com/\" style=\"color: Blue; text-decoration: underline\"><span style=\"font-family: 'Portable User Interface'; font-size: 11px\">google.com</span></a><span style=\"-c1-editable: true; font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; color: Black\">&#x200b;</span></p></div></body></html>",
                            "typePrefix": "TR",
                            "predefined": 8217
                        }
                    ]
                });
        /* tslint:enable:max-line-length */

        // Act
        let error: any;
        let data: IArtifact;
        glossaryService.getGlossary(263).then((response) => {
            data = response;
        }, (err) => {
            error = err; 
        });
        $httpBackend.flush();

        // Assert
        expect(error).toBeUndefined();
        expect(data.id).toBe(263);
        expect(data.subArtifacts.length).toBe(2);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

    it("gets an error if artifact id is invalid", 
        inject(($httpBackend: ng.IHttpBackendService, glossaryService: IGlossaryService) => {

        // Arrange
        $httpBackend.expectGET("/svc/bpartifactstore/glossary/0")
            .respond(404, {
                statusCode: 404,
                message: "Couldn't find the artifact"
            });

        // Act
        let error: any;
        let data: IArtifact;
        glossaryService.getGlossary(0).then( (response) => {
            data = response;
        }, (err) => {
            error = err;
        });

        $httpBackend.flush();

        // Assert
        expect(data).toBeUndefined();
        expect(error.statusCode).toEqual(404);
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    }));

});
