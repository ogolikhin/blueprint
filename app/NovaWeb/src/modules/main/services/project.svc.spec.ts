import "angular";
import "angular-mocks";
import {IProjectService, ProjectService, Data} from "./project.svc";
import {LocalizationServiceMock} from "../../shell/login/mocks.spec";

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

    public getProject(id?: number): ng.IPromise<Data.IProjectItem[]> {
        var deferred = this.$q.defer <Data.IProjectItem[]>();
        var items: Data.IProjectItem[] = [];
        deferred.resolve(items);
        return deferred.promise;
    }
}

describe("ProjectService", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectService", ProjectService);
        $provide.service("localization", LocalizationServiceMock);
    }));

    describe("getFolders", () => {
        it("resolve successfully - one older", inject(($httpBackend: ng.IHttpBackendService, projectService: IProjectService) => {
                // Arrange
            $httpBackend.expectGET("svc/adminstore/instance/folders/1/children")
                .respond(200, <Data.IProjectNode[]>[
                        { Id: 3, "ParentFolderId": 1, Name: "Imported Projects", Type: "Folder", Description : "" }
                    ]
                    );

                // Act
            var error: any;
            var data: Data.IProjectNode[];
            projectService.getFolders().then((responce) => { data = responce; }, (err) => error = err);
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
            var data: Data.IProjectNode[];
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