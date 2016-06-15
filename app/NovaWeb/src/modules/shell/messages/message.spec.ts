import "angular";
import "angular-mocks";
import {MessagesContainerDirective, IMessageService, MessageService, Message, MessageType, MessageDirective} from "../../shell";
import {MessageContainerController} from "./message-container";
import {IConfigValueHelper, ConfigValueHelper } from "../../core";

describe("message directive", () => {
    var element: JQuery;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.directive("message", <any>MessageDirective.factory());
        $provide.service("messageService", MessageService);
        $provide.service("configValueHelper", ConfigValueHelper);
    }));
    beforeEach(inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        $rootScope["config"] = {
            "settings": {
                "StorytellerMessageTimeout": '{ "Warning": 0, "Info": 3000, "Error": 0 }'
            }
        };
    }));

    it("can show an error message", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        // Arrange
        var scope = $rootScope.$new();
        element = $compile(" <message data-message-type=\"error\" />")(scope);
        scope.$digest();
        // Act

        // Assert      
        expect($(element.children(".error-msg_container")[0]).attr("class")).toContain("alert-error");

    })));

    it("hide a directive", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        // Arrange
        var scope = $rootScope.$new();
        element = $compile(" <message data-message-type=\"error\" data-ng-if=\"false\" />")(scope);
        scope.$digest();
        // Assert
        expect(element.html()).toBeUndefined();
    })));

});