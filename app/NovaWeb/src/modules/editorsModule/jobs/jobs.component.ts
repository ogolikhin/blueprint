import {JobsController}  from "./jobs.controller";

export class JobsComponent implements ng.IComponentOptions {
    public template: string = require("./jobs.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = JobsController;
}
