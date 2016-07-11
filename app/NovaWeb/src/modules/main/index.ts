import "angular";
import "angular-sanitize";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
require("script!mxClient");
import * as agGrid from "ag-grid/main";
import * as agGridEnterprise from "ag-grid-enterprise/main";
import "ng-draggable";
import "angular-perfect-scrollbar-2";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "../shell";
import "tinymce";
import * as Enums from "./models/enums";
import {Helper} from "../core/utils/helper";
import {ProjectRepository} from "./services/project-repository";
import {IProjectManager, ProjectManager, Models} from "./services/project-manager";
import * as Relationships from "./models/relationshipModels";
import {PageContent} from "./components/content/pagecontent";
import {BPToolbar} from "./components/bp-toolbar/bp-toolbar";
import {BpSidebarLayout} from "./components/bp-sidebar-layout/bp-sidebar-layout";
import {BpAccordion} from "./components/bp-accordion/bp-accordion";
import {BpAccordionPanel} from "./components/bp-accordion/bp-accordion";
import {ProjectExplorer} from "./components/projectexplorer/project-explorer";
import {MainViewComponent} from "./main.view";
import {BpArtifactInfo} from "./components/bp-artifact/bp-artifact-info";
import {BpArtifactDetails} from "./components/bp-artifact/bp-artifact-details";
import {config as routesConfig} from "./main.state";
import {StencilService} from "./components/editors/graphic/impl/stencil.svc";
import {DiagramService} from "./components/editors/graphic/diagram.svc";
import {BPDiagram} from "./components/editors/graphic/bp-diagram";
import {BPContentSelector} from "./components/content/bp-content-selector";

config.$inject = ["$rootScope", "$state"];
export {
    Enums,
    Models,
    Relationships,
    ProjectRepository,
    IProjectManager, ProjectManager,
};


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

    if (!Helper.isFontFaceSupported() || !Helper.isWebfontAvailable("Open Sans")) {
        $state.transitionTo("error:font");
    }

    tinymce.baseURL = "../novaweb/libs/tinymce";
}

if (agGridEnterprise["LicenseManager"] && angular.isFunction(agGridEnterprise["LicenseManager"].setLicenseKey)) {
    agGridEnterprise["LicenseManager"].setLicenseKey("Blueprint_Software_Systems_Inc._19-May-2016_MTQ5NTE0ODQwMDAwMA==5e9a534267a22bce0af6682e4bbcb799");
}

agGrid.initialiseAgGridWithAngular1(angular);
angular.module("app.main", [
    "ngSanitize", "app.shell", "ui.router", "ui.bootstrap", "ui.tinymce", "agGrid", "ngDraggable", "angular-perfect-scrollbar-2", "formly", "formlyBootstrap"])
    .run(config)
    .service("projectRepository", ProjectRepository)
    .service("projectManager", ProjectManager)
    .service("stencilService", StencilService)
    .service("diagramService", DiagramService)
    .component("bpMainView", new MainViewComponent())
    .component("pagecontent", new PageContent())
    .component("bpToolbar", new BPToolbar())
    .component("bpSidebarLayout", new BpSidebarLayout())
    .component("bpAccordion", new BpAccordion())
    .component("bpAccordionPanel", new BpAccordionPanel())
    .component("bpProjectExplorer", new ProjectExplorer())
    .component("bpArtifactInfo", new BpArtifactInfo())
    .component("bpArtifactDetails", new BpArtifactDetails())
    .component("bpDiagram", new BPDiagram())
    .component("bpContentSelector", new BPContentSelector())
    .value("mxUtils", mxUtils)
    .config(routesConfig)
    .run(formlyConfigTinyMCE);

/* tslint:disable */
function formlyConfigTinyMCE(formlyConfig: AngularFormly.IFormlyConfig) {
    formlyConfig.setType({
        name: "tinymce",
        template: "<textarea ui-tinymce=\"options.data.tinymceOption\" ng-model=\"model[options.key]\" class=\"form-control form-tinymce\"></textarea>",
        wrapper: ["bootstrapLabel"]
    });
    formlyConfig.setType({
        name: "tinymceInline",
        template: "<div class=\"form-tinymce-toolbar\"></div><div ui-tinymce=\"options.data.tinymceOption\" ng-model=\"model[options.key]\" class=\"form-control form-tinymce\" perfect-scrollbar></div>",
        wrapper: ["bootstrapLabel"]
    });
}
/* tslint:enable */
formlyConfigTinyMCE.$inject = ["formlyConfig"];

