import "angular-mocks";
import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";
import {Models} from "../../models";
import {IQuickSearchService} from "./quickSearchService";
import * as angular from "angular";
import {IProjectExplorerService} from "../bp-explorer/project-explorer.service";

describe("Service: Quick Search", () => {
    let service: IQuickSearchService;
    let projectExplorerService: IProjectExplorerService;
    let $httpBackend: ng.IHttpBackendService;
    // Load the module
    beforeEach(angular.mock.module("app.main"));

    // Provide any mocks needed
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        const projectExplorerService = {
            projects: []
        } as IProjectExplorerService;
        $provide.service("projectExplorerService", () => projectExplorerService);
    }));

    // Inject in angular constructs otherwise,
    //  you would need to inject these into each test
    beforeEach(inject((quickSearchService: IQuickSearchService,
                       _projectExplorerService_: IProjectExplorerService,
                       _$httpBackend_: ng.IHttpBackendService) => {
        service = quickSearchService;
        projectExplorerService = _projectExplorerService_;
        $httpBackend = _$httpBackend_;
    }));

    it("should contain a QuickSearchService", () => {
        expect(service).toBeDefined();
    });

    it("search is only enabled if you have at least one open project", () => {
        expect(service.canSearch()).toBe(true);
        const project = {model: {id: 123}} as Models.IViewModel<Models.IArtifact> as any;
        projectExplorerService.projects.push(project);
        expect(service.canSearch()).toBe(false);
    });

    it("searchmetadata - no parameters", () => {
        // arrange
        const project = {model: {id: 123}} as Models.IViewModel<Models.IArtifact> as any;
        projectExplorerService.projects.push(project);
        const data = {
            "Query": "abc",
            "ProjectIds": projectExplorerService.projects.map(project => project.model.id)
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
        const project = {model: {id: 123}} as Models.IViewModel<Models.IArtifact> as any;
        projectExplorerService.projects.push(project);
        const pageSize = 10;
        const data = {
            "Query": "abc",
            "ProjectIds": projectExplorerService.projects.map(project => project.model.id)
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
        const project = {model: {id: 123}} as Models.IViewModel<Models.IArtifact> as any;
        projectExplorerService.projects.push(project);
        const page = 1;
        const pageSize = 10;
        const data = {
            "Query": "abc",
            "ProjectIds": projectExplorerService.projects.map(project => project.model.id)
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
        const project = {model: {id: 123}} as Models.IViewModel<Models.IArtifact> as any;
        projectExplorerService.projects.push(project);
        const data = {
            "Query": "abc",
            "ProjectIds": projectExplorerService.projects.map(project => project.model.id)
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
        const project = {model: {id: 123}} as Models.IViewModel<Models.IArtifact> as any;
        projectExplorerService.projects.push(project);
        const page = 1;
        const pageSize = 10;
        const data = {
            "Query": "abc",
            "ProjectIds": projectExplorerService.projects.map(project => project.model.id)
        };

        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltext/?page=${page}&pageSize=${pageSize}`, data)
            .respond(HttpStatusCode.Success,
                {
                    items: []
                }
            );
        let results;

        // act
        service.search("abc", "header", page, pageSize).then((result) => {
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
        const project = {model: {id: 123}} as Models.IViewModel<Models.IArtifact> as any;
        projectExplorerService.projects.push(project);
        const page = 1;
        const data = {
            "Query": "abc",
            "ProjectIds": projectExplorerService.projects.map(project => project.model.id)
        };

        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltext/?page=${page}`, data)
            .respond(HttpStatusCode.Success,
                {
                    items: []
                }
            );
        let results;

        // act
        service.search("abc", "header", page).then((result) => {
            results = result;
        });
        $httpBackend.flush();

        // assert
        expect(results).toBeDefined();
        expect(results.items).toBeDefined();
        expect(results.items.length).toBe(0);
    });

    xit("search - with pageSize", () => {
        // arrange
        const project = {model: {id: 123}} as Models.IViewModel<Models.IArtifact> as any;
        projectExplorerService.projects.push(project);
        const pageSize = 10;
        const data = {
            "Query": "abc",
            "ProjectIds": projectExplorerService.projects.map(project => project.model.id)
        };

        $httpBackend.expectPOST(`/svc/searchservice/itemsearch/fulltext/?pageSize=${pageSize}`, data)
            .respond(HttpStatusCode.Success,
                {
                    items: []
                }
            );
        let results;

        // act
        service.search("abc", "header", pageSize).then((result) => {
            results = result;
        });
        $httpBackend.flush();

        // assert
        expect(results).toBeDefined();
        expect(results.items).toBeDefined();
        expect(results.items.length).toBe(0);
    });
});
