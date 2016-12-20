import {IAnalyticsProvider, IKeenAccount} from "./analyticsProvider";

export class AnalyticsProviderMock implements IAnalyticsProvider {
    public trackPage(): string {
        return "";
    }
    public trackEvent(eventCollection, action, label?, value?, custom?, jQEvent?): string {
        return "";
    }
    public setAccount(account: IKeenAccount): void {
        //
    }
    pageEvent: string;
    enableLocalhostTracking: boolean;
}