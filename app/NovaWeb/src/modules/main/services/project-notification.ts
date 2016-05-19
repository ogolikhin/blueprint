import "angular";
import {NotificationService} from "../../core/notification";

export enum SubscriptionEnum {
    ProjectLoad,
    ProjectLoaded,
    ProjectChildrenLoad,
    ProjectChildrenLoaded,
    ProjectClose,
    ProjectClosed,
    CurrentProjectChanged,
}

export interface IProjectNotification  {
    subscribe(type: SubscriptionEnum, func: Function);
    unsubscribe(type: SubscriptionEnum, func: Function);
    notify(type: SubscriptionEnum, ...prms: any[]);
}

export class ProjectNotification extends NotificationService implements IProjectNotification {

    public subscribe(type: SubscriptionEnum, func: Function) {
        this.attach(SubscriptionEnum[type], func);
    }

    public unsubscribe(type: SubscriptionEnum, func: Function) {
        this.detach(SubscriptionEnum[type], func);
    }

    public notify(type: SubscriptionEnum, ...prms: any[]) {
        this.dispatch(SubscriptionEnum[type], prms);
    }
};

