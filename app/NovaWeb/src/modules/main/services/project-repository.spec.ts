import "angular";
import "angular-mocks";
import {IProjectRepository, ProjectRepository, Models} from "./project-repository";
import {ProjectRepositoryMock} from "./project-repository.mock";
import {IProjectNotification, SubscriptionEnum} from "./project-notification";
import {LocalizationServiceMock} from "../../shell/login/mocks.spec";

class ProjectNotificationMock implements IProjectNotification {

    public subscribe(type: SubscriptionEnum, func: Function) {
    };
    public unsubscribe(type: SubscriptionEnum, func: Function) {
    };
    public notify(type: SubscriptionEnum, ...prms: any[]) {
    };
}

describe("ProjectService", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectRepository", ProjectRepository);
        $provide.service("projectNotification", ProjectNotificationMock);
        $provide.service("localization", LocalizationServiceMock);
    }));

    describe("getFolders", () => {
        it("resolve successfully - one older", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
                // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/1/children")
                .respond(200, <Models.IProjectNode[]>[
                    { id: 3, name: "Imported Projects", type: 0, description: "", parentFolderId: 1, hasChildren: false }
                    ]);

                // Act
            var error: any;
            var data: Models.IProjectNode[];
            projectRepository.getFolders().then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(data).toEqual(jasmine.any(Array), "incorrect type");
            expect(data.length).toBe(1, "incorrect data returned");
            expect(data[0].id).toBe(3, "incorrect id returned");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
            }));
        });
        it("resolve unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectRepository: IProjectRepository) => {
            // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/5/children")
                .respond(200, <any[]>[]
                );

            // Act
            var error: any;
            var data: Models.IProjectNode[];
            projectRepository.getFolders(5).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(data).toEqual(jasmine.any(Array), "incorrect type");
            expect(data.length).toBe(0, "incorrect data returned");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
});