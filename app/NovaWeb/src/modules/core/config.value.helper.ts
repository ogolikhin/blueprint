import "angular";

export interface IConfigValueHelper {
    getBooleanValue(setting: string, fallBack?: boolean);
    getStringValue(setting: string, fallBack?: string);
}
export class ConfigValueHelper implements IConfigValueHelper {
    static $inject: [string] = ["$rootScope"];
    constructor(private scope: ng.IRootScopeService) {
    }

    getBooleanValue(setting: string, fallBack: boolean = undefined): boolean {
        var value: string = this.scope["config"].settings[setting];
        if (!value) {
            return fallBack;
        } else {
            value = value.toLowerCase();
            if (value === "true") {
                return true;
            } else if (value === "false") {
                return false;
            } else {
                return undefined;
            }
        }
    }

    getStringValue(setting: string, fallBack?: string) {
        var value: string = this.scope["config"].settings[setting];
        if (!value) {
            return fallBack;
        } else {
            return value;
        }
    }
}
