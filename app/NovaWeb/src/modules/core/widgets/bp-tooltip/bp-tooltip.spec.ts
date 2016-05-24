import "../../../main"
import "angular";
import "angular-mocks";
//import {ComponentTest} from "../../../util/component.test";
//import {BPTooltip} from "./bp-tooltip"

describe("Directive BP-Tooltip", () => {
    var tooltipTrigger = `<div bp-tooltip="Tooltip's content">Tooltip trigger</div>`;
    var compile, scope, directiveElement;

    beforeEach(angular.mock.module("app.main"));

    //var directiveTest: ComponentTest<BPTooltip>;

    beforeEach(() => {
        //directiveTest = new ComponentTest<BPTooltip>(tooltipTrigger, "bp-tooltip");
    });

/*    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.directive("bpTooltip", <any>BPTooltip.factory);
        console.log("before each")

    }));
*/
    it("can initialize", inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {

        // Arrange
        let scope = $rootScope.$new();
        let element = $compile(tooltipTrigger)(scope);
        scope.$digest();

        // Act
        $rootScope.$apply();

        // Assert
        var childElement = element.find("div");
        expect(childElement[0]).toBeDefined();
    }));


/*
    beforeEach(function() {
        inject(function($compile, $rootScope) {
            compile = $compile;
            scope = $rootScope.$new();
        });

        directiveElement = getCompiledElement();
    });

    function getCompiledElement(){
        var element = angular.element(tooltipTrigger);
        var compiledElement = compile(element)(scope);
        scope.$digest();
        return compiledElement;
    }

    it("is invoked and the directive tag is removed from the trigger element", () => {
        console.log(directiveElement);
        console.log(directiveElement[0].children);
    });
*/
});