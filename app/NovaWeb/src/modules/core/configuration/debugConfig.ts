declare const ENABLE_LOG: boolean; //Usages replaced by webpack.DefinePlugin

export class DebugConfig {
    static $inject = [
        "$logProvider",
        "$provide",
        "$compileProvider"
    ];

    constructor($logProvider: ng.ILogProvider,
                $provide: ng.auto.IProvideService,
                $compileProvider: ng.ICompileProvider | any) {

        $logProvider.debugEnabled(ENABLE_LOG);

        if (ENABLE_LOG) {
            $compileProvider.debugInfoEnabled(false);
            $compileProvider.commentDirectivesEnabled(false);
            $compileProvider.cssClassDirectivesEnabled(false);
        }

        // Override the default behavior in case debug is disabled
        $provide.decorator("$log", ["$delegate", ($delegate: any) => {
            const origInfo = $delegate.info;
            const origLog = $delegate.log;

            $delegate.info = function() {
                if ($logProvider.debugEnabled()) {
                    origInfo.apply(null, arguments);
                }
            };

            $delegate.log = function () {
                if ($logProvider.debugEnabled()) {
                    origLog.apply(null, arguments);
                }
            };

            return $delegate;
        }]);
    }
}
