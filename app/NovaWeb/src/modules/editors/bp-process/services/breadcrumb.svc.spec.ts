import * as angular from "angular";
import {BreadcrumbService} from "./breadcrumb.svc";
import {NavigationServiceMock} from "../../../core/navigation/navigation.svc.mock";
import {INavigationService} from "../../../core/navigation/navigation.svc";

describe("BreadcrumbService", () => {
    let $q: ng.IQService;
    let $http: ng.IHttpService;
    let navigationService: INavigationService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("navigationService", NavigationServiceMock);
    }));

    beforeEach(inject((_$q_: ng.IQService, _$http_: ng.IHttpService, _navigationService_: INavigationService) => {
        $q = _$q_;
        $http = _$http_;
        navigationService = _navigationService_;
    }));

    it("getReferences doesn't make an http call if no navigation history exists", (done: DoneFn) => inject(($provide: ng.auto.IProvideService) => {
        // arrange
        const navigationState = { id: 1 };
        const service = new BreadcrumbService($q, $http, navigationService);
        const getSpy = spyOn($http, "get");
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences();

        // assert
        expect(getSpy).not.toHaveBeenCalled();
    }));

    it("getReferences makes an http call if navigation history exists", (done: DoneFn) => inject(($provide: ng.auto.IProvideService) => {
        // arrange
        const navigationState = { id: 1, path: [2, 3] };
        const service = new BreadcrumbService($q, $http, navigationService);
        const getSpy = spyOn($http, "get");
        spyOn(navigationService, "getNavigationState").and.returnValues(navigationState);

        // act
        service.getReferences();

        // assert
        expect(getSpy).toHaveBeenCalledWith("/svc/shared/navigation/2/3/1");
    }));
});