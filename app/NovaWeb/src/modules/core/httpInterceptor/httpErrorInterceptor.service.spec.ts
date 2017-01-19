﻿import "./";
import "angular-mocks";
import {IHttpInterceptorConfig} from "./";
import {HttpErrorInterceptor} from "./httpErrorInterceptor.service";
import {SessionSvcMock} from "../../shell/login/mocks.spec";
import {MessageServiceMock} from "../messages/message.mock";
import {HttpStatusCode} from "./http-status-code";
import {ApplicationError} from "../../shell/error/applicationError";

describe("Service: HttpErrorInterceptor", () => {
    beforeEach(angular.mock.module("httpInterceptor"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("session", SessionSvcMock);
        //$provide.service("httpErrorInterceptor", HttpErrorInterceptor);
        $provide.service("messageService", MessageServiceMock);
    }));

    describe("responseError", () => {

        it("process Unauthorized error and do successfull retry", inject(($httpBackend: ng.IHttpBackendService,
                                                                          $rootScope: ng.IRootScopeService,
                                                                          $q: ng.IQService,
                                                                          httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            const response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            const config = <IHttpInterceptorConfig>{url: "/test-end-point", method: "GET"};
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

        it("process Unauthorized error and do failed retry", inject(($httpBackend: ng.IHttpBackendService,
                                                                     $rootScope: ng.IRootScopeService,
                                                                     $q: ng.IQService,
                                                                     httpErrorInterceptor: HttpErrorInterceptor) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            const response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            const config = <IHttpInterceptorConfig>{url: "/test-end-point", method: "GET"};
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
            const response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            const config = <IHttpInterceptorConfig>{ignoreInterceptor: true};
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
            const response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            const config = <IHttpInterceptorConfig>{dontRetry: true};
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
            const response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
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
            expect(processedResponse.data instanceof ApplicationError).toBeTruthy();
            expect(messageService.messages.length).toBe(1);
            expect(messageService.messages[0].messageText).toBe("HttpError_InternalServer");
        }));

        it("process -1: Unavailbale, timeout error ", inject(($rootScope: ng.IRootScopeService,
                                                              $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor,
                                                              messageService: MessageServiceMock) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            const response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
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
            expect(processedResponse.data instanceof ApplicationError).toBeTruthy();
            expect(messageService.messages.length).toBe(1);
            expect(messageService.messages[0].messageText).toBe("HttpError_ServiceUnavailable");
        }));

        it("process -1: canceled by user ", inject(($rootScope: ng.IRootScopeService,
                                                    $q: ng.IQService, httpErrorInterceptor: HttpErrorInterceptor,
                                                    messageService: MessageServiceMock) => {
            // Arrange
            let processedResponse: ng.IHttpPromiseCallbackArg<any>;
            const response: ng.IHttpPromiseCallbackArg<any> = $q.defer();
            const timeout: ng.IDeferred<any> = $q.defer();
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
            expect(processedResponse).toBeDefined();
            expect(messageService.messages.length).toBe(0);
        }));
    });
});
