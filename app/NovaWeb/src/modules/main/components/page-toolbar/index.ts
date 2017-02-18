import "./page-toolbar.scss";

import {PageToolbar} from "./page-toolbar";

angular.module("bp.components.pagetoolbar", ["bp.components.explorer"])
    .component("pageToolbar", new PageToolbar());
