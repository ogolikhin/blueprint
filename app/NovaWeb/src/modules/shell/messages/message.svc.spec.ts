//import "angular";
//import "angular-mocks";
//import {IMessageService, MessageService, Message, MessageType} from "../../shell";
//import {ConfigValueHelper } from "../../core";

//describe("messageService", () => {
//    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//        $provide.service("messageService", MessageService);
//        $provide.service("configValueHelper", ConfigValueHelper);
//    }));

//    beforeEach(inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService, $templateCache: ng.ITemplateCacheService) => {
//       $rootScope["config"] = {
//            "settings": {
//                "StorytellerMessageTimeout": `{ "Warning": 0, "Info": 7000, "Error": 0 }`
//            }
//        };       
//    }));

//    it("addError, returns the message",
//        inject((messageService: IMessageService) => {
//            // Arrange
//            var message = "test1";
//            messageService.addError(message);

//            // Act
//            var result = messageService.messages.getValue();

//            // Assert
//            expect(result.length).toEqual(1);
//            expect(result[0].messageText).toEqual(message);
//            expect(result[0].messageType).toEqual(MessageType.Error);
//        }));

//    it("addMessage, returns the message",
//        inject((messageService: IMessageService) => {
//            // Arrange
//            var message = new Message(MessageType.Info, "someText");
//            messageService.addMessage(message);

//            // Act
//            var result = messageService.messages.getValue();

//            // Assert
//            expect(result.length).toEqual(1);
//            expect(result[0]).toBe(message);
//        }));
    
//    describe("methods", () => {      
//        beforeEach(inject((messageService: IMessageService) => {
//            messageService.addError("test1");
//            messageService.addError("test2");
//            var message = new Message(MessageType.Info, "someText");
//            messageService.addMessage(message);
//        }));

//        it("deleteMessages",
//            inject((messageService: IMessageService) => {
//               // Act
//                messageService.deleteMessageById(1);

//                // Assert
//                expect(messageService.messages.getValue().length).toEqual(2);
//            }));
//        it("dispose",
//            inject((messageService: IMessageService) => {
//                // Act
//                messageService.dispose();

//                // Assert
//                expect(messageService.messages.getValue().length).toEqual(0);
//            }));
            
//    });
//});