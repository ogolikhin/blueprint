import "angular";
import "angular-mocks";
import "angular-messages";
import "angular-sanitize";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { createFormlyModule } from "./formly-config.mock";

let moduleName = createFormlyModule([
    "formly",
    "formlyBootstrap"
], null);

describe("Formly", () => {
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

    it("template should compile", function() {
        compileAndSetupStuff();

        expect(element).toBeDefined();
        expect(node).toBeDefined();
        expect(isolateScope).toBeDefined();
        expect(vm).toBeDefined();
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
