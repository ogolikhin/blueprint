import "angular";

export class Logger {
    public static $inject: [string] = ["$provide"];

    public constructor($provide: ng.auto.IProvideService) {
        $provide.decorator("$log", ["$delegate", Logger.logWrappers]);
        $provide.decorator("$exceptionHandler", ["$delegate", "$log", Logger.exceptionHandler]);
    }

    private static logWrappers($delegate: ng.ILogService): ng.ILogService {
        (<any>$delegate).error = Logger.createLogMethodWrapper($delegate.error, "error");
        //(<any>$delegate).debug = Logger.createLogMethodWrapper($delegate.debug, "debug");
        //(<any>$delegate).info = Logger.createLogMethodWrapper($delegate.info, "info");
        //(<any>$delegate).log = Logger.createLogMethodWrapper($delegate.log, "log");
        //(<any>$delegate).warn = Logger.createLogMethodWrapper($delegate.warn, "warn");

        return $delegate;
    }

    private static exceptionHandler($delegate: ng.IExceptionHandlerService, $log: ng.ILogService) {
        return (exception: Error, cause?: string) => {
            //$delegate(exception, cause);
            $log.error(`Exception: ${exception.name} Message: ${exception.message} Cause: ${cause}`);
        };
    }

    private static createLogMethodWrapper(logCall: Function, name: string): Function {
        return (...args: any[]): void => {
            const now = new Date().toISOString();

            // Prepend timestamp
            args[0] = `${name}: ${now} - ${args[0]}`;

            // Call the original with the output prepended with formatted timestamp
            logCall.apply(null, args);

            // TODO: post message to the server /svc/adminstore/log/js
        };
    }
}