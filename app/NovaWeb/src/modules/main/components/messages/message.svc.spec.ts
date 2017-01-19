import * as angular from "angular";
import "angular-mocks";
import {IMessageService, MessageService} from "./message.svc";
import {Message, MessageType} from "./message";
import {SettingsService} from "../../../core/configuration/settings.service";

describe("messageService", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
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

    it("addError, returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            const message = "test1";
            messageService.addError(message);

            // Act
            const result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0].messageText).toEqual(message);
            expect(result[0].messageType).toEqual(MessageType.Error);
        }));

    it("addInfo with parameters returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            let message = "show {0} items of type {1}";
            let par1 = 3;
            let par2 = "variable";
            messageService.addInfo(message, par1, par2);

            // Act
            let result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0].messageText).toEqual(message);
            expect(result[0].messageType).toEqual(MessageType.Info);
            expect(result[0].parameters).toEqual(jasmine.any(Array));
            expect(result[0].parameters.length).toEqual(2);
        }));

    it("addInfo without parameters returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            let message = "show {0} items of type {1}";
            messageService.addInfo(message);

            // Act
            let result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0].messageText).toEqual(message);
            expect(result[0].messageType).toEqual(MessageType.Info);
            expect(result[0].parameters).toBeUndefined();
        }));

    it("addWarning without parameters",
        inject((messageService: IMessageService) => {
            // Arrange
            let message = "show items of type";
            messageService.addWarning(message);

            // Act
            let result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0].messageText).toEqual(message);
            expect(result[0].messageType).toEqual(MessageType.Warning);
            expect(result[0].parameters).toBeUndefined();
        }));

    it("addWarning with parameters",
        inject((messageService: IMessageService) => {
            // Arrange
            let message = "show items of type";
            messageService.addWarning(message, 1, 2, 3);

            // Act
            let result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0].messageText).toEqual(message);
            expect(result[0].messageType).toEqual(MessageType.Warning);
            expect(result[0].parameters).toEqual(jasmine.any(Array));
            expect(result[0].parameters.length).toEqual(3);
            expect(result[0].parameters.toString()).toEqual("1,2,3");
        }));


    it("addError as an Error, returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            const error = new Error("test1");
            messageService.addError(error);

            // Act
            const result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0].messageText).toEqual(error.message);
            expect(result[0].messageType).toEqual(MessageType.Error);
        }));


    it("addMessage, returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            const message = new Message(MessageType.Info, "someText");
            messageService.addMessage(message);

            // Act
            const result = messageService.messages;

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0]).toBe(message);
        }));


    it("clearMessages, ignores persistent messages",
        inject((messageService: IMessageService) => {
            // Arrange
            const infoMsg = new Message(MessageType.Info, "Info Message");
            const errorMsg = new Message(MessageType.Error, "Error Message", true);
            messageService.addMessage(infoMsg);
            messageService.addMessage(errorMsg);

            // Act
            const result = messageService.messages;
            messageService.clearMessages();

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0]).toBe(errorMsg);
        }));


    it("clearMessages, clears persistent messages also",
        inject((messageService: IMessageService) => {
            // Arrange
            const infoMsg = new Message(MessageType.Info, "Info Message");
            const errorMsg = new Message(MessageType.Error, "Error Message", true);
            messageService.addMessage(infoMsg);
            messageService.addMessage(errorMsg);

            // Act
            const result = messageService.messages;
            messageService.clearMessages(true);

            // Assert
            expect(result.length).toEqual(0);
        }));


    it("clearMessages, clears specific message types even if persistent",
        inject((messageService: IMessageService) => {
            // Arrange
            const infoMsg = new Message(MessageType.Info, "Info Message");
            const errorMsg = new Message(MessageType.Error, "Error Message", true);
            const deletedMessage = new Message(MessageType.Deleted, "Deleted Message", true);
            const lockMessage = new Message(MessageType.Lock, "Lock Message", true);
            messageService.addMessage(infoMsg);
            messageService.addMessage(errorMsg);
            messageService.addMessage(deletedMessage);
            messageService.addMessage(lockMessage);

            // Act
            const result = messageService.messages;
            messageService.clearMessages(false, [MessageType.Deleted, MessageType.Lock]);

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0]).toBe(errorMsg);
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
            const message = new Message(MessageType.Info, "someText");
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
