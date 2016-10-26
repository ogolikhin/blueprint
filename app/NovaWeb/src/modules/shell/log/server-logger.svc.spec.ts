import * as angular from "angular";
import "angular-mocks";
import {HttpStatusCode} from "../../core/http";
import {ServerLoggerSvc} from "./server-logger.svc";

describe("ServerLoggerSvc", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("serverLogger", ServerLoggerSvc);
    }));

    describe("log", () => {
        it("logs succesfully", inject(($httpBackend: ng.IHttpBackendService, serverLogger: ServerLoggerSvc) => {
            // Arrange
            let success: boolean;
            const level: number = 2;
            const inError = {message: "test"};
            const outMessage = {Source: "NovaClient", LogLevel: level, Message: inError.message, StackTrace: ""};
            $httpBackend.expectPOST("/svc/adminstore/log", angular.toJson(outMessage))
                .respond(HttpStatusCode.Success);

            // Act
            serverLogger.log(inError, level).then(() => {
                success = true;
            }, () => {
                success = false;
            });
            $httpBackend.flush();

            // Assert
            expect(success).toBe(true);
        }));

        it("logs unsuccesfully", inject(($httpBackend: ng.IHttpBackendService, serverLogger: ServerLoggerSvc) => {
            // Arrange
            let success: boolean;
            const level: number = 1;
            const inError = {message: "test"};
            const outMessage = {Source: "NovaClient", LogLevel: level, Message: inError.message, StackTrace: ""};
            $httpBackend.expectPOST("/svc/adminstore/log", angular.toJson(outMessage))
                .respond(400);

            // Act
            serverLogger.log(inError, level).then(() => {
                success = true;
            }, () => {
                success = false;
            });
            $httpBackend.flush();

            // Assert
            expect(success).toBe(false);
        }));
    });
});
