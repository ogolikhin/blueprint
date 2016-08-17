import "angular";

export interface ISettingsService {
    /**
     * Returns the string value of a setting
     * @param key The name of the setting
     * @param defaultValue The value to return if the setting does not exist
     * @returns The value of the setting, or defaultValue if it does not exist
     */
    get(key: string, defaultValue?: string): string;

    /**
     * Returns the value of a setting as a number. Numbers must be stored in JSON format
     * (i.e. base 10, floating point with optional exponent)
     * @param key The name of the setting
     * @param defaultValue The value to return if the setting does not exist
     * @param minValue The minimum value that should be returned
     * @param maxValue The maximum value that should be returned
     * @param strict If true, throws an Error if the value is not a valid number, otherwise returns defaultValue
     * @returns The value of the setting as a number, or defaultValue, minValue or maxValue if applicable
     */
    getNumber(key: string, defaultValue?: number, minValue?: number | string, maxValue?: number | string, strict?: boolean): number;

    /**
     * Returns the value of a setting as a boolean. Booleans must be stored in JSON format
     * (i.e. either "true" or "false", all lower case).
     * @param key The name of the setting
     * @param defaultValue The value to return if the setting does not exist
     * @param strict If true, throws an Error if the value is not a valid boolean, otherwise returns defaultValue
     * @returns The value of the setting as a boolean, or defaultValue if it does not exist
     */
    getBoolean(key: string, defaultValue?: boolean, strict?: boolean): boolean;

    /**
     * Returns the value of a setting as an object. Objects must be stored in JSON format.
     * @param key The name of the setting
     * @param defaultValue The value to return if the setting does not exist
     * @param strict If true, throws an Error if the value is not a valid object, otherwise returns defaultValue
     * @returns The value of the setting as an object, or defaultValue if it does not exist
     */
    getObject(key: string, defaultValue?: any, strict?: boolean): any;
}

export class SettingsService implements ISettingsService {
    static $inject: [string] = ["$rootScope"];
    constructor(private scope: ng.IRootScopeService) {
    }

    get(key: string, defaultValue?: string): string {
        return this.scope["config"].settings[key] || defaultValue;
    }

    getNumber(key: string, defaultValue?: number, minValue?: number, maxValue?: number, strict?: boolean): number {
        let value = this.get(key);
        if (value) {
            if (/^-?(0|[1-9]\d*)(\.\d+)?([eE][+-]?\d+)?$/.test(value)) {
                let numberValue = parseFloat(value);
                if (minValue && numberValue < minValue) {
                    numberValue = minValue;
                }
                if (maxValue && numberValue > maxValue) {
                    numberValue = maxValue;
                }
                return numberValue;
            }
            if (strict) {
                throw Error(`Value '${value}' for key '${key}' is not a valid number`);
            }
        }
        return defaultValue;
    }

    getBoolean(key: string, defaultValue?: boolean, strict?: boolean): boolean {
        let value = this.get(key);
        if (value) {
            if (value === "true") {
                return true;
            }
            if (value === "false") {
                return false;
            }
            if (strict) {
                throw Error(`Value '${value}' for key '${key}' is not a valid boolean`);
            }
        }
        return defaultValue;
    }

    getObject(key: string, defaultValue?: any, strict?: boolean): any {
        let value = this.get(key);
        if (value) {
            try {
                return JSON.parse(value);
            } catch (e) {
                if (strict) {
                    throw Error(`Value '${value}' for key '${key}' is not a valid object`);
                }
            }
        }
        return defaultValue;
    }
}
