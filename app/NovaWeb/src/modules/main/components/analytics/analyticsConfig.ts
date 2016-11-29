import {IAnalyticsProvider, IKeenAccount} from "./analyticsProvider";
declare const KEEN_PROJECT_ID: string; //Usages replaced by webpack.DefinePlugin
declare const KEEN_WRITE_KEY: string; //Usages replaced by webpack.DefinePlugin
declare const ENABLE_LOCAL_HOST_TRACKING: boolean; //Usages replaced by webpack.DefinePlugin

export class AnalyticsConfig {
    static $inject = [
        "analyticsProvider",
        "$windowProvider"
    ];


    constructor(analyticsProvider: IAnalyticsProvider,
                $windowProvider: ng.IServiceProvider) {
        const globalWindow: any = $windowProvider.$get();

        const getConfig = (key, defaultValue) => {
            let result = defaultValue;
            if (globalWindow.config && globalWindow.config[key]) {
                result = globalWindow.config[key];
            }
            return result;
        };

        //projectID and writeKey will come for config.js later (from DB config). If these are not provided analytics wont track. will fail silently
        const keenConfig: IKeenAccount = {
            projectId: KEEN_PROJECT_ID, // String (required always)
            writeKey: KEEN_WRITE_KEY  // String (required for sending data)
        };
        analyticsProvider.setAccount(keenConfig);
        analyticsProvider.pageEvent = "$stateChangeSuccess";

        analyticsProvider.enableLocalhostTracking = ENABLE_LOCAL_HOST_TRACKING;

    }
}
