import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {BpEscapeAndHighlightFilter} from "../../shared/filters/bp-escape-highlight/bp-escape-highlight.filter";
import {formlyConfig} from "./formly-config";

export function createFormlyModule(moduleName, dependencies, formlyFields) {
    let app = angular.module(moduleName, dependencies);
    setupFormly(app);
    setupFormlyDirective(app, formlyFields);
    return app.name;

    function setupFormly(ngModule) {
        ngModule
            .service("localization", LocalizationServiceMock)
            .filter("bpEscapeAndHighlight", BpEscapeAndHighlightFilter.factory())
            .run(formlyConfig);
    }

    function setupFormlyDirective(ngModule, fields) {
        ngModule.directive("formlyDir", function formlyDir() {
            return {
                template: `
<div class="formly-dir">
    <form name="vm.form">
        <formly-form model="vm.model" fields="vm.fields" options="vm.options">
        </formly-form>
    </form>
</div>
        `,
                scope: {
                    model: "="
                },
                controllerAs: "vm",
                controller: Controller,
                bindToController: true
            };

            function Controller() {
                const vm = this;
                vm.fields = fields;
            }
        });
    }
}
