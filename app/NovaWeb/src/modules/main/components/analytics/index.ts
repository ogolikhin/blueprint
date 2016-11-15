import {AnalyticsConfig} from "./analyticsConfig";
import {AnalyticsRun} from "./analyticsRun";
angular.module("bp.components.analytics", [])
    .config(AnalyticsConfig)
    .run(AnalyticsRun);
