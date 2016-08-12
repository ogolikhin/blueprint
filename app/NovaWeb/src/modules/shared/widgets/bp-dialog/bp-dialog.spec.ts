import "angular";
import "angular-mocks";
import { IDialogSettings, IDialogService, DialogService, DialogTypeEnum} from "./bp-dialog";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";

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

class ModalServiceInstanceMock implements ng.ui.bootstrap.IModalServiceInstance {
    public static $inject = ["$q"];
    private resultDeffered = this.$q.defer<any>();
    private openedDeffered = this.$q.defer<any>();

    constructor(private $q: ng.IQService) {
        this.opened = this.openedDeffered.promise;
        this.rendered = this.openedDeffered.promise;
        this.result = this.resultDeffered.promise;
    }

    public close(result?: any): void {
        this.resultDeffered.resolve(result);
    }

    public dismiss(reason?: any): void {
        this.resultDeffered.reject();
    }

    public result: angular.IPromise<any>;

    public opened: angular.IPromise<any>;

    public rendered: angular.IPromise<any>;

    public closed: angular.IPromise<any>;
}

describe("DialogService", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("dialogService", DialogService);
        $provide.service("$uibModal", ModalMock);
        $provide.service("localization", LocalizationServiceMock);
    }));

    describe("open method", () => {
        it("simple open dialog ", inject((dialogService: IDialogService) => {
            // Arrange
            var spy = spyOn(dialogService, "openInternal").and.callThrough();
            var settings: IDialogSettings = {};

            // Act
            dialogService.open(settings);

            // Assert
            expect(spy).toHaveBeenCalled();
        }));
        it("open dialog fake", inject((dialogService: IDialogService, $q: ng.IQService) => {
            // Arrange
            var spy = spyOn(dialogService, "openInternal").and.callFake(function () {
                return new ModalServiceInstanceMock($q);
            });
            var settings: IDialogSettings = {};

            // Act
            dialogService.open(settings);

            // Assert
            expect(spy).toHaveBeenCalledTimes(1);
        }));


        it("open dialog with default settings", inject((dialogService: IDialogService) => {
            // Arrange
            spyOn(dialogService, "openInternal").and.callThrough();
            var settings: IDialogSettings = {};

            // Act
            dialogService.open(settings);

            // Assert
            expect(dialogService.params.type).toEqual(DialogTypeEnum.Base, "invalid type [" + dialogService.params.type + "]");
            expect(dialogService.params.okButton).toEqual("App_Button_Ok", "invalid ok button [" + dialogService.params.okButton + "]");
            expect(dialogService.params.cancelButton).toEqual("App_Button_Cancel", "invalid cancel button [" + dialogService.params.cancelButton + "]");
        }));

        it("open dialog with settings", inject((dialogService: IDialogService) => {
            // Arrange
            spyOn(dialogService, "openInternal").and.callThrough();
            var settings: IDialogSettings = {
                type: DialogTypeEnum.Alert,
                cancelButton : "CANCEL",
                okButton: "OKAY",
                template: "template"
            };

            // Act
            dialogService.open(settings);

            // Assert
            expect(dialogService.params.type).toEqual(DialogTypeEnum.Alert, "invalid type [" + dialogService.params.type + "]");
            expect(dialogService.params.okButton).toEqual("OKAY", "invalid ok button [" + dialogService.params.okButton + "]");
            expect(dialogService.params.cancelButton).toEqual("CANCEL", "invalid cancel button [" + dialogService.params.cancelButton + "]");
            expect(dialogService.params.template).toEqual("template", "invalid template [" + dialogService.params.template + "]");
        }));
        it("alert dialog", inject((dialogService: IDialogService) => {
            // Arrange
            spyOn(dialogService, "openInternal").and.callThrough();

            // Act
            dialogService.alert("MESSAGE");

            // Assert
            expect(dialogService.params.type).toEqual(DialogTypeEnum.Alert, "invalid type [" + dialogService.params.type + "]");
            expect(dialogService.params.okButton).toEqual("App_Button_Ok", "invalid ok button [" + dialogService.params.okButton + "]");
            expect(dialogService.params.cancelButton).toEqual(null, "invalid cancel button [" + dialogService.params.cancelButton + "]");
            expect(dialogService.params.message).toEqual("MESSAGE", "invalid message [" + dialogService.params.message + "]");
        }));
        it("alert dialog with header", inject((dialogService: IDialogService) => {
            // Arrange
            spyOn(dialogService, "openInternal").and.callThrough();

            // Act
            dialogService.alert("MESSAGE", "HEADER");

            // Assert
            expect(dialogService.params.message).toEqual("MESSAGE", "invalid message [" + dialogService.params.message + "]");
            expect(dialogService.params.header).toEqual("HEADER", "invalid header [" + dialogService.params.header + "]");
        }));
        it("confirm dialog", inject((dialogService: IDialogService) => {
            // Arrange
            spyOn(dialogService, "openInternal").and.callThrough();

            // Act
            dialogService.confirm("CONFIRM");

            // Assert
            expect(dialogService.params.type).toEqual(DialogTypeEnum.Confirm, "invalid type [" + dialogService.params.type + "]");
            expect(dialogService.params.okButton).toEqual("App_Button_Ok", "invalid ok button [" + dialogService.params.okButton + "]");
            expect(dialogService.params.cancelButton).toEqual("App_Button_Cancel", "invalid cancel button [" + dialogService.params.cancelButton + "]");
            expect(dialogService.params.message).toEqual("CONFIRM", "invalid message [" + dialogService.params.message + "]");
        }));

        it("confirm dialog", inject((dialogService: IDialogService) => {
            // Arrange
            spyOn(dialogService, "openInternal").and.callThrough();

            // Act
            dialogService.confirm("CONFIRM", "HEADER");

            // Assert
            expect(dialogService.params.message).toEqual("CONFIRM", "invalid message [" + dialogService.params.message + "]");
            expect(dialogService.params.header).toEqual("HEADER", "invalid header [" + dialogService.params.header + "]");
        }));
    });
});