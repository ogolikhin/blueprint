import "angular";
import {ILogger} from "./logger.svc";

export class Logger {
    private static infoLevel: number = 0;
    private static warningLevel: number = 1;
    private static errorLevel: number = 2;    
    private static debugLevel: number = 3;
    //private static criticalLevel: number = 4; //unused

    public static $inject: [string] = ["$provide"];

    public constructor($provide: ng.auto.IProvideService) {
        $provide.decorator("$log", ["$delegate", "logger", Logger.logWrappers]);
        $provide.decorator("$exceptionHandler", ["$delegate", "$log", Logger.exceptionHandler]);
       
    }

    private static logWrappers($delegate: ng.ILogService, logger: ILogger): ng.ILogService {
        (<any>$delegate).error = Logger.createLogMethodWrapper($delegate.error, Logger.errorLevel, logger);
        (<any>$delegate).debug = Logger.createLogMethodWrapper($delegate.debug, Logger.debugLevel, logger);
        (<any>$delegate).info = Logger.createLogMethodWrapper($delegate.info, Logger.infoLevel, logger);
        //(<any>$delegate).log = Logger.createLogMethodWrapper($delegate.log, "log");
        (<any>$delegate).warn = Logger.createLogMethodWrapper($delegate.warn, Logger.warningLevel, logger);

        return $delegate;
    }

    private static exceptionHandler($delegate: ng.IExceptionHandlerService, $log: ng.ILogService) {
        return (exception: Error, cause?: string) => {
            //$delegate(exception, cause);
            $log.error(exception);
        };
    }

    private static createLogMethodWrapper(logCall: Function, level: number, logger: ILogger): Function {
        return (...args: any[]): void => {
            if (args[0]) {
                //if we have something to log
                if (args[0].message) {
                    logCall.apply(null, args);
                    logger.log(args[0], level);
                } else {
                    //construct a log message
                    const now = new Date().toISOString();

                    // Prepend timestamp
                    args[0] = `${name}: ${now} - ${args[0]}`;

                    // Call the original with the output prepended with formatted timestamp
                    logCall.apply(null, args);

                    // post message to the server /svc/adminstore/log/js
                    logger.log({ message: args[0] }, level);
                }
            //} else {
                //nothing to log       
            }
        };
    }
}