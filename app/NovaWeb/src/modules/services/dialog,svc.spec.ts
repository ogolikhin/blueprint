import "angular";
import "angular-mocks"
import * as $D from "./dialog.svc";
import {LocalizationServiceMock, ModalServiceInstanceMock} from "../shell/login/mocks.spec";

class ModalMock implements ng.ui.bootstrap.IModalService {
    public static $inject = ["$q"];
    public instanceMock: ModalServiceInstanceMock;
    constructor(private $q: ng.IQService) {
        this.instanceMock = new ModalServiceInstanceMock(this.$q);
    }

    public open(options: ng.ui.bootstrap.IModalSettings): ng.ui.bootstrap.IModalServiceInstance {
        return this.instanceMock;
    }
}

describe("DialogService", () => {
    
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("dialogService", $D.DialogService);
        $provide.service("$uibModal", ModalMock);
        $provide.service("localization", LocalizationServiceMock);
    }));

    describe("open method", () => {
        it("simple open dialog ", inject((dialogService: $D.IDialogService) => {
            // Arrange
            var spy = spyOn(dialogService, "openInternal").and.callThrough();
            var settings: $D.IDialogSettings = {};
            
            // Act
            dialogService.open(settings);

            // Assert
            expect(spy).toHaveBeenCalled();
        }));
        it("open dialog fake", inject((dialogService: $D.IDialogService, $q: ng.IQService) => {
            // Arrange
            var spy = spyOn(dialogService, "openInternal").and.callFake(function () {
                return new ModalServiceInstanceMock($q);
            });
            var settings: $D.IDialogSettings = {};
            
            // Act
            dialogService.open(settings);

            // Assert
            expect(spy).toHaveBeenCalledTimes(1);
        }));


        it("open dialog with default settings", inject((dialogService: $D.IDialogService) => {
            // Arrange
            spyOn(dialogService, "openInternal").and.callThrough();
            var settings: $D.IDialogSettings = {};
            
            // Act
            dialogService.open(settings);

            // Assert
            expect(dialogService.params.type).toEqual($D.DialogTypeEnum.Base, "invalid type [" + dialogService.params.type + "]");
            expect(dialogService.params.okButton).toEqual("App_Button_Ok", "invalid ok button [" + dialogService.params.okButton + "]");
            expect(dialogService.params.cancelButton).toEqual("App_Button_Cancel", "invalid cancel button [" + dialogService.params.cancelButton + "]");
        }));


        it("open dialog with settings", inject((dialogService: $D.IDialogService) => {
            // Arrange
            var spy = spyOn(dialogService, "openInternal").and.callThrough();
            var settings: $D.IDialogSettings = {
                type: $D.DialogTypeEnum.Alert,
                cancelButton : "CANCEL",
                okButton: "OKAY",
                template: "template"
            };
            
            // Act
            dialogService.open(settings);

            // Assert
            expect(dialogService.params.type).toEqual($D.DialogTypeEnum.Alert, "invalid type [" + dialogService.params.type + "]");
            expect(dialogService.params.okButton).toEqual("OKAY", "invalid ok button [" + dialogService.params.okButton + "]");
            expect(dialogService.params.cancelButton).toEqual("CANCEL", "invalid cancel button [" + dialogService.params.cancelButton + "]");
            expect(dialogService.params.template).toEqual("template", "invalid template [" + dialogService.params.template + "]");
        }));
        it("alert dialog", inject((dialogService: $D.IDialogService) => {
            // Arrange
            var spy = spyOn(dialogService, "openInternal").and.callThrough();
            
            // Act
            dialogService.alert("MESSAGE");

            // Assert
            expect(dialogService.params.type).toEqual($D.DialogTypeEnum.Alert, "invalid type [" + dialogService.params.type + "]");
            expect(dialogService.params.okButton).toEqual("App_Button_Ok", "invalid ok button [" + dialogService.params.okButton + "]");
            expect(dialogService.params.cancelButton).toEqual(null, "invalid cancel button [" + dialogService.params.cancelButton + "]");
            expect(dialogService.params.message).toEqual("MESSAGE", "invalid message [" + dialogService.params.message + "]");
        }));
        it("alert dialog with header", inject((dialogService: $D.IDialogService) => {
            // Arrange
            var spy = spyOn(dialogService, "openInternal").and.callThrough();
            
            // Act
            dialogService.alert("MESSAGE","HEADER");

            // Assert
            expect(dialogService.params.message).toEqual("MESSAGE", "invalid message [" + dialogService.params.message + "]");
            expect(dialogService.params.header).toEqual("HEADER", "invalid header [" + dialogService.params.header + "]");
        }));
        it("confirm dialog", inject((dialogService: $D.IDialogService) => {
            // Arrange
            var spy = spyOn(dialogService, "openInternal").and.callThrough();
            
            // Act
            dialogService.confirm("CONFIRM");

            // Assert
            expect(dialogService.params.type).toEqual($D.DialogTypeEnum.Confirm, "invalid type [" + dialogService.params.type + "]");
            expect(dialogService.params.okButton).toEqual("App_Button_Ok", "invalid ok button [" + dialogService.params.okButton + "]");
            expect(dialogService.params.cancelButton).toEqual("App_Button_Cancel", "invalid cancel button [" + dialogService.params.cancelButton + "]");
            expect(dialogService.params.message).toEqual("CONFIRM", "invalid message [" + dialogService.params.message + "]");
        }));

        it("confirm dialog", inject((dialogService: $D.IDialogService) => {
            // Arrange
            var spy = spyOn(dialogService, "openInternal").and.callThrough();
            
            // Act
            dialogService.confirm("CONFIRM","HEADER");

            // Assert
            expect(dialogService.params.message).toEqual("CONFIRM", "invalid message [" + dialogService.params.message + "]");
            expect(dialogService.params.header).toEqual("HEADER", "invalid header [" + dialogService.params.header + "]");
        }));


    });
});