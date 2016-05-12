import "angular";

export class NotificationService  {
    static $inject: [string] = ["$rootScope"];

    constructor(private root: ng.IRootScopeService) {
    }
    public subscribe(name: string, scope: ng.IScope, callback: any) {
        var handler = this.root.$on(name, callback);
        scope.$on("$destroy", handler);
    };

    public notify(name: string, ...prms: any[]) {
        this.root.$emit(name, prms);
    }

};

