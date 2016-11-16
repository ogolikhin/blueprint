import {IAnalyticsService, IKeenAccount} from "./analyticsProvider";
export class AnalyticsConfig {
    static $inject = [
        "AnalyticsProvider",
        "KeenIO"
    ];

    constructor(AnalyticsProvider: IAnalyticsService,
                KeenIO) {
        const keenConfig: IKeenAccount = {
            projectId: KeenIO.projectId, // String (required always)
            writeKey: KeenIO.writeKey   // String (required for sending data)
        };

        AnalyticsProvider.setAccount(keenConfig);
        AnalyticsProvider.pageEvent = "$stateChangeSuccess";
        AnalyticsProvider.enableLocalhostTracking = true;
    }
}
