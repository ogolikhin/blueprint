export interface IAnalyticsService {
    trackAnalyticsTemporalEvent(startTime: number, category: string, action: string, label: string, dimensions?: { [expr: string]: any });
}

export class AnalyticsService implements IAnalyticsService {
    static $inject = [
        "Analytics"
    ];
    
    constructor(private analytics: ng.google.analytics.AnalyticsService) {        
    }

    public trackAnalyticsTemporalEvent(startTime: number, category: string, action: string, label: string, dimensions?: { [expr: string]: any }) {
        const endTime = new Date().getTime();
        const timeSpentInMsec = endTime - startTime;    
        const seconds = timeSpentInMsec / 1000;
        this.analytics.trackEvent(category, action, label, seconds, false, dimensions);
    }
}
