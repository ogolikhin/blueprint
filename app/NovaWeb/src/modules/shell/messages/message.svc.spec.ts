import "angular";
import "angular-mocks";
import {IMessageService, MessageService, Message, MessageType} from "../../shell";

describe("messageService", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageService);
    }));

    it("addError, returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            var message = "test1";
            messageService.addError(message);

            // Act
            var result = messageService.getMessages();

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0].messageText).toEqual(message);
            expect(result[0].messageType).toEqual(MessageType.Error);
        }));

    it("addMessage, returns the message",
        inject((messageService: IMessageService) => {
            // Arrange
            var message = new Message(MessageType.Info, "someText");
            messageService.addMessage(message);

            // Act
            var result = messageService.getMessages();

            // Assert
            expect(result.length).toEqual(1);
            expect(result[0]).toBe(message);
        }));

    it("clearMessages",
        inject((messageService: IMessageService) => {
            // Arrange
            messageService.addError("test1");
            messageService.addError("test2");

            // Act
            messageService.clearMessages();

            // Assert
            expect(messageService.getMessages().length).toEqual(0);
        }));
});