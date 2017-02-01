require("./jobs.scss");

import {JobsService} from "./jobs.service";
import {JobsComponent} from "./jobs.component";

export const JobsEditor = angular.module("jobsEditor", [])
    .service("jobsService", JobsService)
    .component("jobs", new JobsComponent()).name;