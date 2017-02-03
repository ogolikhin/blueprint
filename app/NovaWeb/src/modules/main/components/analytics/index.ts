import "angular-google-analytics";
import {AnalyticsConfig} from "./analyticsConfig";
import {AnalyticsRun} from "./analyticsRun";

angular.module("bp.components.analytics", ["angular-google-analytics"])
    .config(AnalyticsConfig)
    .run(AnalyticsRun);
