import {LocalizationService} from "./localization.service";
import {LocaleConfig} from "./localization.config";

export const Localization = angular.module("localization", [])
    .service("localization", LocalizationService)
    .config(LocaleConfig)
    .name;

export {ILocalizationService} from "./localization.service";

