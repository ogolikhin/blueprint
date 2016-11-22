import {SettingsService} from "./settings";
import {DebugConfig} from "./debugConfig";

angular.module("bp.core.configuration", [])
    .service("settings", SettingsService)
    .config(DebugConfig);
