//3rd party(external) library dependencies used for this module
import "angular";
import "lodash";
//internal dependencies used for this module
import {SettingsService} from "./settings.service";
import {DebugConfig} from "./debug.config";

export const ConfigurationModule = angular.module("configuration", [])
    .service("settings", SettingsService)
    .config(DebugConfig)
    .name;

//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {
    ISettingsService
} from "./settings.service";
