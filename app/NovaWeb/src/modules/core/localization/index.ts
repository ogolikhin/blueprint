import "angular";
import * as moment from "moment";

export interface ILocalizationService {
    get: (name: string, defaultValue?: string) => string;
    current: ILocaleFormat;
    toLocaleNumber(number: number): string;
    parseLocaleNumber(numberAsAny: any): number;
    toStartOfTZDay(date: Date): Date;
}
export interface ILocaleFormat {
    locale: string;
    longDateFormat: string;
    datePickerDayTitle: string;
    datePickerFormat: string;
    decimalSeparator: string;

}

// from http://stackoverflow.com/questions/31942788/angular-ui-datepicker-format-day-header-format-with-with-2-letters
localeConfig.$inject = ["$provide"];
export function localeConfig($provide: ng.auto.IProvideService): void {
    moment.locale(LocalizationService.getBrowserLanguage());

    function delegated($delegate) {
        let value = $delegate.DATETIME_FORMATS;

        value.DAY = moment.weekdays();
        value.SHORTDAY = moment.weekdaysMin().map((it : string) => {
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
    private _locale: string;
    private language: moment.MomentLanguageData;
    private _current: ILocaleFormat;
    constructor(private scope: ng.IRootScopeService) {

        this._current = {} as ILocaleFormat;

        this._current.locale = moment.locale(LocalizationService.getBrowserLanguage());

        this._current.longDateFormat = moment.localeData().longDateFormat("L");
        this._current.datePickerDayTitle = moment.localeData().longDateFormat("LL");
        this._current.datePickerDayTitle = this._current.datePickerDayTitle.indexOf("Y") < this._current.datePickerDayTitle.indexOf("M") ? "yyyy MMMM" : "MMMM yyyy";
        this._current.datePickerFormat = this.uiDatePickerFormatAdaptor(this._current.longDateFormat);
        this._current.decimalSeparator = this.getDecimalSeparator()

    }
    get(name: string, defaultValue?: string): string {
        return this.scope["config"].labels[name] || defaultValue || name || "";
    }

    public get current(): ILocaleFormat{
        return this._current;
    }

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
    
    public parseNumber(numberAsAny: any): number {
        let number: string;
        let thousandSeparator = this._current.decimalSeparator === "." ? "," : ".";

        number = (numberAsAny || "").toString();
        number = number.replace(thousandSeparator, "");
        if (this._current.decimalSeparator !== ".") {
            number = number.replace(this._current.decimalSeparator, ".");
        }

        return parseFloat(number);
    };


    private getDecimalSeparator(): string {
        let separator = ".";
        if (Number.toLocaleString) {
            separator = (1.1).toLocaleString(this.current.locale).replace(/\d/g, "");
        }
        return separator;
    };        


    private  uiDatePickerFormatAdaptor(format: string): string {
        let adapted = format;
        //adapted = adapted.replace(/[^DMY/.-]/gi, "");
        adapted = adapted.replace(/[\u200F]/g, ""); //special case for RTL languages
        adapted = adapted.replace(/D/g, "d").replace(/Y/g, "y");

        if (adapted.length === adapted.replace(/[^dMy]/g, "").length) {
            adapted = adapted.match(/(d{1,4}|M{1,4}|y{1,4})/g).join(" ");
        }

        return adapted;
    };

    public formatNumber(number: number): string {
        if (number === null || typeof number === "undefined" || isNaN(number)) {
            return null;
        }

        let numberAsString: string = number.toString();
        
        if (number - Math.round(number) !== 0) {
            let decimalSeparator = this.getDecimalSeparator();

            if (decimalSeparator !== ".") {
                numberAsString = numberAsString.replace(".", decimalSeparator);
            }
        }

        return numberAsString;
    };

    public toLocaleNumber(number: number): string {
        if (number === null || typeof number === "undefined" || isNaN(number)) {
            return null;
        }

        let numberAsString: string = number.toString();

        if (number - Math.round(number) !== 0) {

            if (this._current.decimalSeparator !== ".") {
                numberAsString = numberAsString.replace(".", this._current.decimalSeparator);
            }
        }

        return numberAsString;
    };

    public parseLocaleNumber(numberAsAny: any ): number {
        let number: string;
        let thousandSeparator = this._current.decimalSeparator === "." ? "," : ".";

        number = (numberAsAny || "").toString();
        number = number.replace(thousandSeparator, "");
        if (this._current.decimalSeparator !== ".") {
            number = number.replace(this._current.decimalSeparator, ".");
        }

        return parseFloat(number);
    };

    public toStartOfTZDay(date: Date): Date {
        let momentDate = moment(date);
        
        if (!momentDate.isValid()) {
            return null;
        }

        let momentString = momentDate.utc().startOf("day").format("YYYY-MM-DD");

        return moment(momentString).toDate();
    };




}