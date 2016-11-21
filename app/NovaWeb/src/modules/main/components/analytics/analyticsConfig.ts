import {IAnalyticsService, IKeenAccount} from "./analyticsProvider";
declare const ENABLE_LOCAL_HOST_TRACKING: boolean; //Usages replaced by webpack.DefinePlugin

export class AnalyticsConfig {
    static $inject = [
        "AnalyticsProvider"
    ];

    constructor(AnalyticsProvider: IAnalyticsService) {
        const getConfig = (key, defaultValue) => {
            const globalWindow: any = window;
            let result = defaultValue;
            if (globalWindow.config && globalWindow.config[key]) {
                result = globalWindow.config[key];
            }
            return result;
        };

        //projectID and writeKey will come for config.js later (from DB config). If these are not provided analytics wont track. will fail silently
        /* tslint:disable */
        const projectId = getConfig("keenProjectId",
            "582cb85c8db53dfda8a78767");
        const writeKey = getConfig("keenWriteKey",
            "E011AFC42952D3500532FA364DA5DC06BB962F988B2F171CB252201B357F48BCBA671F8A8E62060148129B391FE2D1B3A4E8D9BD6F0629DFF66C9C7C2C1F8F612A80E44ACDEA4F6B1408AAF403649EFF9394A399844C744E0E4F72CA204A0E13");
        /* tslint:enable */

        //we need to disabled analytics when running phantomJS as it polutes the usage stats
        const isTest = /PhantomJS/.test(window.navigator.userAgent);

        if ((projectId && writeKey) && !isTest) {
            const keenConfig: IKeenAccount = {
                projectId: projectId, // String (required always)
                writeKey: writeKey   // String (required for sending data)
            };
            AnalyticsProvider.setAccount(keenConfig);
            AnalyticsProvider.pageEvent = "$stateChangeSuccess";

            AnalyticsProvider.enableLocalhostTracking = ENABLE_LOCAL_HOST_TRACKING;
        }

    }
}
