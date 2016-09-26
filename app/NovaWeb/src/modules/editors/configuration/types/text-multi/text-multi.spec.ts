import "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-ui-bootstrap";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { createFormlyModule } from "../../formly-config.mock";

let fieldsDefinition = [
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
    }
];

let moduleName = createFormlyModule([
    "formly",
    "formlyBootstrap"
], fieldsDefinition);

describe("Formly Text", () => {
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