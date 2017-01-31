import * as angular from "angular";
import "angular-animate";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "ui-select";
import "angular-vs-repeat";
import * as agGrid from "ag-grid/main";
import * as agGridEnterprise from "ag-grid-enterprise/main";
import "tinymce";
import "../shell";
import "../shared";
import "../managers";
import "./services/";
import "./components";
import "./services";
import "./view";
import "angular-google-analytics";
import {formlyConfig} from "../editors/";

config.$inject = ["$rootScope", "$state", "$templateCache"];

declare let VERSION: string; //Usages replaced by webpack.DefinePlugin
declare let BUILD_YEAR: string;

export function config($rootScope: ng.IRootScopeService, $state: ng.ui.IStateService, $templateCache: ng.ITemplateCacheService) {
    $rootScope["config"] = window["config"] || {settings: {}, labels: {}};
    $rootScope["version"] = VERSION.split(".")[0] + "." + VERSION.split(".")[1] + " (" + VERSION.replace("-", ".") + ")";
    $rootScope["year"] = BUILD_YEAR;

    $templateCache.put("uib/template/modal/window.html", require("./uibModal.html"));

    let labels = $rootScope["config"].labels;
    if (!labels || (Object.keys(labels).length === 0 && labels.constructor === Object)) {
        $state.transitionTo("error");
    }

    tinymce.baseURL = "../novaweb/libs/tinymce";
}

if (agGridEnterprise["LicenseManager"] && angular.isFunction(agGridEnterprise["LicenseManager"].setLicenseKey)) {
    agGridEnterprise["LicenseManager"].setLicenseKey("Blueprint_Software_Systems_Inc._19-May-2016_MTQ5NTE0ODQwMDAwMA==5e9a534267a22bce0af6682e4bbcb799");
}

configGA.$inject = ["AnalyticsProvider"];
export function configGA(AnalyticsProvider: any) {
    let gATrackingCode = window["config"].settings.GATrackingCode;
    if (gATrackingCode) {
        AnalyticsProvider.setPageEvent("$stateChangeSuccess");
        AnalyticsProvider.setAccount(gATrackingCode);
    }
}

agGrid.initialiseAgGridWithAngular1(angular);
angular.module("app.main", [
    "ngAnimate",
    "ngMessages",
    "ngSanitize",
    "vs-repeat",
    "app.shell",
    "app.shared",
    "ui.router",
    "ui.bootstrap",
    "ui.select",
    "ui.tinymce",
    "agGrid",
    "bp.managers",
    "bp.editors",
    "bp.components",
    "bp.main.services",
    "bp.main.view",
    "angular-google-analytics"
])
.config (configGA)
    .run(config)
    .run(formlyConfig)
    .run(["Analytics", function(Analytics) { let a = 3; }]);

export {
    Enums,
    Models,
    Relationships
} from "./models";
export {
    IWindowManager, WindowManager, IMainWindow, ResizeCause
} from "./services";
