import * as angular from "angular";
import "angular-mocks";
import {LocalizationServiceMock} from "../localization/localization.mock";
import {IMessageService, MessageService} from "./message.svc";
import {Message, MessageType} from "./message";
import {SettingsService} from "../configuration";
import {MessageContainerController, MessageContainerComponent} from "./message-container";


describe("messages container directive", () => {
    var element: ng.IAugmentedJQuery;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.component("messagesContainer", <any>new MessageContainerComponent());
        $provide.service("localization", LocalizationServiceMock);
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

    it("can show a directive", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, messageService: IMessageService) => {
        // Arrange
        var controller: MessageContainerController;
        var scope = $rootScope.$new();

        messageService.addMessage(new Message(MessageType.Error, "Error1"));
        messageService.addMessage(new Message(MessageType.Error, "Error2"));
        messageService.addMessage(new Message(MessageType.Info, "Info1"));
        messageService.addMessage(new Message(MessageType.Info, "Info2"));
        messageService.addMessage(new Message(MessageType.Warning, "Warning1"));

        element = $compile("<messages-container/>")(scope);
        scope.$digest();
        controller = element.isolateScope()["messageContainterCntrl"];

        // Assert       
        expect(element.find("message").length).toEqual(5);
    })));
});
