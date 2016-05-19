//import "angular";
//import "angular-mocks";
//import {IProjectNotification, ProjectNotification, SubscriptionEnum} from "./project-notification";


//describe("Project Notification", () => {

//    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//        $provide.service("projectService", ProjectService);
//        //$provide.service("projectNotification", ProjectNotificationMock);
//        //$provide.service("localization", LocalizationServiceMock);
//    }));

//    describe("open method", () => {
//        it("simple open dialog ", inject((dialogService: $D.IDialogService) => {
//            // Arrange
//            var spy = spyOn(dialogService, "openInternal").and.callThrough();
//            var settings: $D.IDialogSettings = {};

//            // Act
//            dialogService.open(settings);

//            // Assert
//            expect(spy).toHaveBeenCalled();
//        }));
//    });

//});
