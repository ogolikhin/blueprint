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
//import * as moment from "moment";
import * as Enums from "./models/enums";
import * as Models from "./models/models";
import { IArtifactService, ArtifactService,
    ProjectRepository, IProjectRepository,
    IProjectManager, ProjectManager,
    IWindowManager, WindowManager, IMainWindow, ResizeCause } from "./services/";
import { ISelectionManager, SelectionManager, ISelection } from "./services/selection-manager";
import * as Relationships from "./models/relationshipModels";
import { PageContent } from "./components/content/pagecontent";
import { BPToolbar } from "./components/bp-toolbar/bp-toolbar";
import { BpSidebarLayout } from "./components/bp-sidebar-layout/bp-sidebar-layout";
import { BpAccordion } from "./components/bp-accordion/bp-accordion";
import { BpAccordionPanel } from "./components/bp-accordion/bp-accordion";
import { ProjectExplorer } from "./components/projectexplorer/project-explorer";
import { MainViewComponent } from "./main.view";
import { BpArtifactInfo } from "./components/bp-artifact-info/bp-artifact-info";
import { formlyConfigExtendedFields } from "./main.formly";
import "../editors/";
import {ArtifactStateController} from "./router/artifact.state";

import {DetailsStateController} from "./router/editor-states/details.state";
import {DiagramStateController} from "./router/editor-states/diagram.state";
import {GeneralStateController} from "./router/editor-states/general.state";
import {GlossaryStateController} from "./router/editor-states/glossary.state";
import {ProcessStateController} from "./router/editor-states/process.state"; 

import {Routes} from "./router/router.config";

config.$inject = ["$rootScope", "$state"];
export {
    Enums,
    Models,
    Relationships,
    IProjectRepository, ProjectRepository,
    IArtifactService, ArtifactService,
    IProjectManager, ProjectManager,
    ISelectionManager, SelectionManager, ISelection,
    IWindowManager, WindowManager, IMainWindow, ResizeCause
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
    "bp.editors"
])
    .run(config)
    .service("projectRepository", ProjectRepository)
    .service("projectManager", ProjectManager)
    .service("selectionManager", SelectionManager)
    .service("artifactService", ArtifactService)
    .service("windowManager", WindowManager)
    .component("bpMainView", new MainViewComponent())
    .component("pagecontent", new PageContent())
    .component("bpToolbar", new BPToolbar())
    .component("bpSidebarLayout", new BpSidebarLayout())
    .component("bpAccordion", new BpAccordion())
    .component("bpAccordionPanel", new BpAccordionPanel())
    .component("bpProjectExplorer", new ProjectExplorer())
    .component("bpArtifactInfo", new BpArtifactInfo())
    .controller("artifactStateController", ArtifactStateController)
    .controller("generalStateController", GeneralStateController)
    .controller("detailsStateController", DetailsStateController)
    .controller("diagramStateController", DiagramStateController)
    .controller("glossaryStateController", GlossaryStateController)
    .controller("processStateController", ProcessStateController)
    .config(Routes)
    .run(formlyConfigExtendedFields);
