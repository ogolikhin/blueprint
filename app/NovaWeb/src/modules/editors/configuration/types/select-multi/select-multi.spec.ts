import "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "ui-select";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { createFormlyModule } from "../../formly-config.mock";

describe("Formly Select Multi", () => {
    let fieldsDefinition = [
        {
            type: "bpFieldSelectMulti",
            key: "selectMulti",
            templateOptions: {
                options: [
                    { value: 1, name: "Option 1" },
                    { value: 2, name: "Option 2" },
                    { value: 3, name: "Option 3" },
                    { value: 4, name: "Option 4" }
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
                    { value: 40, name: "Option 40" }
                ],
                optionsAttr: "bs-options"
            }
        }
    ];

    let moduleName = createFormlyModule("formlyModuleSelectMulti", [
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
        compileAndSetupStuff({model: {selectMulti: ""}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelectMulti");
        let fieldScope = angular.element(fieldNode[0]).isolateScope();
        fieldNode[0].querySelector(".ui-select-container > div").dispatchEvent(new Event("click", { "bubbles": true }));

        expect(fieldNode.length).toBe(2);
        expect(fieldNode[0]).toBeDefined();
        expect(fieldScope).toBeDefined();
        expect(document.activeElement.tagName.toUpperCase()).toBe("INPUT");
        expect(document.activeElement.classList.contains("ui-select-search")).toBeTruthy();
    });

    it("should fail if empty", function () {
        compileAndSetupStuff({model: {selectMulti: []}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelectMulti")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeFalsy();
        expect((<any>fieldScope).fc.$invalid).toBeTruthy();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeTruthy();
    });

    it("should succeed with values", function () {
        compileAndSetupStuff({model: {selectMulti: [1, 3]}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelectMulti")[0];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
        expect((<any>fieldScope).fc.$error.requiredCustom).toBeUndefined();
    });

    it("should succeed if empty, as not required", function () {
        compileAndSetupStuff({model: {selectMultiNotVal: []}});

        let fieldNode = node.querySelectorAll(".formly-field-bpFieldSelectMulti")[1];
        let fieldScope = angular.element(fieldNode).isolateScope();

        expect((<any>fieldScope).fc.$valid).toBeTruthy();
        expect((<any>fieldScope).fc.$invalid).toBeFalsy();
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