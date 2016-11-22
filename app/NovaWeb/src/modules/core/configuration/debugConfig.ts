declare const ENABLE_LOG: string; //Usages replaced by webpack.DefinePlugin

export class DebugConfig {
    static $inject = [
        "$logProvider"
    ];

    constructor($logProvider) {
        $logProvider.debugEnabled(ENABLE_LOG);
    }
}
