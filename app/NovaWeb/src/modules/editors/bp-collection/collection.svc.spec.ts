import * as angular from "angular";
import "angular-mocks";
import "rx/dist/rx.lite";
import {LocalizationServiceMock} from "../../core//localization/localization.mock";
import {ICollectionService, CollectionService} from "./collection.svc";
//import {IArtifact} from "../../main/models/models";
import {HttpStatusCode} from "../../core/http";
import {Models} from "../../main";
import {ICollection, ICollectionArtifact} from "./models";

describe("Collection Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("collectionService", CollectionService);
        $provide.service("localization", LocalizationServiceMock);
    }));

    it("get collection details",
        inject(($httpBackend: ng.IHttpBackendService, collectionService: ICollectionService) => {
            const collectionId: number = 262;
            // Arrange
            /* tslint:disable:max-line-length */
            $httpBackend.expectGET("/svc/bpartifactstore/collection/" + collectionId)
                .respond(HttpStatusCode.Success, {
                    "id": collectionId,
                    "subArtifacts": [
                        {                            
                            "id": 264,
                            "name": "fleek",
                            "description": "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">on point</p></div></body></html>",
                            "prefix": "TR",
                            "itemTypeId": 5,
                            "itemTypePredefined": Models.ItemTypePredefined.Actor,
                            "artifactPath": "Path1"   
                        },
                        {
                            "id": 386,
                            "name": "google",
                            "description": "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">&#x200b;<a href=\"http://www.google.com/\" style=\"color: Blue; text-decoration: underline\"><span style=\"font-family: 'Portable User Interface'; font-size: 11px\">google.com</span></a><span style=\"-c1-editable: true; font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; color: Black\">&#x200b;</span></p></div></body></html>",
                            "prefix": "TR",
                            "itemTypeId": 5,
                            "itemTypePredefined": Models.ItemTypePredefined.Actor,
                            "artifactPath": "Path1"  
                        }
                    ]
                });
            /* tslint:enable:max-line-length */

            // Act
            let error: any;
            let data: ICollection;
            collectionService.getCollection(collectionId).then((response) => {
                data = response;
            }, (err) => {
                error = err;
            });
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(data.id).toBe(collectionId);
            expect(data.subArtifacts.length).toBe(2);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

    it("gets an error if collection id is invalid",
        inject(($httpBackend: ng.IHttpBackendService, collectionService: ICollectionService) => {

            const collectionId: number = 0;
            // Arrange
            $httpBackend.expectGET("/svc/bpartifactstore/collection/" + collectionId)
                .respond(HttpStatusCode.NotFound, {
                    statusCode: HttpStatusCode.NotFound,
                    message: "Couldn't find the collection"
                });

            // Act
            let error: any;
            let data: ICollection;
            collectionService.getCollection(collectionId).then((response) => {
                data = response;
            }, (err) => {
                error = err;
            });

            $httpBackend.flush();

            // Assert
            expect(data).toBeUndefined();
            expect(error.statusCode).toEqual(HttpStatusCode.NotFound);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
});
