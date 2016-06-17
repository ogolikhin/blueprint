import "angular";
import "angular-mocks";
import {IMessageService, MessageService, Message, MessageType} from "../../shell";
import {IConfigValueHelper, ConfigValueHelper } from "../../core";

describe("messageService", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
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
    
    describe("methods", () => {      
        beforeEach(inject((messageService: IMessageService) => {
            messageService.addError("test1");
            messageService.addError("test2");
            var message = new Message(MessageType.Info, "someText");
            messageService.addMessage(message);
        }));
        it("countsMessageByType",
            inject((messageService: IMessageService) => {
                // Assert
                expect(messageService.countsMessageByType(MessageType.Error)).toEqual(2);
                expect(messageService.countsMessageByType(MessageType.Warning)).toEqual(0);
            }));
        it("hasMessages",
            inject((messageService: IMessageService) => {             
                // Assert
                expect(messageService.hasMessages(MessageType.Error)).toEqual(true);
                expect(messageService.hasMessages(MessageType.Warning)).toEqual(false);
            }));
        it("deleteMessages",
            inject((messageService: IMessageService) => {
               // Act
                messageService.deleteMessages(MessageType.Error);

                // Assert
                expect(messageService.getMessages().length).toEqual(1);
            }));
        it("clearMessages",
            inject((messageService: IMessageService) => {
                // Act
                messageService.clearMessages();

                // Assert
                expect(messageService.getMessages().length).toEqual(0);
            }));
        it("getFirstOfTypeMessage",
            inject((messageService: IMessageService) => {
                // Act
                var msg = messageService.getFirstOfTypeMessage(MessageType.Error);

                // Assert
                expect(msg.messageText).toEqual("test1");
            }));        
    });
});