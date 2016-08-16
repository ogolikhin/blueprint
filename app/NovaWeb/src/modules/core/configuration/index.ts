import "angular";

export interface ISettingsService {
    get(key: string, defaultValue?: string): string;
    getNumber(key: string, defaultValue?: number, minValue?: number | string, maxValue?: number | string): number;
    getBoolean(key: string, defaultValue?: boolean): boolean;
    getObject(key: string, defaultValue?: any): any;
}

export class SettingsService implements ISettingsService {
    static $inject: [string] = ["$rootScope"];
    constructor(private scope: ng.IRootScopeService) {
    }

    get(key: string, defaultValue?: string): string {
        return this.scope["config"].settings[key] || defaultValue;
    }

    getNumber(key: string, defaultValue?: number, minValue?: number, maxValue?: number): number {
        let value = Number(this.get(key)) || defaultValue;
        if (minValue && value < minValue) {
            value = minValue;
        }
        if (maxValue && value > maxValue) {
            value = maxValue;
        }
        return value;
    }

    getBoolean(key: string, defaultValue?: boolean): boolean {
        return Boolean(this.get(key)) || defaultValue;
    }

    getObject(key: string, defaultValue?: any): any {
        let value = this.get(key);
        return value ? JSON.parse(value) : defaultValue;
    }
}
