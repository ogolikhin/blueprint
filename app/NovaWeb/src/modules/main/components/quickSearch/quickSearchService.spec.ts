import "../../";
import * as angular from "angular";
import "angular-mocks";
import {QuickSearchService, IQuickSearchService} from "./quickSearchService";
import { ProjectManagerMock } from "../../../managers/project-manager/project-manager.mock";
import { ArtifactNode } from "../../../managers/project-manager/artifact-node";
import { StatefulArtifact } from "../../../managers/artifact-manager";
import { HttpStatusCode } from "../../../core/http/http-status-code";

describe("Service: Quick Search", () => {
    let service: IQuickSearchService;
    let projectManager: ProjectManagerMock;
    let $httpBackend: ng.IHttpBackendService;
    // Load the module
    beforeEach(angular.mock.module("app.main"));

    // Provide any mocks needed
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectManager", ProjectManagerMock);
    }));

    // Inject in angular constructs otherwise,
    //  you would need to inject these into each test
    beforeEach(inject((
        quickSearchService: IQuickSearchService,
        _projectManager_: ProjectManagerMock,
         _$httpBackend_: ng.IHttpBackendService
        ) => {
        service = quickSearchService;
        projectManager = _projectManager_;
        $httpBackend = _$httpBackend_;
    }));

    it("should contain a QuickSearchService", () => {
        expect(service).toBeDefined();
    });

    it("search is only enabled if you have at least one open project", () => {
        expect(service.canSearch()).toBe(true);
        const statefulArtifact = new StatefulArtifact({id: 123}, null);
        const project = new ArtifactNode(statefulArtifact);
        projectManager.projectCollection.getValue().push(project);
        expect(service.canSearch()).toBe(false);
    });

    it("searchmetadata - no parameters", () => {
        // arrange
        const statefulArtifact = new StatefulArtifact({id: 123}, null);
        const project = new ArtifactNode(statefulArtifact);
        projectManager.projectCollection.getValue().push(project);
        const data =  {
            "Query": "abc",
            "ProjectIds": projectManager.projectCollection.getValue().map(project => project.id)
        };
        const totalCount = 100;
        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltextmetadata/`, data)
            .respond(HttpStatusCode.Success,
                {
                    totalCount: totalCount
                }
            );
        let results;

        // act
        service.metadata("abc").then((result) => {
            results = result;
        });
        $httpBackend.flush();

        // assert
        expect(results).toBeDefined();
        expect(results.totalCount).toBe(totalCount);
    });

    it("searchmetadata - with pageSize", () => {
        // arrange
        const statefulArtifact = new StatefulArtifact({id: 123}, null);
        const project = new ArtifactNode(statefulArtifact);
        projectManager.projectCollection.getValue().push(project);
        const pageSize = 10;
        const data =  {
            "Query": "abc",
            "ProjectIds": projectManager.projectCollection.getValue().map(project => project.id)
        };
        const totalCount = 100;
        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltextmetadata/?pageSize=${pageSize}`, data)
            .respond(HttpStatusCode.Success,
                {
                    totalCount: totalCount
                }
            );
        let results;

        // act
        service.metadata("abc", null, pageSize).then((result) => {
            results = result;
        });
        $httpBackend.flush();

        // assert
        expect(results).toBeDefined();
        expect(results.totalCount).toBe(totalCount);
    });

    it("searchmetadata - with page and page size", () => {
        // arrange
        const statefulArtifact = new StatefulArtifact({id: 123}, null);
        const project = new ArtifactNode(statefulArtifact);
        projectManager.projectCollection.getValue().push(project);
        const page = 1;
        const pageSize = 10;
        const data =  {
            "Query": "abc",
            "ProjectIds": projectManager.projectCollection.getValue().map(project => project.id)
        };
        const totalCount = 100;
        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltextmetadata/?page=${page}&pageSize=${pageSize}`, data)
            .respond(HttpStatusCode.Success,
                {
                    totalCount: totalCount
                }
            );
        let results;

        // act
        service.metadata("abc", page, pageSize).then((result) => {
            results = result;
        });
        $httpBackend.flush();

        // assert
        expect(results).toBeDefined();
        expect(results.totalCount).toBe(totalCount);
    });


    it("search - no parameters", () => {
        // arrange
        const statefulArtifact = new StatefulArtifact({id: 123}, null);
        const project = new ArtifactNode(statefulArtifact);
        projectManager.projectCollection.getValue().push(project);
        const data =  {
            "Query": "abc",
            "ProjectIds": projectManager.projectCollection.getValue().map(project => project.id)
        };

        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltext/`, data)
            .respond(HttpStatusCode.Success,
                {
                    items: []
                }
            );
        let results;

        // act
        service.search("abc").then((result) => {
            results = result;
        });
        $httpBackend.flush();

        // assert
        expect(results).toBeDefined();
        expect(results.items).toBeDefined();
        expect(results.items.length).toBe(0);
    });

    it("search - with page and page size", () => {
        // arrange
        const statefulArtifact = new StatefulArtifact({id: 123}, null);
        const project = new ArtifactNode(statefulArtifact);
        projectManager.projectCollection.getValue().push(project);
        const page = 1;
        const pageSize = 10;
        const data =  {
            "Query": "abc",
            "ProjectIds": projectManager.projectCollection.getValue().map(project => project.id)
        };

        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltext/?page=${page}&pageSize=${pageSize}`, data)
            .respond(HttpStatusCode.Success,
                {
                    items: []
                }
            );
        let results;

        // act
        service.search("abc", page, pageSize).then((result) => {
            results = result;
        });
        $httpBackend.flush();

        // assert
        expect(results).toBeDefined();
        expect(results.items).toBeDefined();
        expect(results.items.length).toBe(0);
    });

    it("search - with page", () => {
        // arrange
        const statefulArtifact = new StatefulArtifact({id: 123}, null);
        const project = new ArtifactNode(statefulArtifact);
        projectManager.projectCollection.getValue().push(project);
        const page = 1;
        const data =  {
            "Query": "abc",
            "ProjectIds": projectManager.projectCollection.getValue().map(project => project.id)
        };

        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltext/?page=${page}`, data)
            .respond(HttpStatusCode.Success,
                {
                    items: []
                }
            );
        let results;

        // act
        service.search("abc", page).then((result) => {
            results = result;
        });
        $httpBackend.flush();

        // assert
        expect(results).toBeDefined();
        expect(results.items).toBeDefined();
        expect(results.items.length).toBe(0);
    });

    it("search - with pageSize", () => {
        // arrange
        const statefulArtifact = new StatefulArtifact({id: 123}, null);
        const project = new ArtifactNode(statefulArtifact);
        projectManager.projectCollection.getValue().push(project);
        const pageSize = 10;
        const data =  {
            "Query": "abc",
            "ProjectIds": projectManager.projectCollection.getValue().map(project => project.id)
        };

        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltext/?pageSize=${pageSize}`, data)
            .respond(HttpStatusCode.Success,
                {
                    items: []
                }
            );
        let results;

        // act
        service.search("abc", null, pageSize).then((result) => {
            results = result;
        });
        $httpBackend.flush();

        // assert
        expect(results).toBeDefined();
        expect(results.items).toBeDefined();
        expect(results.items.length).toBe(0);
    });
});
