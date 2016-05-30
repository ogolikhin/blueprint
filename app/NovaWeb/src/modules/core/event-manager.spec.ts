import "angular";
import "angular-mocks";
import {IEventManager, EventManager, EventSubscriber} from "./event-manager";

 
describe("Global Notification", () => {
    let eventManager: IEventManager;
    let hostId: EventSubscriber = EventSubscriber.Main ;
    
    beforeEach(() => {
        eventManager = new EventManager();
    });

    it("attach undefined notifications", () => {
        // Arrange
        let first;
        eventManager.attach(hostId, "first", first);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toEqual(0);

    });
    it("attach notifications with host signature", () => {
        // Arrange
        let first = function (delta: number) { };
        eventManager.attach(undefined, "first", first);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toEqual(1);

    });
    it("attach notifications with undefined event signatute", () => {
        // Arrange
        let first = function (delta: number) { };
        eventManager.attach(hostId, undefined, first);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toEqual(0);

    });

    it("add notifications", () => {
        // Arrange
        let first = function (delta: number) {
        };
        eventManager.attach(hostId, "first", first);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toEqual(1);
        expect(eventManager["handlers"][0].name).toEqual(EventSubscriber[hostId].toLowerCase() + ".first");

    });
    it("add 3 notifications", () => {
        // Arrange
        let first = function (delta: number) {
        };
        eventManager.attach(hostId, "first", first);
        eventManager.attach(hostId, "first", first);
        eventManager.attach(hostId, "first", first);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toBe(1);
        let handler = eventManager["handlers"][0];
        expect(handler).toBeDefined();
        expect(handler.callbacks).toEqual(jasmine.any(Array));
        expect(handler.callbacks.length).toBe(3);

    });
    it("attach and detach notifications", () => {
        // Arrange
        let first = function (delta: number) {
        };
        eventManager.attach(hostId, "first", first);
        eventManager.detach(hostId, "first", first);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toBe(0);
    });
    it("detach unsuccessful", () => {
        // Arrange
        let first = function (delta: number) {
        };
        let second = function (delta: number) {
        };
        eventManager.attach(hostId, "first", first);

        eventManager.detach(undefined, "first", second);
        eventManager.detach(hostId, undefined, first);
        eventManager.detach(hostId, "first", undefined);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toBe(1);
    });

    it("detach unsuccessful", () => {
        // Arrange
        let first = function (delta: number) {
        };
        let second = function (delta: number) {
        };
        eventManager.attach(hostId, "first", first);
        eventManager.detach(hostId, "first", second);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toBe(1);
    });

    it("attach 2 and detach 1 notifications", () => {
        // Arrange
        let first = function (delta: number) {
        };
        let second = function (delta: any) {
        };
        eventManager.attach(hostId, "first", first);
        eventManager.attach(hostId, "first", second);
        eventManager.detach(hostId, "first", first);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toBe(1);
        let handler = eventManager["handlers"][0];
        expect(handler).toBeDefined();
        expect(handler.callbacks).toEqual(jasmine.any(Array));
        expect(handler.callbacks.length).toBe(1);
    });


    it("dispatch successful", () => {
        // Arrange
        let value: number = 1;
        let func = function (delta: number) {
            value += delta;
        };
        eventManager.attach(hostId, "first", func);

        // Act
        eventManager.dispatch(hostId, "first", 10);

        // Assert
        expect(value).toBe(11);
    });

    it("dispatch unsuccessful (incorrect signature)", () => {
        // Arrange
        let value: number = 1;
        let func = function (delta: number) {
            value += delta;
        };
        eventManager.attach(hostId, "first", func);

        // Act
        eventManager.dispatch(undefined, "first", 10);

        // Assert
        expect(value).toBe(1);
    });

    it("dispatch unsuccessful ", () => {
        // Arrange
        let value: number = 1;
        let func = function (delta: number) {
            value += delta;
        };
        eventManager.attach(hostId, "first", func);

        // Act
        eventManager.dispatch(hostId, "invalid", 10);

        // Assert
        expect(value).toBe(1);
    });

    it("destroy notifications", () => {
        // Arrange
        let func = function (delta: number) {
        };
        eventManager.attach(EventSubscriber.Main, "first", func);
        eventManager.attach(EventSubscriber.Main, "second", func);
        eventManager.attach(EventSubscriber.ProjectManager, "first", func);
        eventManager.attach(EventSubscriber.ProjectManager, "second", func);

        // Act
        eventManager.destroy(EventSubscriber.Main);

        // Assert
        expect(eventManager["handlers"]).toEqual(jasmine.any(Array));
        expect(eventManager["handlers"].length).toBe(2);
        // Act
        eventManager.destroy();
        expect(eventManager["handlers"].length).toBe(0);

    });
    it("destroy all notifications", () => {
        // Arrange
        let func = function (delta: number) {
        };
        eventManager.attach(EventSubscriber.Main, "first", func);
        eventManager.attach(EventSubscriber.Main, "second", func);
        eventManager.attach(EventSubscriber.ProjectManager, "first", func);
        eventManager.attach(EventSubscriber.ProjectManager, "second", func);

        // Act
        eventManager.destroy();

        // Assert
        expect(eventManager["handlers"].length).toBe(0);

    });

});
