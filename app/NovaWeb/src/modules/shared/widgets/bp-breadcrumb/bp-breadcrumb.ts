import * as angular from "angular";
import {IBreadcrumbLink} from "./breadcrumb-link";

export class BPBreadcrumbComponent implements ng.IComponentOptions {
    public template: string = require("./bp-breadcrumb.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPBreadcrumbController;
    public bindings: any = {
        links: "<",
        onSelect: "&?"
    };
}

export interface IBPBreadcrumbController {
    links: IBreadcrumbLink[];
    onSelect?: Function;
}

export class BPBreadcrumbController implements IBPBreadcrumbController {
    public links: IBreadcrumbLink[];
    public onSelect: Function;

    private selectionSubject: Rx.Subject<IBreadcrumbLink>;

    constructor(
    ) {
        this.selectionSubject = new Rx.Subject<IBreadcrumbLink>();

        this.selectionSubject
            .filter((link: IBreadcrumbLink) => link != null && angular.isFunction(this.onSelect))
            .debounce(200)
            .subscribe((link: IBreadcrumbLink) => this.onSelect({ link: link }));
    }

    public $onInit = () => {
    };

    public $onDestroy = () => {
        this.selectionSubject.dispose();
    };

    public click(link: IBreadcrumbLink): void {
        this.selectionSubject.onNext(link);
    }
}