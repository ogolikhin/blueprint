export class AnalyticsConfig{
    static $inject =[
        "AnalyticsProvider"
    ];
    constructor(AnalyticsProvider:ng.google.analytics.AnalyticsProvider){
        //https://github.com/revolunet/angular-google-analytics

        AnalyticsProvider.setAccount('UA-87378361-1');
        AnalyticsProvider.setPageEvent('$stateChangeSuccess');

        // Set the domain name. Use 'none' for localhost
        AnalyticsProvider.setDomainName('none');

        //flag that can disable analytics if needed
        (<any>AnalyticsProvider).disableAnalytics(false);
    }
}
