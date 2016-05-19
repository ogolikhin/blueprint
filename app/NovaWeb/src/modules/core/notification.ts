import "angular";

export interface INotificationService {
    attach(name: string, callback: any);
    detach(name: string, callback: any);
    dispatch(name: string, ...prms: any[]);
}

class ICallbacks {
    name: string;
    callbacks: Function[];
}

export class NotificationService implements INotificationService {
    public handlers: ICallbacks[] = [];

    constructor() { }

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

    public attach(name: string, callback: Function)  {
        let handler = this.getHandlers(name);
        handler.callbacks.push(callback);
    };

    public detach(name: string, callback: Function) {
        let handler = this.getHandlers(name);
        handler.callbacks = handler.callbacks.filter(function (it: Function, index: number) {
            return it !== callback;
        });
        if (!handler.callbacks.length) {
            this.handlers = this.handlers.filter(function (it: ICallbacks) {
                return it.name !== handler.name;
            });
        }
    };

    public dispatch(name: string, ...prms: any[]) {
        let handler = this.getHandlers(name);
        handler.callbacks.map(function (it: Function) {
            it.apply(it, prms);
        });
    }
};

