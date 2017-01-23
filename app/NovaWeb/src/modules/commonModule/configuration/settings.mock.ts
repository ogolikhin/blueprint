import {ISettingsService} from "./settings.service";

export class SettingsServiceMock implements ISettingsService {

    get(key: string, defaultValue?: string): string {
        return defaultValue;
    }

    getNumber(key: string, defaultValue?: number, minValue?: number, maxValue?: number, strict?: boolean): number {
        return defaultValue;
    }

    getBoolean(key: string, defaultValue?: boolean, strict?: boolean): boolean {
        return defaultValue;
    }

    getObject(key: string, defaultValue?: any, strict?: boolean): any {
        return defaultValue;
    }
}
