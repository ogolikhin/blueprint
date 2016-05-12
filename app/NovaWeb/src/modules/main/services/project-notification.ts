import "angular";

export interface IProjectNotification {
    subscribeToOpenProject(func: Function);
    notifyOpenProject(...prms: any[]) 
}

export class ProjectNotification   {

    static $inject: [string] = ["$rootScope"];
    constructor(private root: ng.IRootScopeService) {
    }

    private subscribe(name: string, callback: any) : Function {
        return this.root.$on(name, callback);
    };

    private notify(name: string, ...prms: any[]) {
        this.root.$emit.apply(this.root,[name].concat(prms));
    }

    public subscribeToOpenProject(func: Function) {
        var method = this.subscribe("openproject", func);
        return method;
    }
    public notifyOpenProject(...prms:any[]) {
        this.notify.apply(this, ["openproject"].concat(prms));
    }

};

