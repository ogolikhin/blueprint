export interface IAnalyticsService {
    trackAnalyticsTemporalEvent(startTime: number, category: string, action: string, label: string, dimensions?: { [expr: string]: any });
    trackEvent(category: string, action: string, label: string, value?: any, nonInteractionFlag?: boolean, dimensions?: { [expr: string]: any });

}

export class AnalyticsService implements IAnalyticsService {
    static $inject = [
        "Analytics"
    ];

    constructor(private analytics: ng.google.analytics.AnalyticsService) {
    }

    public trackEvent(category: string, action: string, label: string, value?: any, nonInteractionFlag?: boolean, dimensions?: { [expr: string]: any }) {
        this.analytics.trackEvent(category, action, label, value, nonInteractionFlag, dimensions);
    }

    public trackAnalyticsTemporalEvent(startTime: number, category: string, action: string, label: string, dimensions?: { [expr: string]: any }) {
        const endTime = new Date().getTime();
        const timeSpentInMsec = endTime - startTime;
        this.analytics.trackEvent(category, action, label, timeSpentInMsec, false, dimensions);
    }
}
