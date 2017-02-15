import "angular-google-analytics";
import {AnalyticsConfig} from "./analyticsConfig";
import {AnalyticsRun} from "./analyticsRun";
import {AnalyticsDecorator} from "./analytics";

angular.module("bp.components.analytics", ["angular-google-analytics"])
    .config(AnalyticsDecorator)
    .config(AnalyticsConfig)
    .run(AnalyticsRun);

export {AnalyticsCategories} from "./analyticsCategories";
export {AnalyticsActions} from "./analyticsActions";
