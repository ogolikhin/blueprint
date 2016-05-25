import "../..";
import "angular";
import "angular-mocks";

describe("Directive BP-Tooltip", () => {
    var tooltipTrigger = `<div bp-tooltip="Tooltip's content">Tooltip trigger</div>`;

    beforeEach(angular.mock.module("app.core"));

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