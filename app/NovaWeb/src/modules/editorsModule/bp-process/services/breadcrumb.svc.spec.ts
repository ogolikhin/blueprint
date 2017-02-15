import * as angular from "angular";
import "angular-mocks";
import {BreadcrumbService, IPathItem} from "./breadcrumb.svc";
import {NavigationServiceMock} from "../../../commonModule/navigation/navigation.service.mock";
import {INavigationService} from "../../../commonModule/navigation/navigation.service";

describe("BreadcrumbService", () => {
    let $rootScope: ng.IRootScopeService;
    let $q: ng.IQService;
    let $http: ng.IHttpService;
    let $httpBackend: ng.IHttpBackendService;
    let navigationService: INavigationService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("navigationService", NavigationServiceMock);
    }));

    beforeEach(inject((_$rootScope_: ng.IRootScopeService,
                       _$q_: ng.IQService,
                       _$http_: ng.IHttpService,
                       _$httpBackend_: ng.IHttpBackendService,
                       _navigationService_: INavigationService) => {
        $rootScope = _$rootScope_;
        $q = _$q_;
        $http = _$http_;
        $httpBackend = _$httpBackend_;
        navigationService = _navigationService_;
    }));

    it("getReferences doesn't make an http call if no navigation history exists", (done: DoneFn) => {
        // arrange
        const navigationState = {id: 1};
        const service = new BreadcrumbService($q, $http, navigationService);

        const deferred = $q.defer();
        deferred.resolve([]);
        const getSpy = spyOn($http, "get").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IPathItem[]) => {
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
        const navigationState = {id: 1, path: []};
        const service = new BreadcrumbService($q, $http, navigationService);

        const deferred = $q.defer();
        deferred.resolve([]);
        const getSpy = spyOn($http, "get").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IPathItem[]) => {
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
        const path = [
            {id: 2},
            {id: 3}
        ];
        const navigationState = {id: 1, path: path};
        const service = new BreadcrumbService($q, $http, navigationService);
        const expectedPathItems = [
            {id: 2, version: undefined},
            {id: 3, version: undefined},
            {id: 1, version: undefined}
        ];
        const deferred = $q.defer();
        deferred.resolve([]);
        const postSpy = spyOn($http, "post").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IPathItem[]) => {
                    // assert
                    expect(postSpy).toHaveBeenCalledWith("svc/bpartifactstore/process/breadcrumb", expectedPathItems);
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
        const path = [{id: 2}, {id: 3}];
        const navigationState = {id: 1, path: path};
        const service = new BreadcrumbService($q, $http, navigationService);

        const deferred = $q.defer();
        deferred.reject("Error");
        spyOn($http, "post").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IPathItem[]) => {
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
        const path = [{id: 2, version: undefined}, {id: 3, version: undefined}];
        const navigationState = {id: 1, path: path};
        const service = new BreadcrumbService($q, $http, navigationService);
        const expectedResult = [
            {
                id: 2,
                name: "2",
                isEnabled: true
            },
            {
                id: 3,
                name: "3",
                isEnabled: true
            },
            {
                id: 1,
                name: "1",
                isEnabled: false
            }
        ];

        const deferred = $q.defer();
        deferred.resolve({data: expectedResult});
        spyOn($http, "post").and.returnValue(deferred.promise);
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences()
            .then(
                (promiseValue: IPathItem[]) => {
                    // assert
                    expect(promiseValue).toEqual(expectedResult);
                    done();
                },
                (reason: any) => {
                    done.fail("Expected getReferences to succeed");
                }
            );

        $rootScope.$digest();
    });
});
