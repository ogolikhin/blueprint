import "angular";
import "angular-mocks";
import "angular-messages";
import "angular-ui-bootstrap";
import "angular-ui-tinymce";
import "ui-select";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import "tinymce";
import {PrimitiveType} from "./models/enums";
import {LocalizationServiceMock} from "../core//localization/localization.mock";
import {/*formlyDecorate, */formlyConfigExtendedFields} from "./main.formly";

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

    describe("ReadOnly", () => {
        it("should be initialized properly", function () {
            compileAndSetupStuff({model: {readonlyNumber: 10}});

            let fieldNode = node.querySelector(".formly-field-bpFieldReadOnly");
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect(fieldNode).toBeDefined();
            expect(fieldScope).toBeDefined();
        });

        it("should display read only number", function () {
            compileAndSetupStuff({model: {readonlyNumber: 10}});

            let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[0];

            expect(fieldInput.innerHTML).toBe("10");
        });

        it("should display read only date", function () {
            compileAndSetupStuff({model: {readonlyDate: new Date("2016-08-08")}});

            let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[1];

            expect(fieldInput.innerHTML).toContain("2016");
        });

        it("should display read only multichoice", function () {
            compileAndSetupStuff({model: {readonlySelectMulti: [1, 2]}});

            let fieldInput = node.querySelectorAll(".formly-field-bpFieldReadOnly div.read-only-input")[2];

            expect(fieldInput.children.length).toBe(2);
        });
    });

    describe("SelectMulti", () => {
        it("should be initialized properly", function () {
            compileAndSetupStuff({model: {selectMulti: ""}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelectMulti");
            let fieldScope = angular.element(fieldNode[0]).isolateScope();
            let fieldInput = fieldNode[0].querySelector("input");
            (<any>fieldScope).bpFieldSelectMulti.scrollIntoView({target: fieldInput});

            expect(fieldNode.length).toBe(2);
            expect(fieldNode[0]).toBeDefined();
            expect(fieldScope).toBeDefined();
            expect(fieldInput).toBe(document.activeElement);
        });

        it("should fail if empty", function () {
            compileAndSetupStuff({model: {selectMulti: []}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelectMulti")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.required).toBeTruthy();
        });

        it("should succeed if empty, as not required", function () {
            compileAndSetupStuff({model: {selectMultiNotVal: []}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelectMulti")[1];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        });
    });

    describe("Text", () => {
        it("should be initialized properly", function () {
            compileAndSetupStuff({model: {text: ""}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldText");
            let fieldScope = angular.element(fieldNode[0]).isolateScope();

            expect(fieldNode.length).toBe(2);
            expect(fieldNode[0]).toBeDefined();
            expect(fieldScope).toBeDefined();
        });

        it("should fail if empty", function () {
            compileAndSetupStuff({model: {text: ""}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldText")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.required).toBeTruthy();
        });

        it("should succeed if empty, as not required", function () {
            compileAndSetupStuff({model: {textNotVal: ""}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldText")[1];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        });

        it("should allow changing the value", function () {
            compileAndSetupStuff({model: {text: "abcdefg"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldText")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldInput = fieldNode.querySelector("input");

            fieldInput.value = "hijklmno";
            angular.element(fieldInput).triggerHandler("change");

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect(scope.model.text).not.toBe("abcdefg");
            expect(scope.model.text).toBe("hijklmno");
        });

        it("should blur on Enter key", function () {
            compileAndSetupStuff({model: {text: 10}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldText")[0];
            let fieldInput = fieldNode.querySelector("input");

            fieldInput.focus();
            expect(fieldInput === document.activeElement).toBeTruthy();

            triggerKey(fieldInput, 13, "keyup");
            expect(fieldInput === document.activeElement).toBeFalsy();
        });
    });

    describe("TextMulti", () => {
        it("should be initialized properly", function () {
            compileAndSetupStuff({model: {textmulti: ""}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextMulti");
            let fieldScope = angular.element(fieldNode[0]).isolateScope();

            expect(fieldNode.length).toBe(2);
            expect(fieldNode[0]).toBeDefined();
            expect(fieldScope).toBeDefined();
        });

        it("should fail if empty", function () {
            compileAndSetupStuff({model: {textmulti: ""}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextMulti")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.required).toBeTruthy();
        });

        it("should succeed if empty, as not required", function () {
            compileAndSetupStuff({model: {textmultiNotVal: ""}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextMulti")[1];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        });

        it("should allow changing the value", function () {
            compileAndSetupStuff({model: {textmulti: "abcdefg"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldTextMulti")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldTextarea = fieldNode.querySelector("textarea");

            fieldTextarea.value = "hijklmno";
            angular.element(fieldTextarea).triggerHandler("change");

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect(scope.model.textmulti).not.toBe("abcdefg");
            expect(scope.model.textmulti).toBe("hijklmno");
        });
    });

    describe("Number", () => {
        it("should be initialized properly", function () {
            compileAndSetupStuff({model: {number: 10}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber");
            let fieldScope = angular.element(fieldNode[0]).isolateScope();

            expect(fieldNode.length).toBe(2);
            expect(fieldNode[0]).toBeDefined();
            expect(fieldScope).toBeDefined();
            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        });

        it("should fail if the number is less than min", function () {
            compileAndSetupStuff({model: {number: -100}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.min).toBeTruthy();
        });

        it("should fail if the number is greater than max", function () {
            compileAndSetupStuff({model: {number: 1000}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.max).toBeTruthy();
        });

        it("should fail if the decimals are more than allowed", function () {
            compileAndSetupStuff({model: {number: 10.1234}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.decimalPlaces).toBeTruthy();
        });

        it("should succeed if the decimals are within the allowed count", function () {
            compileAndSetupStuff({model: {number: 10.1}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        });

        it("should succeed even if greater than max and decimal count is wrong, as validation is not required", function () {
            compileAndSetupStuff({model: {numberNotVal: 1000.1234}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[1];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect((<any>fieldScope).fc.$error.max).toBeUndefined();
            expect((<any>fieldScope).fc.$error.min).toBeUndefined();
            expect((<any>fieldScope).fc.$error.decimalPlaces).toBeUndefined();
        });

        it("should allow changing the value", function () {
            compileAndSetupStuff({model: {number: 10}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldInput = fieldNode.querySelector("input");

            fieldInput.value = "20";
            angular.element(fieldInput).triggerHandler("change");

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect(scope.model.number).not.toBe(10);
            expect(scope.model.number).toBe("20");
        });

        it("should blur on Enter key", function () {
            compileAndSetupStuff({model: {number: 10}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[0];
            let fieldInput = fieldNode.querySelector("input");

            fieldInput.focus();
            expect(fieldInput === document.activeElement).toBeTruthy();

            triggerKey(fieldInput, 13, "keyup");
            expect(fieldInput === document.activeElement).toBeFalsy();
        });
    });

    describe("UI Datepicker", () => {
        it("should be initialized properly", function () {
            compileAndSetupStuff({model: {datepicker: "2016-08-08"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker");
            let fieldScope = angular.element(fieldNode[0]).isolateScope();

            expect(fieldNode.length).toBe(2);
            expect(fieldNode[0]).toBeDefined();
            expect(fieldScope).toBeDefined();
            expect((<any>fieldScope).bpFieldDatepicker.opened).toBeFalsy();
            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        });


        it("should open the datepicker popup", function () {
            compileAndSetupStuff({model: {datepicker: "2016-08-08"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldButton = fieldNode.querySelector("button");
            let fieldInput = fieldNode.querySelector("input");

            fieldButton.click();
            //(<any>fieldScope).bpFieldDatepicker.open();
            let selection = fieldInput.value.substring(fieldInput.selectionStart, fieldInput.selectionEnd);

            expect((<any>fieldScope).bpFieldDatepicker.opened).toBeTruthy();
            expect((<any>fieldScope).to.clearText).toEqual("Datepicker_Clear");
            expect((<any>fieldScope).to.closeText).toEqual("Datepicker_Done");
            expect((<any>fieldScope).to.currentText).toEqual("Datepicker_Today");
            expect(selection).toEqual("");
        });

        it("should select the text on click", function () {
            compileAndSetupStuff({model: {datepicker: "2016-08-08"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldInput = fieldNode.querySelector("input");

            fieldInput.click();
            //(<any>fieldScope).bpFieldDatepicker.select();
            let selection = fieldInput.value.substring(fieldInput.selectionStart, fieldInput.selectionEnd);

            expect((<any>fieldScope).bpFieldDatepicker.opened).toBeFalsy();
            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect(selection).toContain("2016");
        });

        it("should allow changing the value", function () {
            compileAndSetupStuff({model: {datepicker: "2016-08-08"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();
            let fieldInput = fieldNode.querySelector("input");

            fieldInput.value = "2017-06-06";
            angular.element(fieldInput).triggerHandler("change");

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect(scope.model.datepicker).not.toBe("2016-08-08");
            expect(scope.model.datepicker).toBe("2017-06-06");
        });

        it("should fail if the date is less than minDate", function () {
            compileAndSetupStuff({model: {datepicker: "2014-05-05"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.minDate).toBeTruthy();
        });

        it("should fail if the date is greater than maxDate", function () {
            compileAndSetupStuff({model: {datepicker: "2018-10-10"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
            expect((<any>fieldScope).fc.$error.maxDate).toBeTruthy();
        });

        it("should succeed even if the date is greater than maxDate, as validation is not required", function () {
            compileAndSetupStuff({model: {datepickerNotVal: "2018-10-10"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker")[1];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeTruthy();
            expect((<any>fieldScope).fc.$invalid).toBeFalsy();
            expect((<any>fieldScope).fc.$error.maxDate).toBeUndefined();
            expect((<any>fieldScope).fc.$error.minDate).toBeUndefined();
        });

        it("should fail if the date is empty", function () {
            compileAndSetupStuff({model: {datepicker: ""}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker")[0];
            let fieldScope = angular.element(fieldNode).isolateScope();

            expect((<any>fieldScope).fc.$valid).toBeFalsy();
            expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        });

        it("should blur on Enter key", function () {
            compileAndSetupStuff({model: {datepicker: "2016-08-08"}});

            let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker")[0];
            let fieldInput = fieldNode.querySelector("input");

            fieldInput.focus();
            expect(fieldInput === document.activeElement).toBeTruthy();

            triggerKey(fieldInput, 13, "keyup");
            expect(fieldInput === document.activeElement).toBeFalsy();
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
        angular.element(targetElement).trigger(e);
    }
});

function createModule() {
    let app = angular.module("formlyModule", [
        //"ui.bootstrap",
        "ui.select",
        "ui.tinymce",
        "formly",
        "formlyBootstrap"
    ]);
    setupFormly(app);
    setupDirectiveThatUsesFormly(app);
    return app.name;

    function setupFormly(ngModule) {
        ngModule
            .service("localization", LocalizationServiceMock)
            //.config(formlyDecorate)
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
                //vm.options = {};
                vm.fields = getFields();

                function submit() {
                    if (vm.form.$valid) {
                        vm.onSubmit();
                    }
                }

                function getFields() {
                    return [
                        {
                            type: "bpFieldReadOnly",
                            key: "readonlyNumber",
                            data: {
                                primitiveType: PrimitiveType.Number
                            }
                        },
                        {
                            type: "bpFieldReadOnly",
                            key: "readonlyDate",
                            data: {
                                primitiveType: PrimitiveType.Date
                            }
                        },
                        {
                            type: "bpFieldReadOnly",
                            key: "readonlySelectMulti",
                            data: {
                                primitiveType: PrimitiveType.Choice
                            },
                            templateOptions: {
                                options: [
                                    { value: 1, name: "Option 1" },
                                    { value: 2, name: "Option 2" },
                                    { value: 3, name: "Option 3" },
                                    { value: 4, name: "Option 4" },
                                    { value: 5, name: "Option 5" }
                                ]
                            }
                        },
                        {
                            type: "bpFieldTinymce",
                            key: "tinymce"
                        },
                        {
                            type: "bpFieldInlineTinymce",
                            key: "inlineTinymce"
                        },
                        {
                            type: "bpFieldSelectMulti",
                            key: "selectMulti",
                            templateOptions: {
                                options: [
                                    { value: 1, name: "Option 1" },
                                    { value: 2, name: "Option 2" },
                                    { value: 3, name: "Option 3" },
                                    { value: 4, name: "Option 4" },
                                    { value: 5, name: "Option 5" }
                                ],
                                optionsAttr: "bs-options",
                                required: true
                            }
                        },
                        {
                            type: "bpFieldSelectMulti",
                            key: "selectMultiNotVal",
                            templateOptions: {
                                options: [
                                    { value: 10, name: "Option 10" },
                                    { value: 20, name: "Option 20" },
                                    { value: 30, name: "Option 30" },
                                    { value: 40, name: "Option 40" },
                                    { value: 50, name: "Option 50" }
                                ],
                                optionsAttr: "bs-options"
                            }
                        },
                        {
                            type: "bpFieldText",
                            key: "text",
                            templateOptions: {
                                required: true
                            }
                        },
                        {
                            type: "bpFieldText",
                            key: "textNotVal"
                        },
                        {
                            type: "bpFieldTextMulti",
                            key: "textmulti",
                            templateOptions: {
                                required: true
                            }
                        },
                        {
                            type: "bpFieldTextMulti",
                            key: "textmultiNotVal"
                        },
                        {
                            type: "bpFieldNumber",
                            key: "number",
                            templateOptions: {
                                min: 5,
                                max: 100,
                                decimalPlaces: 2
                            },
                            data: {
                                isValidated: true
                            }
                        },
                        {
                            type: "bpFieldNumber",
                            key: "numberNotVal",
                            templateOptions: {
                                min: 5,
                                max: 100,
                                decimalPlaces: 2
                            },
                            data: {
                                isValidated: false
                            }
                        },
                        {
                            type: "bpFieldDatepicker",
                            key: "datepicker",
                            templateOptions: {
                                required: true,
                                datepickerOptions: {
                                    maxDate: "2017-09-09",
                                    minDate: "2015-07-07"
                                }
                            },
                            data: {
                                isValidated: true
                            }
                        },
                        {
                            type: "bpFieldDatepicker",
                            key: "datepickerNotVal",
                            templateOptions: {
                                required: true,
                                datepickerOptions: {
                                    maxDate: "2017-09-09",
                                    minDate: "2015-07-07"
                                }
                            },
                            data: {
                                isValidated: false
                            }
                        }
                    ];
                }
            }
        });
    }
}