import "angular";
import "lodash";
import "angular-mocks";
import "angular-ui-router";
import {LocalStorageService, ILocalStorageService} from "./localStorage.service";

describe("LocalStorage", () => {

    let localizationStorageService: ILocalStorageService;

    beforeEach(inject(($log: ng.ILogService) => {
        localizationStorageService = new LocalStorageService($log);
    }));

    describe("LocalStorage", () => {

        it("is created", () => {
            // Assert
            expect(localizationStorageService).not.toBeNull();
        });

        it("read null", () => {
            spyOn(localStorage, "getItem").and.callFake((key: string): String => {
                return null;
            });

            const readResult = localizationStorageService.read(null);
            // Assert
            expect(readResult).toBeNull();
        });

        it("read not null", () => {
            const customKey = "key";
            const customValue = "value";
            spyOn(localStorage, "getItem").and.callFake((key: string): String => {
                if (key && key === customKey) {
                    return customValue;
                }
                return null;
            });

            const readResult = localizationStorageService.read(customKey);
            // Assert
            expect(readResult).toEqual(customValue);
        });

        it("read object", () => {
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

            const readResult = localizationStorageService.readObject(customKey);
            // Assert
            expect(readResult).toEqual(customValue);
        });

        it("write item", () => {
            const customKey = "key";
            const customValue = "value";
            const setItemSpy = spyOn(localStorage, "setItem").and.callFake((key: string, data: string): void => {
                //
            });

            localizationStorageService.write(customKey, customValue);
            // Assert
            expect(setItemSpy).toHaveBeenCalledWith(customKey, customValue);
        });

        it("write object", () => {
            const customKey = "key";
            const customValue = {
                a: "a",
                b: 5
            };
            const setItemSpy = spyOn(localStorage, "setItem").and.callFake((key: string, data: string): void => {
                //
            });

            localizationStorageService.writeObject(customKey, customValue);
            // Assert
            expect(setItemSpy).toHaveBeenCalledWith(customKey, JSON.stringify(customValue));
        });

        it("remove item", () => {
            const customKey = "key";
            const removeSpy = spyOn(localStorage, "removeItem").and.callFake((path: string): void => {
                //
            });

            localizationStorageService.remove(customKey);
            // Assert
            expect(removeSpy).toHaveBeenCalledWith(customKey);
        });

        it("clear", () => {
            const clearSpy = spyOn(localStorage, "clear").and.callFake((): void => {
                //
            });

            localizationStorageService.clear();
            // Assert
            expect(clearSpy).toHaveBeenCalled();
        });
    });
});
