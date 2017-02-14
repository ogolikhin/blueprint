import "angular-google-analytics";
import {AnalyticsConfig} from "./analyticsConfig";
import {AnalyticsRun} from "./analyticsRun";
import {AnalyticsService} from "./analyticsService";

angular.module("bp.components.analytics", ["angular-google-analytics"])
    .service("analyticsService", AnalyticsService)
    .config(AnalyticsConfig)
    .run(AnalyticsRun);

export {AnalyticsCategories} from "./analyticsCategories";
export {AnalyticsActions} from "./analyticsActions";
export {IAnalyticsService} from "./analyticsService";
export {AnalyticsServiceMock} from "./analyticsService.mock";
