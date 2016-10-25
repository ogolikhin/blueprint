import * as angular from "angular";
import "angular-mocks";
import {HttpErrorInterceptor} from "./http-error-interceptor";
import {IHttpInterceptorConfig, HttpStatusCode, AppicationError} from "../../core";
import {SessionSvcMock} from "../login/mocks.spec";
import {MessageServiceMock} from "../../core/messages/message.mock";

describe("HttpErrorInterceptor", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $httpProvider: ng.IHttpProvider) => {
        $provide.service("session", SessionSvcMock);
        $provide.service("httpErrorInterceptor", HttpErrorInterceptor);
        $provide.service("messageService", MessageServiceMock);
    }));

    describe("responseError", () => {

        it("process Unauthorized error and do successfull retry", inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                                                                          $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            let response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            let config = <IHttpInterceptorConfig>{url: "/test-end-point", method: "GET"};
            response.config = config;
            response.status = HttpStatusCode.Unauthorized;
            $httpBackend.expectGET("/test-end-point").respond(HttpStatusCode.Success, "OK");

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
            expect(processedResponse.status).toBe(HttpStatusCode.Success);
        }));

        it("process Unauthorized error and do failed retry", inject(($httpBackend: ng.IHttpBackendService, $rootScope: ng.IRootScopeService,
                                                                     $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            let response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            let config = <IHttpInterceptorConfig>{url: "/test-end-point", method: "GET"};
            response.config = config;
            response.status = HttpStatusCode.Unauthorized;
            $httpBackend.expectGET("/test-end-point").respond(HttpStatusCode.ServerError, "Any Error");

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
            expect(processedResponse.status).toBe(HttpStatusCode.ServerError);
        }));

        it("process Unauthorized error from http request that should be ignored", inject(($rootScope: ng.IRootScopeService,
                                                                                          $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            let response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            let config = <IHttpInterceptorConfig>{ignoreInterceptor: true};
            response.config = config;
            response.status = HttpStatusCode.Unauthorized;

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
            expect(processedResponse.status).toBe(HttpStatusCode.Unauthorized);
        }));

        it("process Unauthorized error from http request that should not by retried", inject(($rootScope: ng.IRootScopeService,
                                                                                              $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            let response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            let config = <IHttpInterceptorConfig>{dontRetry: true};
            response.config = config;
            response.status = HttpStatusCode.Unauthorized;

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
            expect(processedResponse.status).toBe(HttpStatusCode.Unauthorized);
        }));

        it("process ServerError from http request", inject(($rootScope: ng.IRootScopeService,
                                                            $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor, 
                                                            messageService: MessageServiceMock) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            let response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            response.status = HttpStatusCode.ServerError;

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
            expect(processedResponse.data).toBeDefined();
            expect(processedResponse.data instanceof AppicationError).toBeTruthy();
            expect(messageService.messages.length).toBe(1);
            expect(messageService.messages[0].messageText).toBe("HttpError_InternalServer");
        }));

        it("process -1: Unavailbale, timeout error ", inject(($rootScope: ng.IRootScopeService,
                                                              $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor, 
                                                              messageService: MessageServiceMock) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            let response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
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
            expect(processedResponse).toBeDefined();
            expect(processedResponse.data).toBeDefined();
            expect(processedResponse.data instanceof AppicationError).toBeTruthy();
            expect(messageService.messages.length).toBe(1);
            expect(messageService.messages[0].messageText).toBe("HttpError_ServiceUnavailable");
        }));

        it("process -1: canceled by user ", inject(($rootScope: ng.IRootScopeService,
                                                    $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor, messageService: MessageServiceMock) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            let response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            let timeout: ng.IDeferred<any> = $q.defer();
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
