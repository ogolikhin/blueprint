import {IBreadcrumbLink} from "./breadcrumb-link";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {INavigationState} from "../../../core/navigation/navigation-state";

export class BPBreadcrumbComponent implements ng.IComponentOptions {
    public template: string = require("./bp-breadcrumb.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPBreadcrumbController;
    public bindings: any = {
        links: "<",
        trackPath: "<?"
    };
}

export interface IBPBreadcrumbController {
    breadcrumbs: IBreadcrumbLink[];
    trackPath: boolean;
}

export class BPBreadcrumbController implements IBPBreadcrumbController {
    public breadcrumbs: IBreadcrumbLink[];
    public trackPath: boolean;

    public static $inject: [string] = [
        "navigationService"
    ];

    constructor(private navigationService: INavigationService) {
    }

    public $onDestroy() {
        this.breadcrumbs = undefined;
    }

    public $onChanges(changesObj: any) {
        this.breadcrumbs = changesObj && changesObj.links && changesObj.links.currentValue || [];
    }

    public getNavigationParams(link: IBreadcrumbLink, pathIndex: number): INavigationState {
        const params = {
            id: link.id,
            version: link.version,
            path: undefined
        };

        if (this.trackPath) {
            params.path = this.navigationService.getNavigateBackRouterPath(pathIndex);
        }
        return params;
    }
}
