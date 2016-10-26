export class QuickSearchComponent implements ng.IComponentOptions {
    /*if we need to inject dependencies into the directive class we do it the following way*/
    public bindings: any;
    public controller: any;
    public controllerAs: any;
    public template: string;

    constructor() {
        this.bindings = {};
        this.template = require("./quickSearch.html");
        this.controllerAs = "quickSearch";
        this.controller = "quickSearchController";
    }
}
