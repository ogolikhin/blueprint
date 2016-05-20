import "angular";
import "angular-mocks";
import {IProjectNotification, ProjectNotification, SubscriptionEnum} from "./project-notification";


describe("Global Notification", () => {
    let notificator: IProjectNotification;
    beforeEach(() => {
        notificator = new ProjectNotification();
    });

    it("subscribe to notification", () => {
        // Arrange
        let value: number = 1;
        let func = function (delta: number) {
            value += delta;
        };
        notificator.subscribe(SubscriptionEnum.ProjectLoad, func);

        // Act
        notificator.notify(SubscriptionEnum.ProjectLoad, 10);

        // Assert
        expect(value).toBe(11);
    });

    it("notify with parameters ", () => {
        // Arrange
        let value: string = "";
        let func = function (one: string, two: string, three: string, four: string) {
            value += `${one} ${two} ${three} ${four}`;
        };
        notificator.subscribe(SubscriptionEnum.ProjectLoad, func);

        // Act
        notificator.notify(SubscriptionEnum.ProjectLoad, "this", "is", "a", "test");

        // Assert
        expect(value).toBe("this is a test");
    });

    it("notify invalid", () => {
        // Arrange
        let value: string = "";
        let func = function (one: string, two: string, three: string, four: string) {
            value += `${one} ${two} ${three} ${four}`;
        };
        notificator.subscribe(SubscriptionEnum.ProjectLoad, func);

        // Act
        notificator.notify(SubscriptionEnum.ProjectLoaded, "this", "is", "a", "test");

        // Assert
        expect(value).toBe("");
    });
    it("unsubsribe successfull", () => {
        // Arrange
        let value: string = "";
        let func = function (one: string) {
            value += one;
        };
        notificator.subscribe(SubscriptionEnum.ProjectLoad, func);

        // Act
        notificator.unsubscribe(SubscriptionEnum.ProjectLoad, func);

        // Assert
        // Assert
        expect(notificator["handlers"]).toEqual(jasmine.any(Array));
        expect(notificator["handlers"].length).toBe(0);
    });
    it("unsubsribe successfull 2 handlers", () => {
        // Arrange
        let value: string = "";
        let func = function (one: string) {
            value += one;
        };
        notificator.subscribe(SubscriptionEnum.ProjectLoad, func);
        notificator.subscribe(SubscriptionEnum.ProjectLoad, func);

        // Act
        notificator.unsubscribe(SubscriptionEnum.ProjectLoad, func);

        // Assert
        // Assert
        expect(notificator["handlers"]).toEqual(jasmine.any(Array));
        expect(notificator["handlers"].length).toBe(0);
    });

    it("unsubsribe unsuccessfull", () => {
        // Arrange
        let value: string = "";
        let func = function (one: string) {
            value += one;
        };
        notificator.subscribe(SubscriptionEnum.ProjectLoad, func);

        // Act
        notificator.unsubscribe(SubscriptionEnum.ProjectLoaded, func);

        // Assert
        // Assert
        expect(notificator["handlers"]).toEqual(jasmine.any(Array));
        expect(notificator["handlers"].length).toBe(1);
        //let handler = notificator["handlers"][0];
        //expect(handler).toBeDefined();
        //expect(handler.callbacks).toEqual(jasmine.any(Array));
        //expect(handler.callbacks.length).toBe(1);
    });

});
