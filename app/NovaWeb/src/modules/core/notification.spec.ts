import "angular";
import "angular-mocks";
import {INotificationService, NotificationService} from "./notification";

 
describe("Global Notification", () => {
    let notificator: INotificationService;
    let hostId = "host";
    beforeEach(()=> {
        notificator = new NotificationService();
    });

    it("add notifications", () => {
        // Arrange
        let first = function (delta:number) {
        }
        notificator.attach(hostId,"first", first);

        // Assert
        expect(notificator["handlers"]).toEqual(jasmine.any(Array));
        expect(notificator["handlers"].length).toEqual(1);
        expect(notificator["handlers"][0].name).toEqual("first");

    });
    it("add 3 notifications", () => {
        // Arrange
        let first = function (delta: number) {
        }
        notificator.attach(hostId, "first", first);
        notificator.attach(hostId, "first", first);
        notificator.attach(hostId, "first", first);

        // Assert
        expect(notificator["handlers"]).toEqual(jasmine.any(Array));
        expect(notificator["handlers"].length).toBe(1);
        let handler = notificator["handlers"][0];
        expect(handler).toBeDefined();
        expect(handler.callbacks).toEqual(jasmine.any(Array));
        expect(handler.callbacks.length).toBe(3);

    });
    it("attach and detach notifications", () => {
        // Arrange
        let first = function (delta: number) {
        }
        notificator.attach(hostId, "first", first);
        notificator.detach(hostId, "first", first);

        // Assert
        expect(notificator["handlers"]).toEqual(jasmine.any(Array));
        expect(notificator["handlers"].length).toBe(0);
    });
    it("detach invalid", () => {
        // Arrange
        let first = function (delta: number) {
        }
        let second = function (delta: number) {
        }
        notificator.attach(hostId, "first", first);
        notificator.detach(hostId, "first", second);

        // Assert
        expect(notificator["handlers"]).toEqual(jasmine.any(Array));
        expect(notificator["handlers"].length).toBe(1);
    });

    it("attach 2 and detach 1 notifications", () => {
        // Arrange
        let first = function (delta: number) {
        }
        let second = function (delta: any) {
        }
        notificator.attach(hostId, "first", first);
        notificator.attach(hostId, "first", second);
        notificator.detach(hostId, "first", first);

        // Assert
        expect(notificator["handlers"]).toEqual(jasmine.any(Array));
        expect(notificator["handlers"].length).toBe(1);
        let handler = notificator["handlers"][0];
        expect(handler).toBeDefined();
        expect(handler.callbacks).toEqual(jasmine.any(Array));
        expect(handler.callbacks.length).toBe(1);
    });


    it("dispatch notifications", () => {
        // Arrange
        let value: number = 1;
        let func = function (delta: number) {
            value += delta;
        }
        notificator.attach(hostId, "first", func);

        // Act
        notificator.dispatch(hostId, "first", 10);

        // Assert
        expect(value).toBe(11);
    });

    it("dispatch and not executed", () => {
        // Arrange
        let value: number = 1;
        let func = function (delta: number) {
            value += delta;
        }
        notificator.attach(hostId, "first", func);

        // Act
        notificator.dispatch(hostId, "invalid", 10);

        // Assert
        expect(value).toBe(1);
    });

});
