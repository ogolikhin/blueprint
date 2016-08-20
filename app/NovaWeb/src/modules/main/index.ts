import "angular";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "ui-select";
require("script!mxClient");
import * as agGrid from "ag-grid/main";
import * as agGridEnterprise from "ag-grid-enterprise/main";
import "ng-draggable";
import "angular-perfect-scrollbar-2";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "../shell";
import "../shared";
import "tinymce";
import "./services/";
import { MainViewComponent } from "./main.view";
import { formlyConfigExtendedFields } from "./main.formly";
import "../editors/";
import "./components";
import "./router";
import "./services";

config.$inject = ["$rootScope", "$state"];

declare var VERSION: string; //Usages replaced by webpack.DefinePlugin
declare var BUILD_YEAR: string;


export function config($rootScope: ng.IRootScopeService, $state: ng.ui.IStateService) {
    $rootScope["config"] = window["config"] || { settings: {}, labels: {} };
    $rootScope["version"] = VERSION.split(".")[0] + "." + VERSION.split(".")[1] + " (" + VERSION.replace("-", ".") + ")";
    $rootScope["year"] = BUILD_YEAR;

    let labels = $rootScope["config"].labels;
    if (!labels || (Object.keys(labels).length === 0 && labels.constructor === Object)) {
        $state.transitionTo("error");
    }

    tinymce.baseURL = "../novaweb/libs/tinymce";

//    moment.locale(Helper.getFirstBrowserLanguage());
    //console.log("language: " + moment.locale(), "date format: " + moment.localeData().longDateFormat("L"));
}

if (agGridEnterprise["LicenseManager"] && angular.isFunction(agGridEnterprise["LicenseManager"].setLicenseKey)) {
    agGridEnterprise["LicenseManager"].setLicenseKey("Blueprint_Software_Systems_Inc._19-May-2016_MTQ5NTE0ODQwMDAwMA==5e9a534267a22bce0af6682e4bbcb799");
}

agGrid.initialiseAgGridWithAngular1(angular);
angular.module("app.main", [
    "ngMessages",
    "ngSanitize",
    "app.shell",
    "app.shared",
    "ui.router",
    "ui.bootstrap",
    "ui.select",
    "ui.tinymce",
    "agGrid",
    "ngDraggable",
    "angular-perfect-scrollbar-2",
    "formly",
    "formlyBootstrap",
    "bp.editors",
    "bp.components",
    "bp.router",
    "bp.main.services"
])
    .run(config)
    .component("bpMainView", new MainViewComponent())
    .run(formlyConfigExtendedFields);

export {
    Enums,
    Models,
    Relationships
} from "./models";
export {
    IProjectRepository, ProjectRepository,
    IArtifactService, ArtifactService,
    IProjectManager, ProjectManager,
    ISelectionManager, SelectionManager, ISelection, SelectionSource,
    IWindowManager, WindowManager, IMainWindow, ResizeCause
} from "./services";
