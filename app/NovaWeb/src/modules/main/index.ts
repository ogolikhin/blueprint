import "angular";
import "angular-sanitize";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import * as agGrid from "ag-grid/main";
import * as agGridEnterprise from "ag-grid-enterprise/main";
import "ng-draggable";
import "angular-perfect-scrollbar-2";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "../shell";
import "tinymce";
import * as Enums from "./models/enums";
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
require("script!mxClient");

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
    .component("bpMainView", new MainViewComponent())
    .component("pagecontent", new PageContent())
    .component("bpToolbar", new BPToolbar())
    .component("bpSidebarLayout", new BpSidebarLayout())
    .component("bpAccordion", new BpAccordion())
    .component("bpAccordionPanel", new BpAccordionPanel())
    .component("bpProjectExplorer", new ProjectExplorer())
    .component("bpArtifactInfo", new BpArtifactInfo())
    .component("bpArtifactDetails", new BpArtifactDetails())
    .config(routesConfig)
    .run(formlyConfigTinyMCE);


function formlyConfigTinyMCE(formlyConfig: AngularFormly.IFormlyConfig) {
    formlyConfig.setType({
        name: 'tinymce',
        template: `<textarea ui-tinymce="options.data.tinymceOption" ng-model="model[options.key]" class="form-control form-tinymce"></textarea>`,
        wrapper: ["bootstrapLabel"],
        defaultOptions: {
            data: { // using data property
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standard tinymce option
                    inline: true,
                    plugins: "advlist autolink link image paste lists charmap print noneditable",
                    fixed_toolbar_container: ".form-tinymce-toolbar"
                }
            }
        }
    });
    formlyConfig.setType({
        name: 'tinymceInline',  
        template: `<div class="form-tinymce-toolbar"></div><div ui-tinymce="options.data.tinymceOption" ng-model="model[options.key]" class="form-control form-tinymce" perfect-scrollbar></div>`,
        wrapper: ["bootstrapLabel"],
        defaultOptions: {
            data: { // using data property
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standard tinymce option
                    inline: true,
                    fixed_toolbar_container: ".form-tinymce-toolbar",
                    plugins: "advlist autolink link image paste lists charmap print noneditable", //mentions
                    //mentions: {
                    //    source: tinymceMentionsData,
                    //    delay: 100,
                    //    items: 5,
                    //    queryBy: "fullname",
                    //    insert: function (item) {
                    //        return `<a class="mceNonEditable" href="mailto:` + item.emailaddress + `" title="ID# ` + item.id + `">` + item.fullname + `</a>`;
                    //    }
                    //},
                }
            }
        }
    });
}
formlyConfigTinyMCE.$inject = ["formlyConfig"];

