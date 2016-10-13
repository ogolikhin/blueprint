import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import {createFormlyModule} from "../../formly-config.mock";

describe("Formly Number", () => {
    let fieldsDefinition = [
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
        }
    ];

    let moduleName = createFormlyModule("formlyModuleNumber", [
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
        compileAndSetupStuff({model: {number: "10"}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber");
        let fieldScope = angular.element(fieldNode[0]).isolateScope();

        expect(fieldNode.length).toBe(2);
        expect(fieldNode[0]).toBeDefined();
        expect(fieldScope).toBeDefined();
        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
    });

    it("should fail if the number is in a wrong format (literal)", function () {
        compileAndSetupStuff({model: {number: "a"}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.wrongFormat).toBeTruthy();
    });

    it("should fail if the number is in a wrong format (invalid)", function () {
        compileAndSetupStuff({model: {number: "2."}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.wrongFormat).toBeTruthy();
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
        compileAndSetupStuff({model: {number: 10.12}});

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
        expect((<any>fieldScope).fc.$error.wrongFormat).toBeUndefined();
    });

    it("should fail when format is wrong, even if validation is not required", function () {
        compileAndSetupStuff({model: {numberNotVal: "2."}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[1];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.wrongFormat).toBeTruthy();
    });

    it("should allow changing the value", function () {
        compileAndSetupStuff({model: {number: 10}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldNumber")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let fieldInput = fieldNode.querySelector("input");

        fieldInput.value = "20.2";
        angular.element(fieldInput).triggerHandler("change");

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect(scope.model.number).not.toBe(10);
        expect(scope.model.number).toBe("20.2");
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
