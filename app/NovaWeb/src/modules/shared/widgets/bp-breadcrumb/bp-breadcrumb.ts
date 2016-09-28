import {IBreadcrumbLink} from "./breadcrumb-link";

export class BPBreadcrumbComponent implements ng.IComponentOptions {
    public template: string = require("./bp-breadcrumb.html");
    public controller: Function = BPBreadcrumbController;
    public bindings: any = {
        links: "<",
        onSelect: "&?"
    };
}

export interface IBPBreadcrumbController {
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
            .filter(link => link != null && angular.isFunction(this.onSelect))
            .subscribe(link => this.onSelect({ link: link }));
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