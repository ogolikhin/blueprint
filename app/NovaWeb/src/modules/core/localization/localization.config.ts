// from http://stackoverflow.com/questions/31942788/angular-ui-datepicker-format-day-header-format-with-with-2-letters
import {LocalizationService} from "./localization.service";

LocaleConfig.$inject = ["$provide"];
export function LocaleConfig($provide: ng.auto.IProvideService): void {
    moment.locale(LocalizationService.getBrowserLanguage());

    function delegated($delegate) {
        let value = $delegate.DATETIME_FORMATS;

        value.DAY = moment.weekdays();
        value.SHORTDAY = moment.weekdaysMin().map((it: string) => {
            return it.substr(0, 1).toUpperCase();
        });
        value.MONTH = moment.months();
        value.SHORTMONTH = moment.monthsShort();

        return $delegate;
    }

    $provide.decorator("$locale", ["$delegate", delegated]);
}
