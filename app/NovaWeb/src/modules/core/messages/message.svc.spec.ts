import * as angular from "angular";
import "angular-mocks";
import { LocalizationServiceMock } from "../localization/localization.mock";
import { IMessageService, MessageService } from "./message.svc";
import { Message, MessageType} from "./message";
import { SettingsService } from "../configuration";

describe("messageService", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
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

    it("addError, returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            var message = "test1";
            messageService.addError(message);

            // Act
            var result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0].messageText).toEqual(message);
            expect(result[0].messageType).toEqual(MessageType.Error);
        }));

    it("addError as an Error, returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            var error = new Error("test1");
            messageService.addError(error);

            // Act
            var result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0].messageText).toEqual(error.message);
            expect(result[0].messageType).toEqual(MessageType.Error);
        }));


    it("addMessage, returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            var message = new Message(MessageType.Info, "someText");
            messageService.addMessage(message);

            // Act
            var result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0]).toBe(message);
        }));

    it("don't add a message if it already exists in the list",
        inject((messageService: IMessageService) => {
            // Arrange
            const message = new Message(MessageType.Info, "someText");
            messageService.addMessage(message);

            // Act
            messageService.addMessage(new Message(MessageType.Info, "someText"));
            const result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0]).toBe(message);
        }));
    
    describe("methods", () => {      
        beforeEach(inject((messageService: IMessageService) => {
            messageService.addError("test1");
            messageService.addError("test2");
            var message = new Message(MessageType.Info, "someText");
            messageService.addMessage(message);
        }));

        it("deleteMessages",
            inject((messageService: IMessageService) => {
               // Act
                messageService.deleteMessageById(1);

                // Assert
                expect(messageService.messages.length).toEqual(2);
            }));
        it("dispose",
            inject((messageService: IMessageService) => {
                // Act
                messageService.dispose();

                // Assert
                expect(messageService.messages.length).toEqual(0);
            }));
            
    });
});