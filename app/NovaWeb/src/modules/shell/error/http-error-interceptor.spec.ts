import "angular";
import "angular-mocks";
import { 
    HttpErrorInterceptor, 
    HttpErrorStatusCodes, 
    IHttpInterceptorConfig 

} from "./http-error-interceptor";
import { SessionSvcMock } from "../login/mocks.spec";
import {MessageServiceMock} from "../../core/messages/message.mock"

describe("HttpErrorInterceptor", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $httpProvider: ng.IHttpProvider) => {
        $provide.service("session", SessionSvcMock);
        $provide.service("httpErrorInterceptor", HttpErrorInterceptor);
        $provide.service("messageService", MessageServiceMock);
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
            expect(processedResponse.status).toBe(HttpErrorStatusCodes.Unauthorized);
        }));

        it("process 401 error and do successfull retry", inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            var config = <IHttpInterceptorConfig>{ url: "/test-end-point", method: "GET" };
            response.config = config;
            response.status = 401;
            $httpBackend.expectGET("/test-end-point").respond(200, "OK");
            
            // Act
            httpErrorInterceptor.responseError(response).then(
                (okResponse) => {
                    processedResponse = okResponse;
                },
                (errResponse: any) => {
                    processedResponse = errResponse;
                }
            );
            $httpBackend.flush();          

            // Assert
            expect(config.dontRetry).toBeTruthy();
            $httpBackend.verifyNoOutstandingRequest();
            expect(processedResponse.status).toBe(HttpErrorStatusCodes.Succsess);
        }));

        it("process 401 error and do failed retry", inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            var config = <IHttpInterceptorConfig>{ url: "/test-end-point", method: "GET" };
            response.config = config;
            response.status = 401;
            $httpBackend.expectGET("/test-end-point").respond(500, "Any Error");
            
            // Act
            httpErrorInterceptor.responseError(response).then(
                (okResponse) => {
                    processedResponse = okResponse;
                },
                (errResponse: any) => {
                    processedResponse = errResponse;
                }
            );
            $httpBackend.flush();          

            // Assert
            $httpBackend.verifyNoOutstandingRequest();
            expect(processedResponse.status).toBe(500);
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

        it("process 401 error from http request that should not by retried", inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            var config = <IHttpInterceptorConfig>{ dontRetry: true };
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

        it("process 500 error from http request", inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor, messageService: MessageServiceMock) => {
            // Arrange
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            response.status = 500;
            
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
            expect(processedResponse).toBeUndefined();
            expect(messageService.messages.length).toBe(1);
            expect(messageService.messages[0].messageText).toBe("HttpError_InternalServer");

        }));

        it("process -1: Unavailbale, timeout error ", inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor, messageService: MessageServiceMock) => {
            // Arrange
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            response.status = -1;
            
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
            expect(processedResponse).toBeUndefined();
            expect(messageService.messages.length).toBe(1);
            expect(messageService.messages[0].messageText).toBe("HttpError_ServiceUnavailable");
        }));

        it("process -1: canceled by user ", inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor, messageService: MessageServiceMock) => {
            // Arrange
            var processedResponse: ng.IHttpPromiseCallbackArg<any>;
            var response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            var timeout: ng.IDeferred<any> = $q.defer();
            timeout.resolve();
            response.config = {
                method: "GET",
                url: "",    
                timeout: timeout.promise
            } as ng.IRequestConfig;
            response.status = -1;

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
            expect(processedResponse).toBeUndefined();
            expect(messageService.messages.length).toBe(0);
        }));


        
    });

    
});

