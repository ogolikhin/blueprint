import { LocalizationService } from "./";

export class LocalizationServiceMock extends LocalizationService {
    public get(name: string): string {
        return name;
    }
}
