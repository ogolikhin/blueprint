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
    CurrentArtifactChanged,
}

export interface IProjectNotification  {
    subscribe(type: SubscriptionEnum, func: Function);
    unsubscribe(type: SubscriptionEnum, func: Function);
    notify(type: SubscriptionEnum, ...prms: any[]);
}

export class ProjectNotification extends NotificationService implements IProjectNotification {
    private hostId = "projectnotificator"
    public subscribe(type: SubscriptionEnum, func: Function) {
        this.attach(this.hostId, SubscriptionEnum[type], func);
    }

    public unsubscribe(type: SubscriptionEnum, func: Function) {
        this.detach(this.hostId, SubscriptionEnum[type], func);
    }

    public notify(type: SubscriptionEnum, ...prms: any[]) {
        this.dispatch(this.hostId, SubscriptionEnum[type], ...prms);
    }
};

