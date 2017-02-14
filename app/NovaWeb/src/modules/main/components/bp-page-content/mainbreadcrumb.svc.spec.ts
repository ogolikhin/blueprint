import "angular-mocks";
import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";
import {ArtifactService} from "../../../managers/artifact-manager";
import {ArtifactServiceMock} from "../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {ProjectService} from "../../../managers/project-manager/project-service";
import {ProjectServiceMock} from "../../../managers/project-manager/project-service.mock";
import {ItemTypePredefined} from "../../models/itemTypePredefined.enum";
import {IMainBreadcrumbService, MainBreadcrumbService} from "./mainbreadcrumb.svc";
import * as angular from "angular";

describe("Breadcrumb service successful", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("projectService", ProjectServiceMock);
        $provide.service("mainbreadcrumbService", MainBreadcrumbService);
    }));

    describe("reload breadcrumb", () => {

        it("reload artifact breadcrumb successfully",
            inject(($timeout: ng.ITimeoutService, mainbreadcrumbService: IMainBreadcrumbService) => {

                // Arrange
                mainbreadcrumbService.breadcrumbLinks = [];

                // Act
                let error: any;
                let data = ArtifactServiceMock.createArtifact(100);
                let artifactState = {
                    historical: false
                };
                data.artifactState = artifactState;
                mainbreadcrumbService.reloadBreadcrumbs(data);
                $timeout.flush();

                // Assert
                expect(error).toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks).not.toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks.length).toEqual(1);
            }));

        it("reload project breadcrumb successfully",
            inject(($timeout: ng.ITimeoutService, mainbreadcrumbService: IMainBreadcrumbService) => {

                // Arrange
                mainbreadcrumbService.breadcrumbLinks = [];

                // Act
                let error: any;
                let data = ArtifactServiceMock.createArtifact(100);
                data.predefinedType = ItemTypePredefined.Project;
                mainbreadcrumbService.reloadBreadcrumbs(data);
                $timeout.flush();

                // Assert
                expect(error).toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks).not.toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks.length).toEqual(1);
            }));
    });

});

describe("Breadcrumb service unsuccessful", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("mainbreadcrumbService", MainBreadcrumbService);
        $provide.service("artifactService", ArtifactService);
        $provide.service("projectService", ProjectService);
    }));

    describe("reload breadcrumb", () => {

        it("reload artifact breadcrumb unsuccessful",
            inject(($httpBackend: ng.IHttpBackendService, mainbreadcrumbService: IMainBreadcrumbService) => {

                // Arrange
                mainbreadcrumbService.breadcrumbLinks = [];
                $httpBackend.expectGET("/svc/artifactstore/artifacts/100/navigationPath")
                    .respond(HttpStatusCode.NotFound, {
                        statusCode: HttpStatusCode.NotFound
                    });

                // Act
                let error: any;
                let data = ArtifactServiceMock.createArtifact(100);
                let artifactState = {
                    historical: false
                };
                data.artifactState = artifactState;
                mainbreadcrumbService.reloadBreadcrumbs(data);
                $httpBackend.flush();

                // Assert
                expect(error).toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks).not.toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks.length).toEqual(0);
            }));

        it("reload breadcrumb for deleted artifact is not available",
            inject(($httpBackend: ng.IHttpBackendService, mainbreadcrumbService: IMainBreadcrumbService) => {

                // Arrange
                mainbreadcrumbService.breadcrumbLinks = [];

                // Act
                let error: any;
                let data = ArtifactServiceMock.createArtifact(100);
                let artifactState = {
                    historical: true,
                    deleted: true
                };
                data.artifactState = artifactState;
                mainbreadcrumbService.reloadBreadcrumbs(data);

                // Assert
                $httpBackend.verifyNoOutstandingRequest();
                expect(error).toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks).not.toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks.length).toEqual(0);
            })
        );

        it("reload project breadcrumb unsuccessful",
            inject(($httpBackend: ng.IHttpBackendService, mainbreadcrumbService: IMainBreadcrumbService) => {

                // Arrange
                mainbreadcrumbService.breadcrumbLinks = [];
                $httpBackend.expectGET("/svc/adminstore/instance/projects/100/navigationPath?includeProjectItself=false")
                    .respond(HttpStatusCode.NotFound, {
                        statusCode: HttpStatusCode.NotFound
                    });

                // Act
                let error: any;
                let data = ArtifactServiceMock.createArtifact(100);
                data.predefinedType = ItemTypePredefined.Project;
                mainbreadcrumbService.reloadBreadcrumbs(data);
                $httpBackend.flush();

                // Assert
                expect(error).toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks).not.toBeUndefined();
                expect(mainbreadcrumbService.breadcrumbLinks.length).toEqual(0);
            }));
    });

});


