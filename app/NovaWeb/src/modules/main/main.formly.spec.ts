import "angular";
import "angular-mocks";
import "angular-messages";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "tinymce";
import {LocalizationServiceMock} from "../core/localization.mock";
import {formlyDecorate, formlyConfigExtendedFields} from "./main.formly";

let moduleName = createModule();

describe("Formly", () => {
    beforeEach(angular.mock.module(moduleName));

    afterEach(() => {
        angular.element("body").empty();
    });

    let template = `<test-dir model="model" on-submit="onSubmit()"></test-dir>`;
    let compile, scope, element, node, isolateScope, vm;

    beforeEach(
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                compile = $compile;
                scope = $rootScope.$new();
                scope.model = {};
            }
        )
    );

    it("template should compile", function() {
        compileAndSetupStuff();

        expect(element).toBeDefined();
        expect(node).toBeDefined();
        expect(isolateScope).toBeDefined();
        expect(vm).toBeDefined();
    });

    describe("UI Datepicker", () => {
        it("should be initialized properly", function () {
            compileAndSetupStuff({model: {datepicker: "2016-08-08"}});

            let fieldNode = node.querySelector(".formly-field-datepicker");
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldInput = fieldNode.querySelector("input");
            triggerKey(angular.element(fieldInput), 27, "keyup");

            (<any>fieldScope).datepicker.open();

            expect(fieldNode).toBeDefined();
            expect(fieldScope).toBeDefined();
            expect((<any>fieldScope).datepicker.opened).toBeTruthy();
            expect((<any>fieldScope).to.clearText).toEqual("Datepicker_Clear");
            expect((<any>fieldScope).to.closeText).toEqual("Datepicker_Done");
            expect((<any>fieldScope).to.currentText).toEqual("Datepicker_Today");
        });

        it("should fail if the date is less than minDate", function () {
            compileAndSetupStuff({model: {datepicker: "2016-05-05"}});

            let fieldNode = node.querySelector(".formly-field-datepicker");
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldInput = fieldNode.querySelector("input");
            triggerKey(angular.element(fieldInput), 27, "keyup");

            expect(fieldNode).toBeDefined();
            expect(fieldScope).toBeDefined();
            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.dateIsGreaterThanMin).toBeTruthy();
        });

        it("should fail if the date is greater than maxDate", function () {
            compileAndSetupStuff({model: {datepicker: "2016-10-10"}});

            let fieldNode = node.querySelector(".formly-field-datepicker");
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldInput = fieldNode.querySelector("input");
            triggerKey(angular.element(fieldInput), 27, "keyup");

            expect(fieldNode).toBeDefined();
            expect(fieldScope).toBeDefined();
            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.dateIsLessThanMax).toBeTruthy();
        });

        it("should fail if the date is empty", function () {
            compileAndSetupStuff({model: {datepicker: ""}});

            let fieldNode = node.querySelector(".formly-field-datepicker");
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldInput = fieldNode.querySelector("input");
            triggerKey(angular.element(fieldInput), 27, "keyup");

            expect(fieldNode).toBeDefined();
            expect(fieldScope).toBeDefined();
            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$viewValue.toLowerCase()).toContain("invalid");
        });
    });

    function compileAndSetupStuff(extraScopeProps?) {
        angular.merge(scope, extraScopeProps);
        element = compile(template)(scope);
        angular.element("body").append(element);
        scope.$digest();
        node = element[0];
        isolateScope = element.isolateScope();
        vm = isolateScope.vm;
    }

    function triggerKey(targetElement, keyCode, eventType) {
        let e = angular.element.Event(eventType);
        e.which = keyCode;
        targetElement.trigger(e);
    }
});

function createModule() {
    let app = angular.module("formlyModule", ["formly", "formlyBootstrap"]);
    setupFormly(app);
    setupDirectiveThatUsesFormly(app);
    return app.name;

    function setupFormly(ngModule) {
        ngModule
            .service("localization", LocalizationServiceMock)
            .config(formlyDecorate)
            .run(formlyConfigExtendedFields);
    }

    function setupDirectiveThatUsesFormly(ngModule) {
        ngModule.directive("testDir", function azDir() {
            return {
                template: `
          <div class="test-dir">
            <form name="vm.form" ng-submit="vm.onSubmit()">
              <formly-form model="vm.model" fields="vm.fields" options="vm.options">
                <button type="submit" ng-disabled="vm.form.$invalid">Submit</button>
              </formly-form>
            </form>
          </div>
        `,
                scope: {
                    model: "=",
                    onSubmit: "&"
                },
                controllerAs: "vm",
                controller: TestDirCtrl,
                bindToController: true
            };

            function TestDirCtrl() {
                var vm = this;

                // function assignment
                vm.submit = submit;

                // vm assignment
                vm.options = {};
                vm.fields = getFields();

                function submit() {
                    if (vm.form.$valid) {
                        vm.onSubmit();
                    }
                }

                function getFields() {
                    return [
                        {
                            type: "tinymce",
                            key: "tinymce"
                        },
                        {
                            type: "tinymceInline",
                            key: "tinymceInline"
                        },
                        {
                            type: "datepicker",
                            key: "datepicker",
                            templateOptions: {
                                datepickerOptions: {
                                    maxDate: "2016-09-09",
                                    minDate: "2016-07-07"
                                }
                            }
                        }
                    ];
                }
            }
        });
    }
}