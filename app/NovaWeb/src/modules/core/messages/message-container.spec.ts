﻿import "angular";
import "angular-mocks";
import "lodash";
import {LocalizationServiceMock} from "../localization/localization.service.mock";
import {IMessageService, MessageService} from "./message.svc";
import {Message, MessageType} from "./message";
import {MessageContainerController, MessageContainerComponent} from "./message-container";
import {SettingsService} from "../configuration/settings.service";


describe("messages container directive", () => {
    let element: ng.IAugmentedJQuery;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.component("messagesContainer", <any>new MessageContainerComponent());
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageService);
        $provide.service("settings", SettingsService);
    }));

    beforeEach(inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
        $rootScope["config"] = {
            "settings": {
                "StorytellerMessageTimeout": `{"Warning": 0, "Info": 3000, "Error": 0}`
            }
        };
    }));

    it("can show a directive", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, messageService: IMessageService) => {
        // Arrange
        let controller: MessageContainerController;
        let scope = $rootScope.$new();

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

    it("show message with parameters", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, messageService: IMessageService) => {
        // Arrange
        let controller: MessageContainerController;
        let scope = $rootScope.$new();

        let message = "show {0} <strong>items</strong> of type {1}";
        let par1 = 3;
        let par2 = "variable";
        let result = "show 3 <strong>items</strong> of type variable";

        messageService.addMessage(new Message(MessageType.Info, message, false, par1, par2));

        element = $compile("<messages-container/>")(scope);
        scope.$digest();
        controller = element.isolateScope()["messageContainterCntrl"];
        const messageContainer = element.find("message")[0] as HTMLElement;
        const messageText = messageContainer.firstElementChild;

        // Assert
        expect(messageContainer.children.length).toEqual(1);
        expect(messageText.textContent).toEqual(result);
        expect(messageText.innerHTML).toEqual(_.escape(result));

    })));

});
