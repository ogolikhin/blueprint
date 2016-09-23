export interface IBPBreadcrumbController {

}

export class BPBreadcrumbController implements IBPBreadcrumbController {

    public name: string;

    constructor() {        
    }    
}

export class BPBreadcrumbComponent implements ng.IComponentOptions {
    public template: string = require("./bp-breadcrumb.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPBreadcrumbController;
    public bindings: any = {
        name: "@"
    };
}

