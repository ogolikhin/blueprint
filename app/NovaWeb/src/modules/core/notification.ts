import "angular";

export interface INotification {
    signin(name: string, callback: any);
    signout(name: string, callback: any);
    dispatch(name: string, ...prms: any[]);
}

class ICallbacks {
    name: string;
    callbacks: Function[];
}

export class NotificationService implements INotification {
    static $inject: [string] = ["$rootScope"];
    public handlers: ICallbacks[] = [];

    constructor(private root: ng.IRootScopeService) {
    }

    private getHandlers(name: string): ICallbacks {
        let handler = (this.handlers.filter(function (it: ICallbacks) {
            return it.name === name;
        }) || [])[0];
        if (!handler) {
            handler = <ICallbacks>{ name: name, callbacks: [] };
            this.handlers.push(handler);
        }
        return handler;
    };

    public signin(name: string, callback: Function)  {
        let handler = this.getHandlers(name);
        handler.callbacks.push(callback);
    };

    public signout(name: string, callback: Function) {
        let handler = this.getHandlers(name);
        handler.callbacks = handler.callbacks.filter(function (it: Function, index: number) {
            return it !== callback;
        }) || [];
    };

    public dispatch(name: string, prms: any[]) {
        let handler = this.getHandlers(name);
        handler.callbacks.map(function (it: Function) {
            it.apply(it, prms);
        });
    }


    //public subscribeTo(name: string, callback: any): Function {
    //    return this.root.$on(name, callback);
    //};

    //public notifyTo(name: string, ...prms: any[]) {
    //    this.root.$emit.apply(this.root, [name].concat(prms));
    //}


};

