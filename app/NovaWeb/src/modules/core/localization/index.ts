import "angular";
import * as moment from "moment";

export interface ILocalizationService {
    get: (name: string, defaultValue?: string) => string;
    current: BPLocale;
}

export class BPLocale  {
    private _locale: string;
    private _shortDateFormat: string;
    private _longDateFormat: string;
    private _datePickerDayTitle: string;
    private _datePickerFormat: string;
    private _decimalSeparator: string;
    private _thousandSeparator: string;
    private _firstDayOfWeek: number;
    constructor(locale: string) {
        let format: string;
        this._locale = moment.locale(locale);

        this._decimalSeparator = ".";
        if (Number.toLocaleString) {
            this._decimalSeparator = (1.1).toLocaleString(this.locale).replace(/\d/g, "");
        }

        this._thousandSeparator = this.decimalSeparator === "." ? "," : ".";

        this._shortDateFormat = moment.localeData().longDateFormat("L");
        this._longDateFormat = this._shortDateFormat + " " + moment.localeData().longDateFormat("LT");

        format = moment.localeData().longDateFormat("LL");
        this._datePickerDayTitle = format.indexOf("Y") < format.indexOf("M") ? "yyyy MMMM" : "MMMM yyyy";

        this._datePickerFormat = this.getDatePickerFormat();

        this._firstDayOfWeek = (moment.localeData() as any).firstDayOfWeek() as number;
    }

    public get locale(): string {
        return this._locale;
    }
    public get shortDateFormat(): string {
        return this._shortDateFormat;
    }
    public get longDateFormat(): string {
        return this._longDateFormat;
    }
    public get datePickerDayTitle(): string {
        return this._datePickerDayTitle;
    }
    public get datePickerFormat(): string {
        return this._datePickerFormat;
    }
    public get decimalSeparator(): string {
        return this._decimalSeparator;
    }
    public get thousandSeparator(): string {
        return this._thousandSeparator;
    }
    public get firstDayOfWeek(): number {
        return this._firstDayOfWeek;
    }

    

    public toNumber(value: string | number, fraction?: number): number {

        if (angular.isNumber(value)) {
            return value;
        }

        let ts = this.thousandSeparator === "." ? "\\." : ",";
        let ds = this.decimalSeparator === "." ? "\\." : ",";
        let expression = "^-?(?!0" + ts + ")(\\d{1,3}(" + ts + "\\d{3})*|\\d+)";

        if (angular.isNumber(fraction)) {
            if (fraction > 0) {
                expression += "(" + ds + "\\d{1," + fraction.toString() + "})?";
            }
        } else {
            expression += "(" + ds + "\\d+)?";
        }
        expression += "$";
        
        let stringValue = String(value);
        let rx = new RegExp(expression, "g");
        if (rx.test(stringValue)) {
            stringValue = stringValue.replace(new RegExp(ts, "g"), "");
            if (this.decimalSeparator !== ".") {
                stringValue = stringValue.replace(new RegExp(ds), ".");
            }
            if (stringValue.indexOf(".") > 0) {
                return parseFloat(stringValue);
            } else {
                return parseInt(stringValue, 10);
            }
        } else {
            return null;
        }
    };

    public formatNumber(value: number | string, decimals?: number, groups?: boolean) {
        let options: Intl.NumberFormatOptions = {};
        if (angular.isNumber(value)) {
            if (angular.isNumber(decimals)) {
                options.minimumFractionDigits = 0;
                options.maximumFractionDigits = decimals;
            }
            if (groups) {
                options.useGrouping = groups;
            }

            return value.toLocaleString(this.locale, options);
        }
        return null;
    }

    public toDate(value: string | Date, reset?: boolean): Date {
        if (value) {
            let d = moment(value);
            if (d.isValid()) {
                if (reset === true) {
                    d.startOf("day");
                }
                return d.toDate();
            }
        }
        return null;
    };

    public formatDate(value: Date, format?: string) {
        let d = moment(value);
        let result: string = null;
        if (d.isValid()) {
            result = d.format(format);
        }
        return result;
    }
    public formatShortDateTime(value: Date) {
        let d = moment(value);
        let result: string = null;
        if (d.isValid()) {
            result = d.format("l LT");
        }
        return result;
    }

    private getDatePickerFormat(): string {
        let adapted = this.shortDateFormat;
        //adapted = adapted.replace(/[^DMY/.-]/gi, "");
        adapted = adapted.replace(/[\u200F]/g, ""); //special case for RTL languages
        adapted = adapted.replace(/D/g, "d").replace(/Y/g, "y");

        if (adapted.length === adapted.replace(/[^dMy]/g, "").length) {
            adapted = adapted.match(/(d{1,4}|M{1,4}|y{1,4})/g).join(" ");
        }

        return adapted;
    };

}


// from http://stackoverflow.com/questions/31942788/angular-ui-datepicker-format-day-header-format-with-with-2-letters
localeConfig.$inject = ["$provide"];
export function localeConfig($provide: ng.auto.IProvideService): void {
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
  

export class LocalizationService implements ILocalizationService {
    public static $inject: [string] = [ "$rootScope"];

    constructor(private scope: ng.IRootScopeService, locale?: string) {
        this.current = new BPLocale(locale || LocalizationService.getBrowserLanguage());
    }
    get(name: string, defaultValue?: string): string {
        return this.scope["config"].labels[name] || defaultValue || name || "";
    }

    public current: BPLocale;

    public static getBrowserLanguage(): string {
        // The most reliable way of getting the user's preferred langauge would be to read the Accept-Languages request
        // header on the server. In Chrome 32+ and Firefox 32+ that header's value is available in navigator.languages
        // In the returned array the languages are ordered by preference with the most preferred language first (see:
        // https://developer.mozilla.org/en-US/docs/Web/API/NavigatorLanguage/languages
        // For other browsers:
        // - Internet Explorer:
        //   navigator.userLanguage is the language set in Windows Control Panel / Regional Options
        //   navigator.browserLanguage returns the language of the UI of the browser and it is decided by the version of
        //   the executable installed
        //   navigator.systemLanguage gives the locale used by Windows itself
        // - Safari: uses the language set at the system level (similar to navigator.systemLanguage of IE above)
        // The order of elements in browserLanguagePropertyKeys has been set based on the above information.
        let nav = window.navigator,
            browserLanguagePropertyKeys = ["userLanguage", "systemLanguage", "language", "browserLanguage"],
            language;

        // support for HTML 5.1 "navigator.languages"
        if (Array.isArray((<any>nav).languages)) {
            for (let i = 0; i < (<any>nav).languages.length; i++) {
                language = (<any>nav).languages[i];
                if (language && language.length) {
                    return language;
                }
            }
        }

        // support for other well known properties in browsers
        for (let i = 0; i < browserLanguagePropertyKeys.length; i++) {
            language = nav[browserLanguagePropertyKeys[i]];
            if (language && language.length) {
                return language;
            }
        }

        return null;
    };

}