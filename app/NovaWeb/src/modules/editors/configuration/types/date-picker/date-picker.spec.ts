import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "ui-select";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { createFormlyModule } from "../../formly-config.mock";

describe("Formly Date Picker", () => {
    let fieldsDefinition = [
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

    let moduleName = createFormlyModule("formlyModuleDatePicker", [
        "ngSanitize",
        "ui.select",
        "formly",
        "formlyBootstrap"
    ], fieldsDefinition);

    beforeEach(angular.mock.module(moduleName));

    afterEach(() => {
        angular.element("body").empty();
    });

    let template = `<formly-dir model="model"></formly-dir>`;
    let compile, scope, rootScope, element, node, isolateScope, vm;

    beforeEach(
        inject(
            ($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
                rootScope = $rootScope;
                compile = $compile;
                scope = rootScope.$new();
                scope.model = {};
            }
        )
    );

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

    it("should fail if the date is less than 1753-01-01 (SQL starting date), even if validation is not required", function () {
        compileAndSetupStuff({model: {datepickerNotVal: "1752-10-10"}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldDatepicker")[1];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.maxDate).toBeUndefined();
        expect((<any>fieldScope).fc.$error.minDate).toBeUndefined();
        expect((<any>fieldScope).fc.$error.minDateSQL).toBeTruthy();
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

    function compileAndSetupStuff(extraScopeProps?) {
        angular.merge(scope, extraScopeProps);
        element = compile(template)(scope);
        angular.element("body").append(element);
        scope.$digest();
        rootScope.$apply();
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