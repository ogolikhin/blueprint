import * as angular from "angular";
import "angular-mocks";
import {BreadcrumbService, IArtifactReference} from "./breadcrumb.svc";
import {ItemTypePredefined} from "../../../main/models/enums";
import {NavigationServiceMock} from "../../../core/navigation/navigation.svc.mock";
import {INavigationService} from "../../../core/navigation/navigation.svc";

describe("BreadcrumbService", () => {
    let $rootScope: ng.IRootScopeService;
    let $q: ng.IQService;
    let $http: ng.IHttpService;
    let $httpBackend: ng.IHttpBackendService;
    let navigationService: INavigationService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("navigationService", NavigationServiceMock);
    }));

    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService, 
        _$q_: ng.IQService, 
        _$http_: ng.IHttpService, 
        _$httpBackend_: ng.IHttpBackendService, 
        _navigationService_: INavigationService
    ) => {
        $rootScope = _$rootScope_;
        $q = _$q_;
        $http = _$http_;
        $httpBackend = _$httpBackend_;
        navigationService = _navigationService_;
    }));

    it("getReferences doesn't make an http call if no navigation history exists", (done: DoneFn) => {
        // arrange
        const navigationState = { id: 1 };
        const service = new BreadcrumbService($q, $http, navigationService);
        
        const deferred = $q.defer();
        deferred.resolve([]);
        const getSpy = spyOn($http, "get").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IArtifactReference[]) => {
                    done.fail("Expected getReferences to fail");
                },
                (reason: any) => {
                    // assert
                    expect(getSpy).not.toHaveBeenCalled();
                    done();
                }
            );

        $rootScope.$digest();
    });

    it("getReferences doesn't make an http call if navigation history is empty", (done: DoneFn) => {
        // arrange
        const navigationState = { id: 1, path: [] };
        const service = new BreadcrumbService($q, $http, navigationService);
        
        const deferred = $q.defer();
        deferred.resolve([]);
        const getSpy = spyOn($http, "get").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IArtifactReference[]) => {
                    done.fail("Expected getReferences to fail");
                },
                (reason: any) => {
                    // assert
                    expect(getSpy).not.toHaveBeenCalled();
                    done();
                }
            );

        $rootScope.$digest();
    });

    it("getReferences makes an http call if navigation history exists", (done: DoneFn) => {
        // arrange
        const navigationState = { id: 1, path: [2, 3] };
        const service = new BreadcrumbService($q, $http, navigationService);

        const deferred = $q.defer();
        deferred.resolve([]);
        const getSpy = spyOn($http, "get").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IArtifactReference[]) => {
                    // assert
                    expect(getSpy).toHaveBeenCalledWith("/svc/shared/navigation/2/3/1");
                    done();
                },
                (reason: any) => {
                    done.fail("Expected getReferences to succeed");
                }
            );
        
        $rootScope.$digest();
    });

    it("getReferences fails if http call fails", (done: DoneFn) => {
        // arrange
        const navigationState = { id: 1, path: [2, 3] };
        const service = new BreadcrumbService($q, $http, navigationService);

        const deferred = $q.defer();
        deferred.reject("Error");
        spyOn($http, "get").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IArtifactReference[]) => {
                    // assert
                    done.fail("Expected getReferences to fail");
                    
                },
                (reason: any) => {
                    expect(reason).toEqual("Error");
                    done();
                }
            );
        
        $rootScope.$digest();
    });

    it("getReferences succeeds if http call succeeds", (done: DoneFn) => {
        // arrange
        const navigationState = { id: 1, path: [2, 3] };
        const service = new BreadcrumbService($q, $http, navigationService);
        const references = [{
            id: 1,
            projectId: 67,
            name: "test",
            typePrefix: "TST",
            projectName: "Test Project",
            baseItemTypePredefined: ItemTypePredefined.Process,
            link: "http://test"
        }];

        const deferred = $q.defer();
        deferred.resolve({ data: references });
        spyOn($http, "get").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IArtifactReference[]) => {
                    // assert
                    expect(promiseValue).toEqual(references);
                    done();
                },
                (reason: any) => {
                    done.fail("Expected getReferences to succeed");
                }
            );
        
        $rootScope.$digest();
    });
});