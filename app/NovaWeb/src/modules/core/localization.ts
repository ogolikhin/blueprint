import "angular";

export interface ILocalizationService {
    get: (name: string, defaultValue?: string) => string;

}
export class LocalizationService implements ILocalizationService {
    static $inject: [string] = ["$rootScope"];
    constructor(private scope: ng.IRootScopeService) {
    }
    get(name: string, defaultValue?: string): string {
        return  this.scope["config"].labels[name] || defaultValue || "";
    }
}

export class LocalizationServiceMock implements ILocalizationService {
    public get(name: string): string {
        return name;
    }
}
