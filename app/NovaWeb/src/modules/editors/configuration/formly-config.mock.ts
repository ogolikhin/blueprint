import "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { LocalizationServiceMock } from "../../core/localization/localization.mock";
//import { BpEscapeAndHighlightFilter } from "../../shared/filters/bp-escape-hightlight/bp-escape-highlight.filter";
import { formlyConfig } from "./formly-config";

export function createFormlyModule(dependencies, formlyFields) {
    let app = angular.module("formlyModule", dependencies);
    setupFormly(app);
    setupFormlyDirective(app, formlyFields);
    return app.name;

    function setupFormly(ngModule) {
        ngModule
            .service("localization", LocalizationServiceMock)
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
                var vm = this;
                vm.fields = fields;
            }
        });
    }
}