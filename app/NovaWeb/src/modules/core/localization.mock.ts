import {ILocalizationService} from "./localization";
export class LocalizationServiceMock implements ILocalizationService {
    public get(name: string): string {
        return name;
    }
}
