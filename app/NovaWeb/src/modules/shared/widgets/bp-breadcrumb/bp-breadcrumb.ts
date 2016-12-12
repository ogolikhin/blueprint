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
    links: IBreadcrumbLink[];
    trackPath: boolean;
}

export class BPBreadcrumbController implements IBPBreadcrumbController {
    public links: IBreadcrumbLink[];
    public trackPath: boolean;

    public static $inject: [string] = [
        "navigationService"
    ];

    constructor(private navigationService: INavigationService) {
    }

    public $onInit() {
        this.links = angular.isDefined(this.links) ? this.links : [];
    }

    public $onDestroy() {
        this.links = undefined;
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
