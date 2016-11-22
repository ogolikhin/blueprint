import {AnalyticsConfig} from "./analyticsConfig";
import {AnalyticsProvider} from "./analyticsProvider";
import {KeenTrackEvent} from "./analyticsDirective";
import {AnalyticsRun} from "./analyticsRun";

angular.module("bp.components.analytics", ["bp.core.configuration"])
    .provider("analytics", AnalyticsProvider)
    .directive("keenTrackEvent", KeenTrackEvent.instance())
    .config(AnalyticsConfig)
    .run(AnalyticsRun);
