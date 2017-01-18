import {SettingsService} from "./settings.service";
import {DebugConfig} from "./debug.config";

export const ConfigurationModule = angular.module("configuration", [])
    .service("settings", SettingsService)
    .config(DebugConfig)
    .name;

export {
    ISettingsService
} from "./settings.service";
