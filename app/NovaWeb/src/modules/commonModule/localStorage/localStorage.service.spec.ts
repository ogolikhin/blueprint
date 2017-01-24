import "./";
import "angular-mocks";
import {LocalStorageService, ILocalStorageService} from "./localStorage.service";

describe("LocalStorage", () => {

    beforeEach(angular.mock.module("localStorage"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localStorageService", LocalStorageService);
    }));

    describe("LocalStorage", () => {

        it("is created", inject((localStorageService:ILocalStorageService) => {
            // Assert
            expect(localStorageService).not.toBeNull();
        }));

        it("read null", inject((localStorageService:ILocalStorageService) => {
            spyOn(localStorage, "getItem").and.callFake((key: string): String => {
                return null;
            });

            const readResult = localStorageService.read(null);
            // Assert
            expect(readResult).toBeNull();
        }));

        it("read not null", inject((localStorageService:ILocalStorageService) => {
            const customKey = "key";
            const customValue = "value";
            spyOn(localStorage, "getItem").and.callFake((key: string): String => {
                if (key && key === customKey) {
                    return customValue;
                }
                return null;
            });

            const readResult = localStorageService.read(customKey);
            // Assert
            expect(readResult).toEqual(customValue);
        }));

        it("read object", inject((localStorageService:ILocalStorageService) => {
            const customKey = "key";
            const customValue = {
                a: "a",
                b: 5
            };
            spyOn(localStorage, "getItem").and.callFake((key: string): any => {
                if (key && key === customKey) {
                    return JSON.stringify(customValue);
                }
                return null;
            });

            const readResult = localStorageService.readObject(customKey);
            // Assert
            expect(readResult).toEqual(customValue);
        }));

        it("write item", inject((localStorageService:ILocalStorageService) => {
            const customKey = "key";
            const customValue = "value";
            const setItemSpy = spyOn(localStorage, "setItem").and.callFake((key: string, data: string): void => {
                //
            });

            localStorageService.write(customKey, customValue);
            // Assert
            expect(setItemSpy).toHaveBeenCalledWith(customKey, customValue);
        }));

        it("write object", inject((localStorageService:ILocalStorageService) => {
            const customKey = "key";
            const customValue = {
                a: "a",
                b: 5
            };
            const setItemSpy = spyOn(localStorage, "setItem").and.callFake((key: string, data: string): void => {
                //
            });

            localStorageService.writeObject(customKey, customValue);
            // Assert
            expect(setItemSpy).toHaveBeenCalledWith(customKey, JSON.stringify(customValue));
        }));

        it("remove item", inject((localStorageService:ILocalStorageService) => {
            const customKey = "key";
            const removeSpy = spyOn(localStorage, "removeItem").and.callFake((path: string): void => {
                //
            });

            localStorageService.remove(customKey);
            // Assert
            expect(removeSpy).toHaveBeenCalledWith(customKey);
        }));

        it("clear", inject((localStorageService:ILocalStorageService) => {
            const clearSpy = spyOn(localStorage, "clear").and.callFake((): void => {
                //
            });

            localStorageService.clear();
            // Assert
            expect(clearSpy).toHaveBeenCalled();
        }));
    });
});
