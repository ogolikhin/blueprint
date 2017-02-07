import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import {createFormlyModule} from "../../formly-config.mock";
import {ValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {Models} from "../../../../main/models";

describe("Formly Text", () => {
    
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {        
        $provide.service("validationService", ValidationService);
    }));
    
    let fieldsDefinition = [
        {
            type: "bpFieldText",
            key: "text",
            templateOptions: {
                required: true
            }
        },
        {
            type: "bpFieldText",
            key: "textNotVal",
            data: {
                propertyTypePredefined: Models.PropertyTypePredefined.Description
            }
        },
        {
            type: "bpFieldText",
            key: "name",
            data: {
                propertyTypePredefined: Models.PropertyTypePredefined.Name
            }
        }
    ];

    let moduleName = createFormlyModule("formlyModuleText", [
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
        compileAndSetupStuff({model: {text: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldText");
        let fieldScope = angular.element(fieldNode[0]).isolateScope();

        expect(fieldNode.length).toBe(3);
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

    it("should fail if system property name is empty", function () {
        compileAndSetupStuff({model: {name: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldText")[2];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
    });

    it("should fail if system property name is whitespace", function () {
        compileAndSetupStuff({model: {name: "   "}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldText")[2];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
    });

    it("should succeed if system property name has any value", function () {
        compileAndSetupStuff({model: {name: "Artifact Name"}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldText")[2];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
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