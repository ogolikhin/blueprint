export class AnalyticsConfig {
    static $inject = [
        "AnalyticsProvider"
    ];

    constructor(AnalyticsProvider: ng.google.analytics.AnalyticsProvider,
                $injector) {
        //https://github.com/revolunet/angular-google-analytics
        let host = window.location.hostname;

        if (window.location.hostname.indexOf("localhost") > -1) {
            host = "none";
        }

        AnalyticsProvider.logAllCalls(true);
        //AnalyticsProvider.startOffline(true);
        AnalyticsProvider.setAccount("UA-87378361-1");
        AnalyticsProvider.setPageEvent("$stateChangeSuccess");
        // Set the domain name. Use "none" for localhost
        AnalyticsProvider.setDomainName(host);

        //flag that can disable analytics if needed
        //(<any>AnalyticsProvider).disableAnalytics(false);
    }
}
