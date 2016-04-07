import "angular";

export interface ILocalizationService {
    get: (name: string) => string;

}
export class LocalizationService implements ILocalizationService {
    static $inject: [string] = ["$rootScope"];
    constructor(private scope: ng.IRootScopeService) {
    }
    get(name: string): string {
        return  this.scope["config"].labels[name] || "";
    }
}
