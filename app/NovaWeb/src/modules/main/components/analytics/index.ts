import {AnalyticsConfig} from "./analyticsConfig";
import {AnalyticsProvider} from "./analyticsProvider";
import {KeenTrackEvent} from "./analyticsDirective";
import {KeenIO} from "./keenIoConstant";
import {AnalyticsRun} from "./analyticsRun";

angular.module("bp.components.analytics", [])
    .provider("Analytics", AnalyticsProvider)
    .directive("keenTrackEvent", KeenTrackEvent.instance())
    .constant("KeenIO", KeenIO.Default)
    .config(AnalyticsConfig)
    .run(AnalyticsRun);
