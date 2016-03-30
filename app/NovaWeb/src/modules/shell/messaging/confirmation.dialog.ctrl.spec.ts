import "angular";
import "angular-mocks"
import {ConfirmationDialogCtrl} from "./confirmation.dialog.ctrl";
import {ILocalizationService} from "../../core/localization";

export class ModalServiceInstanceMock implements ng.ui.bootstrap.IModalServiceInstance {
    public actualResult: boolean;

    constructor() {
    }

    public close(result?: any): void {
        this.actualResult = result;
    }

    public dismiss(reason?: any): void {
    }

    public result: angular.IPromise<any>;

    public opened: angular.IPromise<any>;

    public rendered: angular.IPromise<any>;
}
export class LocalizationServiceMock implements ILocalizationService {
    public get(name: string): string {
        return '';
    }
}

describe("ConfirmationDialogCtrl", () => {
    describe("accept", () => {
        it("return true", () => {
            // Arrange
            var serviceInstanceMock = new ModalServiceInstanceMock();
            var localizationMock = new LocalizationServiceMock();
            var confirmationDialogCtrl = new ConfirmationDialogCtrl(serviceInstanceMock, localizationMock);
            
            // Act
            confirmationDialogCtrl.accept();

            // Assert
            expect(serviceInstanceMock.actualResult).toBe(true, "result is false");
        });
    });

    describe("cancel", () => {
        it("return false", () => {
            // Arrange
            var serviceInstanceMock = new ModalServiceInstanceMock();
            var localizationMock = new LocalizationServiceMock();
            var confirmationDialogCtrl = new ConfirmationDialogCtrl(serviceInstanceMock, localizationMock);
            
            // Act
            confirmationDialogCtrl.cancel();

            // Assert
            expect(serviceInstanceMock.actualResult).toBe(false, "result is true");
        });
    });
});