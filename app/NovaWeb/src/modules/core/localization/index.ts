import {LocalizationService, localeConfig} from "./localizationService"


angular.module("bp.core")
    .service("localization", LocalizationService)
    .config(localeConfig);
;
