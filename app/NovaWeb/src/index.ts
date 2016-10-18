import * as angular from "angular";
import * as Rx from "rx";
import 'es6-shim';


import "./modules/main/";
// load our default (non specific) css
import "./styles/screen.scss";

declare var appBootstrap: any;

(function () {
    if (appBootstrap.isSupportedVersion()) {
        appBootstrap.initApp();
    }
})();
