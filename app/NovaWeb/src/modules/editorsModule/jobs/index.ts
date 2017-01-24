require("./jobs.scss");

import {JobsService} from "./jobs.svc";
import {JobsComponent} from "./jobs";

angular.module("bp.editors.jobs", [])
    .service("jobsService", JobsService)
    .component("jobs", new JobsComponent());