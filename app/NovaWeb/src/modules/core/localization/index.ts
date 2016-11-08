import {LocalizationService, localeConfig} from "./localizationService";


angular.module("bp.core.localization", [])
    .service("localization", LocalizationService)
    .config(localeConfig);

