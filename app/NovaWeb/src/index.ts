import * as angular from "angular";
import * as Rx from "rx";

import "./modules/main/";
// load our default (non specific) css
require("./styles/main.scss");


declare let appBootstrap: any;

(function () {
    if (appBootstrap.isSupportedVersion()) {
        appBootstrap.initApp();
    }
})();
