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
import * as Models from "./models/models";
import {IArtifactService, ArtifactService, ProjectRepository, IProjectManager, ProjectManager} from "./services/";
import * as Relationships from "./models/relationshipModels";
import {PageContent} from "./components/content/pagecontent";
import {BPToolbar} from "./components/bp-toolbar/bp-toolbar";
import {BpSidebarLayout} from "./components/bp-sidebar-layout/bp-sidebar-layout";
import {BpAccordion} from "./components/bp-accordion/bp-accordion";
import {BpAccordionPanel} from "./components/bp-accordion/bp-accordion";
import {ProjectExplorer} from "./components/projectexplorer/project-explorer";
import {MainViewComponent} from "./main.view";
import {BpArtifactInfo} from "./components/bp-artifact/bp-artifact-info";
import {BpArtifact} from "./components/editors/general/bp-artifact";
import {config as routesConfig} from "./main.state";
import {StencilService} from "./components/editors/graphic/impl/stencil.svc";
import {DiagramService} from "./components/editors/graphic/diagram.svc";
import {BPDiagram} from "./components/editors/graphic/bp-diagram";


config.$inject = ["$rootScope", "$state"];
export {
    Enums,
    Models,
    Relationships,
//    IProjectRepository, ProjectRepository,
    IArtifactService, ArtifactService,
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
    .service("stencilService", StencilService)
    .service("diagramService", DiagramService)
    .service("artifactService", ArtifactService)
    .component("bpMainView", new MainViewComponent())
    .component("pagecontent", new PageContent())
    .component("bpToolbar", new BPToolbar())
    .component("bpSidebarLayout", new BpSidebarLayout())
    .component("bpAccordion", new BpAccordion())
    .component("bpAccordionPanel", new BpAccordionPanel())
    .component("bpProjectExplorer", new ProjectExplorer())
    .component("bpArtifactInfo", new BpArtifactInfo())
    .component("bpArtifact", new BpArtifact())
    .component("bpDiagram", new BPDiagram())
    .value("mxUtils", mxUtils)
    .config(routesConfig)
    .config(Decorate)
    .run(formlyConfigExtendedFields);

function Decorate($provide) {
    function Delegated($delegate) {
        var value = $delegate.DATETIME_FORMATS;

        value.SHORTDAY = [
            "S",
            "M",
            "T",
            "W",
            "T",
            "F",
            "S"
        ];

        return $delegate;
    }
    Delegated.$inject = ["$delegate"];

    $provide.decorator("$locale", Delegated);
}
Decorate.$inject = ["$provide"];

/* tslint:disable */

function formlyConfigExtendedFields(formlyConfig: AngularFormly.IFormlyConfig) {
    var attributes = [
        "date-disabled",
        "custom-class",
        "show-weeks",
        "starting-day",
        "init-date",
        "min-mode",
        "max-mode",
        "format-day",
        "format-month",
        "format-year",
        "format-day-header",
        "format-day-title",
        "format-month-title",
        "year-range",
        "shortcut-propagation",
        "datepicker-popup",
        "show-button-bar",
        "current-text",
        "clear-text",
        "close-text",
        "close-on-date-selection",
        "datepicker-append-to-body"
    ];

    var bindings = [
        "datepicker-mode",
        "min-date",
        "max-date"
    ];

    var ngModelAttrs = {};

    angular.forEach(attributes, function(attr) {
        ngModelAttrs[Helper.camelCase(attr)] = {attribute: attr};
    });

    angular.forEach(bindings, function(binding) {
        ngModelAttrs[Helper.camelCase(binding)] = {bound: binding};
    });

    //console.log(ngModelAttrs);

    formlyConfig.setType({
        name: "tinymce",
        template: `<textarea ui-tinymce="options.data.tinymceOption" ng-model="model[options.key]" class="form-control form-tinymce"></textarea>`,
        wrapper: ["bootstrapLabel"],
        defaultOptions: {
            data: { // using data property
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standard tinymce option
                    plugins: "advlist autolink link image paste lists charmap print noneditable"
                }
            }
        }
    });
    formlyConfig.setType({
        name: "tinymceInline",
        template: `<div class="form-tinymce-toolbar" ng-class="options.key"></div><div ui-tinymce="options.data.tinymceOption" ng-model="model[options.key]" class="form-control form-tinymce" perfect-scrollbar></div>`,
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
    formlyConfig.setType({
        name: "datepicker",
        template: `<div class="form-datepicker input-group">
            <input type="text"
                id="{{::id}}"
                name="{{::id}}"
                ng-model="model[options.key]"
                class="form-control "
                ng-click="datepicker.open($event)"
                uib-datepicker-popup="{{to.datepickerOptions.format}}"
                is-open="datepicker.opened"
                datepicker-append-to-body="to.datepickerAppendToBody" 
                datepicker-options="to.datepickerOptions" />
            <span class="input-group-btn">
                <button type="button" class="btn btn-default" ng-click="datepicker.open($event)" ng-disabled="to.disabled"><i class="glyphicon glyphicon-calendar"></i></button>
            </span>
        </div>`,
        wrapper: ["bootstrapLabel", "bootstrapHasError"],
        defaultOptions: {
            ngModelAttrs: ngModelAttrs,
            templateOptions: {
                datepickerOptions: {
                    format: "dd/MM/yyyy",
                    formatDay: "d",
                    formatDayHeader: "EEE",
                    initDate: new Date(),
                    showWeeks: false
                },
                datepickerPopup: "dd-MMMM-yyyy",
                datepickerAppendToBody: true
            }
        },
        controller: ["$scope", function ($scope) {
            $scope.datepicker = {};

            $scope.datepicker.opened = false;

            $scope.datepicker.open = function ($event) {
                $scope.datepicker.opened = !$scope.datepicker.opened;
            };
        }]
    });
}
/* tslint:enable */
formlyConfigExtendedFields.$inject = ["formlyConfig"];

