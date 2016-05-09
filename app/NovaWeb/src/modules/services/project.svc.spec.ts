import "angular";
import "angular-mocks"
import {IProjectNode, IProjectService, ProjectService} from "./project.svc";
import {LocalizationServiceMock} from "../shell/login/mocks.spec";

describe("ProjectService", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectService", ProjectService);
        $provide.service("localization", LocalizationServiceMock);
    }));

    describe("getFolders", () => {
        it("resolve successfully - one older", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
                // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/1/children")
                .respond(200, <IProjectNode[]>[
                        { Id: 3, "ParentFolderId": 1, Name: "Imported Projects", Type: "Folder", Description : "" }
                    ]
                    );

                // Act
            var error: any;
            var data: IProjectNode[];
            var result = projectService.getFolders().then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(data).toEqual(jasmine.any(Array), "incorrect type");
            expect(data.length).toBe(1, "incorrect data returned");
            expect(data[0].Id).toBe(3, "incorrect id returned");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
            }));
        });
        it("resolve unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/5/children")
                .respond(200, <any[]>[]
                );

            // Act
            var error: any;
            var data: IProjectNode[];
            var result = projectService.getFolders(5).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(data).toEqual(jasmine.any(Array), "incorrect type")
            expect(data.length).toBe(0, "incorrect data returned")
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
});