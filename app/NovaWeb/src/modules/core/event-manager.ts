import "angular";

export enum EventSubscriber {
    Main,
    ProjectManager
}


export interface IEventManager {
    attach(subsriberId: EventSubscriber, name: string, callback: any);
    detach(subsriberId: EventSubscriber, name: string, callback: any);
    dispatch(subsriberId: EventSubscriber, name: string, ...prms: any[]);
    destroy(subsriberId?: EventSubscriber);
}

class ICallbacks {
    name: string;
    callbacks: Function[];
}

export class EventManager implements IEventManager {
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

    private mask(...args: string[]) {
        return args.join(`.`).toLowerCase();
    }

    public attach(subsriberId: EventSubscriber, name: string, callback: Function)  {
        if (!name || !callback) {
            return;
        }
        let handler = this.getHandlers(this.mask(EventSubscriber[subsriberId], name));
        handler.callbacks.push(callback);
    };

    public detach(subsriberId: EventSubscriber, name: string, callback: Function) {
        if (!callback) {
            return;
        }
        let handler = this.getHandlers(this.mask(EventSubscriber[subsriberId], name));
        handler.callbacks = handler.callbacks.filter(function (it: Function, index: number) {
            return it !== callback;
        });
        if (!handler.callbacks.length) {
            this.handlers = this.handlers.filter(function (it: ICallbacks) {
                return it.name !== handler.name;
            });
        }
    };

    public dispatch(subsriberId: EventSubscriber, name: string, ...prms: any[]) {
        if (!name ) {
            return;
        }
        let handler = this.getHandlers(this.mask(EventSubscriber[subsriberId], name));
        handler.callbacks.map(function (it: Function) {
            it.apply(it, prms);
        });
    }

    public destroy(subsriberId?: EventSubscriber) {
        //passing nothing or undefined host id removes all handlers
        //otherwise, removes handler by specified id if found
        let re = new RegExp(`^${this.mask(EventSubscriber[subsriberId],"")}.`);
        this.handlers = this.handlers.filter(function (it) {
            if (subsriberId || re.test(it.name)) {
                delete it.callbacks;
                return false;
            }
            return true;
                
        });
    }
};

