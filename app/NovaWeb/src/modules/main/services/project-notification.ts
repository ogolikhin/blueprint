import "angular";
import {INotification, NotificationService} from "../../core/notification"

export enum SubscriptionEnum {
    ProjectLoad,
    ProjectLoaded,
    ProjectNodeLoad,
    ProjectNodeLoaded,
    CurrentProjectChanged,
}

export interface IProjectNotification  {
    subscribe(type: SubscriptionEnum, func: Function);
    unsubscribe(type: SubscriptionEnum, func: Function);
    notify(type: SubscriptionEnum, ...prms: any[]) 
}

export class ProjectNotification extends NotificationService implements IProjectNotification {

    public subscribe(type: SubscriptionEnum, func: Function) {
        this.signin(SubscriptionEnum[type], func);
    }

    public unsubscribe(type: SubscriptionEnum, func: Function) {
        this.signout(SubscriptionEnum[type], func);
    }

    public notify(type: SubscriptionEnum, ...prms: any[]) {
        this.notifyTo(SubscriptionEnum[type], prms);
    }
};

