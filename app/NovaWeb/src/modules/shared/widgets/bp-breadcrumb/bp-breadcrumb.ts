import {IBreadcrumbLink} from "./breadcrumb-link";

export class BPBreadcrumbComponent implements ng.IComponentOptions {
    public template: string = require("./bp-breadcrumb.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPBreadcrumbController;
    public bindings: any = {
        links: "<"
    };
}

export interface IBPBreadcrumbController {
    links: IBreadcrumbLink[];
}

export class BPBreadcrumbController implements IBPBreadcrumbController {
    public links: IBreadcrumbLink[];

    public $onInit = () => {
        this.links = angular.isDefined(this.links) ? this.links : [];
    };

    public $onDestroy = () => {
        this.links = undefined;
    };
}
