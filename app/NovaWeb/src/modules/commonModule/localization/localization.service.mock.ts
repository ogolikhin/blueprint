import {LocalizationService, BPLocale} from "./localization.service";
export class LocalizationServiceMock extends LocalizationService {
    private _current = new BPLocale("en");

    public get(name: string): string {
        return name;
    }

    public get current(): BPLocale {
        return this._current;
    }

    public set current(value: BPLocale) {
        return;
    }
}
