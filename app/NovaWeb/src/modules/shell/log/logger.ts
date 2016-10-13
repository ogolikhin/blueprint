import "angular";
import {IServerLogger} from "./server-logger.svc";

export class Logger {
    //private static infoLevel: number = 0;
    //private static warningLevel: number = 1;
    private static errorLevel: number = 2;
    //private static debugLevel: number = 3;
    //private static criticalLevel: number = 4; //unused
    /* tslint:enable:no-unused-variable */

    public static $inject: [string] = ["$provide"];

    public constructor($provide: ng.auto.IProvideService) {
        $provide.decorator("$log", ["$delegate", "serverLogger", Logger.logWrappers]);
        $provide.decorator("$exceptionHandler", ["$delegate", "$log", Logger.exceptionHandler]);
        $provide.decorator("$window", ["$delegate", "serverLogger", Logger.windowExceptionHandler]);
    }

    private static logWrappers($delegate: ng.ILogService, logger: IServerLogger): ng.ILogService {
        (<any>$delegate).error = Logger.createLogMethodWrapper($delegate.error, Logger.errorLevel, logger);
        //(<any>$delegate).debug = Logger.createLogMethodWrapper($delegate.debug, Logger.debugLevel, logger);
        //(<any>$delegate).info = Logger.createLogMethodWrapper($delegate.info, Logger.infoLevel, logger);
        //(<any>$delegate).log = Logger.createLogMethodWrapper($delegate.log, "log");
        //(<any>$delegate).warn = Logger.createLogMethodWrapper($delegate.warn, Logger.warningLevel, logger);
        return $delegate;
    }

    private static windowExceptionHandler($delegate: ng.IWindowService, logger: IServerLogger) {
        (<any>$delegate).onerror = Logger.createLogMethodWrapper($delegate.onerror, Logger.errorLevel, logger);
        return $delegate;
    }

    private static exceptionHandler($delegate: ng.IExceptionHandlerService, $log: ng.ILogService) {
        return (exception: Error, cause?: string) => {
            $log.error(exception);
        };
    }

    private static createLogMethodWrapper(logCall: Function, level: number, serverLogger: IServerLogger): Function {
        return (...args: any[]): void => {
            if (args[0]) {
                //if we have something to log
                //post message to the server /svc/adminstore/log/js
                if (args[0].message || args[0].message === "") {
                    serverLogger.log(args[0], level);
                } else {
                    serverLogger.log({message: args[0]}, level);
                }
                // Call the original with the output prepended with formatted timestamp
                if (logCall) {
                    logCall.apply(null, args);
                }
                //} else {
                //nothing to log       
            }
        };
    }
}
