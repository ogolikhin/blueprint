export enum MessageType {
    Error = 1, Info = 2, Warning = 3
}

export class Message {
    public onMessageAction: (actionName: string) => void;

    constructor(public messageType: MessageType, public messageText) {
    }
}

export interface IMessageController {
    messageType: MessageType;
    closeAlert: Function;
    onMessageClosed: Function;
}

export class MessageController implements IMessageController {
    public messageType: MessageType;
    public closeAlert: Function;
    public onMessageClosed: Function;
}

export interface IMessageScope extends ng.IScope {
    messageCntrl: MessageController;    
}

export class MessageComponent implements ng.IComponentOptions {
    public template: string = require("./message.html");
    public controller: Function = MessageController;
    public bindings: any = {
        onMessageClosed: "&"
    };
}

export class MessageDirective implements ng.IDirective { 
    public template: string = require("./message.html");
    public restrict = "E";

    public transclude = true;

    public scope = {
        onMessageClosed: "&"     
    };

    public static directive: any[] = [
        "$timeout",
        "$rootScope",
        ($timeout: ng.ITimeoutService, $rootScope: ng.IRootScopeService) => {
            return new MessageDirective($timeout, $rootScope);
        }];

    constructor(private $timeout, private $rootScope) {
    }

    public link: ng.IDirectiveLinkFn = ($scope: IMessageScope, $element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
        $scope.messageCntrl.closeAlert = () => {
            if ($scope.messageCntrl.onMessageClosed) {
                $scope.messageCntrl.onMessageClosed();
            }
        }
        $scope.messageCntrl.messageType = attrs["messageType"];
    };

    public controller = MessageController;
    public controllerAs = "messageCntrl";
    public bindToController = true;
}


