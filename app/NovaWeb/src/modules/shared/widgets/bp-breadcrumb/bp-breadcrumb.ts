import * as angular from "angular";
import {IBreadcrumbLink} from "./breadcrumb-link";

export class BPBreadcrumbComponent implements ng.IComponentOptions {
    public template: string = require("./bp-breadcrumb.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPBreadcrumbController;
    public bindings: any = {
        links: "<",
        onNavigate: "&?"
    };
}

export interface IBPBreadcrumbController {
    links: IBreadcrumbLink[];
    onNavigate: (parameter: { link: IBreadcrumbLink }) => void;
}

export class BPBreadcrumbController implements IBPBreadcrumbController {
    public links: IBreadcrumbLink[];
    public onNavigate: (parameter: { link: IBreadcrumbLink }) => void;

    public $onInit = () => {
        this.links = angular.isDefined(this.links) ? this.links : [];
    };

    public $onDestroy = () => {
        this.dispose();
    };

    public dispose() {
        delete this.links;
        delete this.onNavigate;
    }
}