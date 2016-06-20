import "angular";
import "angular-mocks";
//import {LocalizationServiceMock} from "../../core/localization.mock";
import {HttpErrorInterceptor, HttpHandledErrorStatusCodes, IHttpInterceptorConfig} from "./http-error-interceptor";
import {SessionSvcMock} from "./mocks.spec";

describe("HttpErrorInterceptor", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $httpProvider: ng.IHttpProvider) => {
        $provide.service("session", SessionSvcMock);
        $provide.service("httpErrorInterceptor", HttpErrorInterceptor);
    }));

    describe("responseError", () => {
        it("process 401 error from http request", inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var deferred: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            deferred.status = 401;
            
            // Act
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
            expect(processedResponse.status).toBe(HttpHandledErrorStatusCodes.handledUnauthorizedStatus);
        }));

        it("process 401 error from http request that should be ignored", inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            var config = <IHttpInterceptorConfig>{ ignoreInterceptor: true };
            response.config = config;
            response.status = 401;

            // Act
            httpErrorInterceptor.responseError(response).then(
                () => {
                    processedResponse = undefined;
                },
                (resp: any) => {
                    processedResponse = resp;
                }
            );
            $rootScope.$digest();

            // Assert
            expect(processedResponse).toBeDefined();
            expect(processedResponse.status).toBe(401);
        }));

        it("process other error from http request", inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var deferred: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            deferred.status = 500;
            
            // Act
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

