import "angular";

export interface IExtendedAnalyticsService extends ng.google.analytics.AnalyticsService {
    trackAnalyticsTemporalEvent(startTime: number, category: string, action: string, label: string, dimensions?: { [expr: string]: any });
}

export class AnalyticsDecorator {
    public static $inject: [string] = ["$provide"];

    constructor($provide: ng.auto.IProvideService) {
        $provide.decorator("Analytics", ["$delegate",
            ($delegate) => {
                const trackAnalyticsTemporalEvent = (startTime: number, category: string, action: string,
                                                     label: string, dimensions?: { [expr: string]: any }) => {
                    const endTime = new Date().getTime();
                    const timeSpentInMsec = endTime - startTime;
                    $delegate.trackEvent(category, action, label, timeSpentInMsec, false, dimensions);
                };

                $delegate.trackAnalyticsTemporalEvent = trackAnalyticsTemporalEvent;
                return $delegate;
            }
        ]);
    }
}
