import "angular";
import "angular-mocks";
import {MessageDirective, MessageService, Message, MessageType} from "../../shell";

describe("message directive", () => {
    var element: JQuery;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.directive("message", <any>MessageDirective.factory());
    }));

    it("can show a directive", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        // Arrange
        var directiveTemplate = "<div class=\"bp-alert alert\" data-ng-class=\"{'error':'alert-error', 'warning':'alert-warning', 'success':'alert-success' } [messageType]\"><button type=\"button\" class=\"close\" data-=\"\" dismiss=\"alert- msg - box\" data-ng-click=\"closeAlert()\"><span class=\"fa- remove\"> </span></button><span class=\"fa\" data-ng-class=\"{'error':'fonticon-error', 'warning':'fonticon-warning', 'success':'fonticon-success' } [messageType]\"> </span> &lt; span data-ng-transclude&gt; &lt; div class=\"clear\"&gt; </div> &lt; /div&gt;\"";
        $templateCache.put("/Areas/Web/App/Common/Messaging/messageTemplate.html", directiveTemplate);

        var scope = $rootScope.$new();
        element = $compile(" <message />")(scope);
        scope.$digest();
        // Act

        // Assert
        expect(element.html()).toContain(directiveTemplate);
    })));

    it("can show an error message", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        // Arrange
        var directiveTemplate = "<div class=\"bp-alert alert alert-error\" data-ng-class=\"{'error':'alert-error', 'warning':'alert-warning', 'success':'alert-success' } [messageType]\"><button type=\"button\" class=\"close\" data-=\"\" dismiss=\"alert- msg - box\" data-ng-click=\"closeAlert()\"><span class=\"fa- remove\"> </span></button><span class=\"fonticon-error\" data-ng-class=\"{'error':'fonticon-error', 'warning':'fonticon-warning', 'success':'fonticon-success' } [messageType]\"> </span> &lt; span data-ng-transclude&gt; &lt; div class=\"clear\"&gt; </div> &lt; /div&gt;\"";
        $templateCache.put("/Areas/Web/App/Common/Messaging/messageTemplate.html", directiveTemplate);

        var scope = $rootScope.$new();
        element = $compile(" <message data-message-type=\"error\" />")(scope);
        scope.$digest();
        // Act

        // Assert
        expect(element.html()).toContain(directiveTemplate);
        expect($(element.children(".bp-alert")[0]).attr("class")).toEqual("bp-alert alert alert-error");

    })));

    it("hide a directive", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        // Arrange
        var directiveTemplate = "<div class=\"bp-alert alert alert-error\" data-ng-class=\"{'error':'alert-error', 'warning':'alert-warning', 'success':'alert-success' } [messageType]\"><button type=\"button\" class=\"close\" data-=\"\" dismiss=\"alert- msg - box\" data-ng-click=\"closeAlert()\"><span class=\"fa- remove\"> </span></button><span class=\"fonticon-error\" data-ng-class=\"{'error':'fonticon-error', 'warning':'fonticon-warning', 'success':'fonticon-success' } [messageType]\"> </span> &lt; span data-ng-transclude&gt; &lt; div class=\"clear\"&gt; </div> &lt; /div&gt;\"";
        $templateCache.put("/Areas/Web/App/Common/Messaging/messageTemplate.html", directiveTemplate);

        var scope = $rootScope.$new();
        element = $compile(" <message data-message-type=\"error\" data-ng-if=\"false\" />")(scope);
        scope.$digest();
        // Assert
        expect(element.html()).toBeUndefined();
    })));

    it("close directive method called", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        // Arrange
        var directiveTemplate = "<div class=\"bp-alert alert alert-error\" data-ng-class=\"{'error':'alert-error', 'warning':'alert-warning', 'success':'alert-success' } [messageType]\"><button type=\"button\" class=\"close\" data-=\"\" dismiss=\"alert- msg - box\" data-ng-click=\"closeAlert()\"><span class=\"fa- remove\"> </span></button><span class=\"fonticon-error\" data-ng-class=\"{'error':'fonticon-error', 'warning':'fonticon-warning', 'success':'fonticon-success' } [messageType]\"> </span> &lt; span data-ng-transclude&gt; &lt; div class=\"clear\"&gt; </div> &lt; /div&gt;\"";
        $templateCache.put("/Areas/Web/App/Common/Messaging/messageTemplate.html", directiveTemplate);

        var scope = $rootScope.$new();
        scope["closeMessages"] = () => { };
        var spyEvent = spyOn(scope, "closeMessages");
        element = $compile("<message data-message-type=\"error\" data-on-message-closed=\"closeMessages()\" />")(scope);
        scope.$digest();


        $(element.children()[0]).children("button").click();

        // Assert
        expect(spyEvent).toHaveBeenCalled();
    })));
});