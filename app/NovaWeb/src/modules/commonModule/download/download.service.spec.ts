import {HttpStatusCode} from "../httpInterceptor/http-status-code";
import "angular";
import "angular-mocks";
import {HeartbeatService, IHeartbeatService} from "../../shell/login/heartbeat.service";
import {DownloadService} from "./download.service";

describe("DownloadService tests", () => {

    let heartbeatService: IHeartbeatService;
    let $window: ng.IWindowService;
    let $q: ng.IQService;
    let $httpBackend: ng.IHttpBackendService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("heartbeatService", HeartbeatService);
    }));

    beforeEach(inject(( _heartbeatService_: IHeartbeatService,
                        _$q_: ng.IQService,
                        _$window_: ng.IWindowService,
                        _$httpBackend_: ng.IHttpBackendService) => {

        heartbeatService = _heartbeatService_;
        $window = _$window_;
        $q = _$q_;
        $httpBackend = _$httpBackend_;

    }));

    describe("downloadFile", () => {

        it("calls $window.open when heart beat service returns success", () => {

            $httpBackend.expectGET("/svc/adminstore/sessions/alive")
                .respond(HttpStatusCode.Success, {});

            const spyWindowOpen = spyOn($window, "open");

            const downloadService = new DownloadService($window, heartbeatService);

            downloadService.downloadFile("abc");
            $httpBackend.flush();

            expect(spyWindowOpen).toHaveBeenCalled();
        });

        it("does not call $window.open when heart beat service does not return success", () => {

            $httpBackend.expectGET("/svc/adminstore/sessions/alive")
                .respond(HttpStatusCode.Unauthorized, {});

            const spyWindowOpen = spyOn($window, "open");

            const downloadService = new DownloadService($window, heartbeatService);

            downloadService.downloadFile("abc");
            $httpBackend.flush();

            expect(spyWindowOpen).not.toHaveBeenCalled();
        });
    });
});
