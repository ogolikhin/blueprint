import "angular";
import "angular-mocks";
import {IProjectService, ProjectService, Models} from "./project.svc";
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

export class ProjectServiceMock implements IProjectService {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) { }

    public getFolders(id?: number): ng.IPromise<any[]> {
        var deferred = this.$q.defer<any[]>();
        var folders = [
            {
                "Id": 3,
                "ParentFolderId": 1,
                "Name": "Folder with content",
                "Type": "Folder"
            },
            {
                "Id": 7,
                "ParentFolderId": 1,
                "Name": "Empty folder",
                "Type": "Folder"
            },
            {
                "Id": 8,
                "ParentFolderId": 1,
                "Name": "<button onclick=\"alert('Hey!')\">Embedded HTML in name</button>",
                "Type": "Folder"
            },
            {
                "Id": 33,
                "ParentFolderId": 1,
                "Name": "Process",
                "Description": "Process description",
                "Type": "Project"
            }
        ];
        deferred.resolve(folders);
        return deferred.promise;
    }

    public getProject(id?: number): ng.IPromise<Models.IProjectItem[]> {
        var deferred = this.$q.defer <Models.IProjectItem[]>();
        var items: Models.IProjectItem[] = [];
        deferred.resolve(items);
        return deferred.promise;
    }
}

describe("ProjectService", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectService", ProjectService);
        $provide.service("projectNotification", ProjectNotificationMock);
        $provide.service("localization", LocalizationServiceMock);
    }));

    describe("getFolders", () => {
        it("resolve successfully - one older", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
                // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/1/children")
                .respond(200, <Models.IProjectNode[]>[
                        { id: 3, parentFolderId: 1, name: "Imported Projects", type: "Folder", description : "" }
                    ]
                    );

                // Act
            var error: any;
            var data: Models.IProjectNode[];
            projectService.getFolders().then((responce) => { data = responce; }, (err) => error = err);
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
        it("resolve unsuccessfully", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
            // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/5/children")
                .respond(200, <any[]>[]
                );

            // Act
            var error: any;
            var data: Models.IProjectNode[];
            projectService.getFolders(5).then((responce) => { data = responce; }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(data).toEqual(jasmine.any(Array), "incorrect type");
            expect(data.length).toBe(0, "incorrect data returned");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
});