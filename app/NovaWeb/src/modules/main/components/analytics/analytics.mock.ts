import {IExtendedAnalyticsService} from "./analytics";
export class AnalyticsServiceMock implements IExtendedAnalyticsService {
    public log = [];
    public offlineQueue = [];

    public set(param: string, value: string | number) {
        return "";
    }

    public trackAnalyticsTemporalEvent(startTime: number, category: string, action: string, label: string, dimensions?: { [expr: string]: any }) {
        const a = 6;
    }

    public trackEvent() {
        return "";
    }

    public registerScriptTags() {
        return true;
    }

    public registerTrackers() {
        return true;
    }

    public createScriptTag(): void {
        const scriptCreated = true;
    }

    public createAnalyticsScriptTag(): void {
        const createAnalyticsScriptTag = true;
    }

    public getUrl() {
        return "url";
    }

    public offline (offlineMode: boolean): void {
        const offlineModeConst = offlineMode;
    }

    public trackPage (pageURL: string, title?: string, dimensions?: { [expr: string]: any }): void {
        const trackPage: boolean = true;
    }

    public trackException (descrption: string, isFatal: boolean): void {
        const setDescription = descrption;
        const setFatal = isFatal;
    }
}
