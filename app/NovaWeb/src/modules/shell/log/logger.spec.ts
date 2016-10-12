import * as angular from "angular";
import "angular-mocks";
import { IServerLogger } from "./server-logger.svc";
import { Logger } from "./logger";
import { LogMock } from "./server-logger.svc.mock";

//global buffer to check logger output
var msg: string;

export class ServerLoggerMock implements IServerLogger {
    static $inject: [string] = ["$injector"];
    constructor(private $injector: ng.auto.IInjectorService) {
    }

    log(message: any, level: number): ng.IPromise<any> {
        var $q: ng.IQService = <ng.IQService>this.$injector.get("$q");
        var deferred = $q.defer<any>();

        //log to global variable
        msg = message.message;

        deferred.resolve();
        return deferred.promise;
    }
}

describe("Logger", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("$log", LogMock);
        /* tslint:disable - we are calling this to initialize decorators */
        new Logger($provide);
        /* tslint:enable */
        $provide.service("serverLogger", ServerLoggerMock);
    }));

    describe("logger", () => {
        it("logs an error with message set", inject(($rootScope: ng.IRootScopeService, $log: ng.ILogService) => {
            // Arrange

            // Act
            $log.error({ message: "test" });
            $rootScope.$digest();

            // Assert
            expect(msg).toBe("test", "message not set correctly");
        }));

        it("logs a string error", inject(($rootScope: ng.IRootScopeService, $log: ng.ILogService) => {
            // Arrange

            // Act
            $log.error("test");
            $rootScope.$digest();

            // Assert
            expect(msg).toBe("test", "message not set correctly");
        }));
    });
});
