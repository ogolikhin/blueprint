import * as angular from "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "ui-select";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import {createFormlyModule} from "../../formly-config.mock";
import {ValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";

describe("Formly Select", () => {
    let fieldsDefinition = [
        {
            type: "bpFieldSelect",
            key: "select",
            templateOptions: {
                options: [
                    {value: 1, name: "Option 1"},
                    {value: 2, name: "Option 2"},
                    {value: 3, name: "Option 3"},
                    {value: 4, name: "Option 4"},
                    {value: 5, name: "Option 5"}
                ],
                optionsAttr: "bs-options",
                required: true
            },
            data: {
                isValidated: false,
                lookup: 2
            }
        },
        {
            type: "bpFieldSelect",
            key: "selectNotVal",
            templateOptions: {
                options: [
                    {value: 10, name: "Option 10"},
                    {value: 20, name: "Option 20"},
                    {value: 30, name: "Option 30"},
                    {value: 40, name: "Option 40"},
                    {value: 50, name: "Option 50"}
                ],
                optionsAttr: "bs-options"
            }
        }
    ];

    let moduleName = createFormlyModule("formlyModuleSelect", [
        "ngSanitize",
        "ui.select",
        "formly",
        "formlyBootstrap"
    ], fieldsDefinition);

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {        
        $provide.service("validationService", ValidationService);
    }));

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
        compileAndSetupStuff({model: {select: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelect");
        let fieldScope = angular.element(fieldNode[0]).isolateScope();
        let fieldSearch = fieldNode[0].querySelector(".ui-select-search");
        angular.element(fieldSearch).triggerHandler("click");

        expect(fieldNode.length).toBe(2);
        expect(fieldNode[0]).toBeDefined();
        expect(fieldScope).toBeDefined();
    });

    it("should fail if empty", function () {
        compileAndSetupStuff({model: {select: undefined}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelect")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let fieldChosen = fieldNode.querySelector(".ui-select-match-item-chosen");

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.required).toBeTruthy();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeTruthy();
        expect(fieldChosen.innerHTML).toBe("");
    });

    it("should succeed with value", function () {
        compileAndSetupStuff({model: {select: 1}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelect")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let fieldChosen = fieldNode.querySelector(".ui-select-match-item-chosen");

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeUndefined();
        expect(fieldChosen.innerHTML).toContain("Option 1");
    });

    xit("should succeed with custom value", function () {
        compileAndSetupStuff({model: {select: {customValue: "Custom value"}}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelect")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();
        let fieldChosen = fieldNode.querySelector(".ui-select-match-item-chosen");

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeUndefined();
        expect(fieldChosen.innerHTML).toContain("Custom value");
    });

    it("should succeed if empty, as not required", function () {
        compileAndSetupStuff({model: {selectNotVal: null}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelect")[1];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect((<any>fieldScope).fc.$error.required).toBeUndefined();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeUndefined();
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
});
