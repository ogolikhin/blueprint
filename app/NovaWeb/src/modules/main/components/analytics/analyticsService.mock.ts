import {IAnalyticsService} from "./analyticsService";

export class AnalyticsServiceMock implements IAnalyticsService {

    public trackAnalyticsTemporalEvent(startTime: number, category: string, action: string, label: string, dimensions?: { [expr: string]: any }) {
        const a = 6;
    }


    public trackEvent() {
        return "";
    }
}
