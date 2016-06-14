import "angular";
import "angular-mocks";
import {MessagesContainerDirective, MessageService, Message, MessageType} from "../../shell";

describe("messages container directive", () => {
    var element: JQuery;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService, $compileProvider: ng.ICompileProvider) => {
        $compileProvider.directive("messagesContainer", <any>MessagesContainerDirective.factory());
    }));

    it("can show a directive", (inject(($compile: ng.ICompileService, $rootScope: ng.IRootScopeService) => {
        // Arrange
        var scope = $rootScope.$new();


        var messages: Message[] = [];

        messages.push(new Message(MessageType.Error, "Error1"));
        messages.push(new Message(MessageType.Error, "Error2"));
        messages.push(new Message(MessageType.Info, "Info"));
        scope["messages"] = messages;

        scope["hasMessages"] = () => { return true; };
        scope["closeMessages"] = () => { };

        element = $compile(" <messages-container has-messages='hasMessages'  data-messages='messages' data-close-messages='closeMessages' />")(scope);
        scope.$digest();
        // Act

        // Assert
        // 3 types of messages
        expect(element[0].childElementCount).toEqual(4);
        // 2 error messages
        expect(element[0].children[0].getElementsByTagName("ul")[0].getElementsByTagName("li").length).toEqual(2);
        // 1 success messages
        expect(element[0].children[1].getElementsByTagName("ul")[0].getElementsByTagName("li").length).toEqual(1);
        // no warnings
        expect(element[0].children[2].getElementsByTagName("ul")[0].getElementsByTagName("li").length).toEqual(0);
    })));
});