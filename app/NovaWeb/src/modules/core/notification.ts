import "angular";

export interface INotificationService {
    attach(host: string, name: string, callback: any);
    detach(host: string, name: string, callback: any);
    dispatch(host: string, name: string, ...prms: any[]);
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
        }))[0];
        if (!handler) {
            handler = <ICallbacks>{ name: name, callbacks: [] };
            this.handlers.push(handler);
        }
        return handler;
    };

    private getId(...args: string[]) {
        return args.join(`.`);
    }

    public attach(host: string, name: string, callback: Function)  {
        if (!host || !name) {
            return;
        }
        let handler = this.getHandlers(this.getId(host,name));
        handler.callbacks.push(callback);
    };

    public detach(host: string, name: string, callback: Function) {
        if (!host || !name) {
            return;
        }
        let handler = this.getHandlers(this.getId(host, name));
        handler.callbacks = handler.callbacks.filter(function (it: Function, index: number) {
            return it !== callback;
        });
        if (!handler.callbacks.length) {
            this.handlers = this.handlers.filter(function (it: ICallbacks) {
                return it.name !== handler.name;
            });
        }
    };

    public dispatch(host: string, name: string, ...prms: any[]) {
        if (!host || !name) {
            return;
        }
        let handler = this.getHandlers(this.getId(host, name));
        handler.callbacks.map(function (it: Function) {
            it.apply(it, prms);
        });
    }
};

