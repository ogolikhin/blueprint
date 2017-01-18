import * as moment from "moment";

/*todo: Remove this entire service as its not needed. Replace it with Angular.translate as all these functions are to be
 * filters not static functions like this
 */

export class BPLocale {
    private _locale: string;
    private _shortDateFormat: string;
    private _longDateFormat: string;
    private _systemDateFormat: string;
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
        this._systemDateFormat = "YYYY-MM-DDTHH:mm:ss";

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

    public get systemDateFormat(): string {
        return this._systemDateFormat;
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
        if (_.isNumber(value)) {
            return value;
        }

        if (_.isString(value) && value === "") {
            return null;
        }

        let ts = this.thousandSeparator === "." ? "\\." : ",";
        let ds = this.decimalSeparator === "." ? "\\." : ",";
        let expression = "^-?(?!0" + ts + ")(\\d{1,3}(" + ts + "\\d{3})*|\\d+|)";

        if (_.isNumber(fraction)) {
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
            if (stringValue.indexOf(".") >= 0) {
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
        if (_.isNumber(value)) {
            if (_.isNumber(decimals)) {
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

    public isValidDate(value: string, format: string = this._shortDateFormat): boolean {
        let d = moment(value, format, true).isValid();
        return d;
    }

    public toDate(value: string | Date, reset?: boolean, format?: string): Date {
        let d: moment.Moment;
        if (_.isDate(value)) {
            d = moment(value);
        } else {
            d = moment(String(value), format || moment.defaultFormat, !!format);
        }
        if (d.isValid()) {
            if (reset === true) {
                d.startOf("day");
            }
            return d.toDate();
        }
        return null;
    };


    public formatDate(value: Date, format?: string) {
        let d = moment(value);
        let result: string = null;
        if (d.isValid()) {
            result = d.format(format || this.systemDateFormat);
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

export interface ILocalizationService {
    get: (name: string, defaultValue?: string) => string;
    current: BPLocale;
}

export class LocalizationService implements ILocalizationService {
    public static $inject: [string] = ["$rootScope"];

    constructor(private scope: ng.IRootScopeService, locale?: string) {
        this.current = new BPLocale(locale || LocalizationService.getBrowserLanguage());
    }

    get(name: string, defaultValue?: string): string {
        let result = defaultValue || name || "";
        if (this.scope["config"] &&
            this.scope["config"].labels &&
            this.scope["config"].labels[name]) {
            result = this.scope["config"].labels[name];
        }
        return result;
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
