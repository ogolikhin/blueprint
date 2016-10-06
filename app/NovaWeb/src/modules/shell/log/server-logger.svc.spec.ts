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
            var success: boolean;
            var level: number = 2;
            var inError = { message: "test" };
            var outMessage = { Source: "NovaClient", LogLevel: level, Message: inError.message, StackTrace: "" };
            $httpBackend.expectPOST("/svc/adminstore/log", angular.toJson(outMessage))
                .respond(HttpStatusCode.Success);

            // Act
            serverLogger.log(inError, level).then(() => { success = true; }, () => { success = false; });
            $httpBackend.flush();

            // Assert
            expect(success).toBe(true);
        }));

        it("logs unsuccesfully", inject(($httpBackend: ng.IHttpBackendService, serverLogger: ServerLoggerSvc) => {
            // Arrange
            var success: boolean;
            var level: number = 1;
            var inError = { message: "test" };
            var outMessage = { Source: "NovaClient", LogLevel: level, Message: inError.message, StackTrace: "" };
            $httpBackend.expectPOST("/svc/adminstore/log", angular.toJson(outMessage))
                .respond(400);

            // Act
            serverLogger.log(inError, level).then(() => { success = true; }, () => { success = false; });
            $httpBackend.flush();

            // Assert
            expect(success).toBe(false);
        }));
    });
});