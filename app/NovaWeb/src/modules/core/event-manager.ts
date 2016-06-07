import "angular";
import {Helper} from "./utils/helper";

export enum EventSubscriber {
    Main,
    ProjectManager
}


export interface IEventManager {
    attach(subsriberId: EventSubscriber, name: string, callback: any): string;
    detach(subsriberId: EventSubscriber, name: string, callback: any);
    detachById(id: string);
    dispatch(subsriberId: EventSubscriber, name: string, ...prms: any[]);
    destroy(subsriberId?: EventSubscriber);
}

interface ICallback {
    id: string;
    callback: Function;
}

interface IEventHandlers {
    name: string;
    callbacks: ICallback[]; 
}

export class EventManager implements IEventManager {
    public handlers: IEventHandlers[] = [];

    constructor() { }

    private getHandlers(name: string): IEventHandlers {
        let handler = (this.handlers.filter(function (it: IEventHandlers) {
            return it.name === name;
        }))[0];
        if (!handler) {
            handler = <IEventHandlers>{ name: name, callbacks: [] };
            this.handlers.push(handler);
        }
        return handler;
    };

    private mask(...args: string[]) {
        return args.join(`.`).toLowerCase();
    }

    public attach(subsriberId: EventSubscriber, name: string, callback: Function): string  {
        if (!name || !callback) {
            return;
        }
        let handler = this.getHandlers(this.mask(EventSubscriber[subsriberId], name));
        let item = <ICallback>{ id: Helper.UID, callback: callback };
        handler.callbacks.push(item);
        return item.id;        
    };

    
    public detach(subsriberId: EventSubscriber, name: string , callback?: Function) {
        if (!callback) {
            return;
        }
        let handler = this.getHandlers(this.mask(EventSubscriber[subsriberId], name));
        handler.callbacks = handler.callbacks.filter(function (it: ICallback, index: number) {
            return it.callback !== callback;
        });
        if (!handler.callbacks.length) {
            this.handlers = this.handlers.filter(function (it: IEventHandlers) {
                return it.name !== handler.name;
            });
        }
    };
    public detachById(id: string) {
        this.handlers.forEach(function (handler: IEventHandlers) {
            handler.callbacks = handler.callbacks.filter(function (it: ICallback) {
                return it.id !== id;
            })
            if (!handler.callbacks.length) {
                this.handlers = this.handlers.filter(function (it: IEventHandlers) {
                    return it.name !== handler.name;
                });
            }
        }.bind(this));
    };

    public dispatch(subsriberId: EventSubscriber, name: string, ...prms: any[]) {
        if (!name ) {
            return;
        }
        let handler = this.getHandlers(this.mask(EventSubscriber[subsriberId], name));
        handler.callbacks.map(function (it: ICallback) {
            //console.log(`Dispatch event ${it.id}`);
            it.callback.apply(it.callback, prms);
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

