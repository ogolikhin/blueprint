export class AnalyticsConfig {
    static $inject = [
        "AnalyticsProvider",
        "$windowProvider"
    ];

    // 'any' is needed for 'disableAnalytics' which is not in typings yet
    constructor(AnalyticsProvider: ng.google.analytics.AnalyticsProvider | any,
                $windowProvider: ng.IServiceProvider) {

        const $window = $windowProvider.$get();
        const gATrackingCode = $window.config.settings.GATrackingCode;

        if (gATrackingCode) {
            AnalyticsProvider.setPageEvent("$stateChangeSuccess");
            AnalyticsProvider.setAccount(gATrackingCode);
        } else {
            AnalyticsProvider.startOffline(true);
            AnalyticsProvider.disableAnalytics(true);
        }
    }
}
