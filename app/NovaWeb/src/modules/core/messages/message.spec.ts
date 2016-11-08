import "angular-mocks";
import {MessageService} from "./message.svc";
import {MessageComponent} from "./message";
import {SettingsService} from "../configuration/settings";

describe("message directive", () => {
    let element: JQuery;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.component("message", <any>new MessageComponent());
        $provide.service("messageService", MessageService);
        $provide.service("settings", SettingsService);
    }));
    beforeEach(inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        $rootScope["config"] = {
            "settings": {
                "StorytellerMessageTimeout": `{ "Warning": 0, "Info": 3000, "Error": 0 }`
            }
        };
    }));

    it("can show an error message", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        // Arrange
        let scope = $rootScope.$new();
        element = $compile(" <message data-message-type=\"error\" />")(scope);
        scope.$digest();
        // Act

        // Assert
        expect($(element.children(".container")[0]).attr("class")).toContain("error");

    })));

    it("hide a directive", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        // Arrange
        let scope = $rootScope.$new();
        element = $compile(" <message data-message-type=\"error\" data-ng-if=\"false\" />")(scope);
        scope.$digest();
        // Assert
        expect(element.html()).toBeUndefined();
    })));

});
