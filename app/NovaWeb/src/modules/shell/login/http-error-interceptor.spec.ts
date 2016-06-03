import "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../../core/localization.mock";
import {HttpErrorInterceptor} from "./http-error-interceptor";
import {SessionSvcMock} from "./mocks.spec";

describe("HttpErrorInterceptor", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $httpProvider: ng.IHttpProvider) => {
        $provide.service("session", SessionSvcMock);
        $provide.service("httpErrorInterceptor", HttpErrorInterceptor);
    }));

    describe("responseError", () => {
        it("process 401 error from http request", inject(($rootScope: ng.IRootScopeService, $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            
            // Act
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var deferred: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            deferred.status = 401;
            httpErrorInterceptor.responseError(deferred).then(
                () => {
                    processedResponse = undefined;
                },
                (response: any) => {
                    processedResponse = response;
                }
            );
            $rootScope.$digest();

            // Assert
            expect(processedResponse).toBeDefined();
            expect(processedResponse.status).toBe(1401);
        }));

        it("process other error from http request", inject(($rootScope: ng.IRootScopeService, $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            
            // Act
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var deferred: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            deferred.status = 500;
            httpErrorInterceptor.responseError(deferred).then(
                () => {
                    processedResponse = undefined;
                },
                (response: any) => {
                    processedResponse = response;
                }
            );
            $rootScope.$digest();

            // Assert
            expect(processedResponse).toBeDefined();
            expect(processedResponse.status).toBe(500);
        }));
        
    });

    
});

