//3rd party(external) library dependencies used for this module
import "angular";
import "lodash";
//internal dependencies used for this module
import {LocalizationService} from "./localization.service";
import {LocaleConfig} from "./localization.config";

export const Localization = angular.module("localization", [])
    .service("localization", LocalizationService)
    .config(LocaleConfig)
    .name;

//export 'API' interfaces from this module so that we can access them elsewhere in the project
export {ILocalizationService} from "./localization.service";

