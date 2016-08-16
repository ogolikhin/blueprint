import "angular";
import "angular-mocks";

import { SettingsService, ISettingsService } from "./";

describe("Settings", () => {
    let settings: ISettingsService;

    beforeEach(inject(($rootScope: ng.IRootScopeService) => {
        settings = new SettingsService($rootScope);
        $rootScope["config"] = {
            settings: {
                "string": "stringValue",
                "number": "5",
                "boolean": "true",
                "object": "{ \"string\": \"s\", \"number\": 5, \"array\": [\"s\", 5], \"boolean\": true }"
            }
        };
    }));

    describe("get", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            let value = settings.get("string");

            // Assert
            expect(value).toEqual("stringValue");
        });
        it("returns undefined if setting doesn't exist", () => {
            // Arrange

            // Act
            let value = settings.get("");

            // Assert
            expect(value).toBeUndefined();
        });
        it("returns specified default value if setting doesn't exist", () => {
            // Arrange
            let expected = "123";

            // Act
            let value = settings.get("", expected);

            // Assert
            expect(value).toBe(expected);
        });
    });

    describe("getNumber", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            let value = settings.getNumber("number");

            // Assert
            expect(value).toEqual(5);
        });
        it("returns undefined if setting doesn't exist", () => {
            // Arrange

            // Act
            let value = settings.getNumber("");

            // Assert
            expect(value).toBeUndefined();
        });
        it("returns specified default value if setting doesn't exist", () => {
            // Arrange
            let expected = 123;

            // Act
            let value = settings.getNumber("", expected);

            // Assert
            expect(value).toBe(expected);
        });
        it("returns specified minValue if setting value is lower", () => {
            // Arrange
            let minValue = 10;

            // Act
            let value = settings.getNumber("number", 100, minValue);

            // Assert
            expect(value).toBe(minValue);
        });
        it("returns specified maxValue if setting value is higher", () => {
            // Arrange
            let maxValue = 2;

            // Act
            let value = settings.getNumber("number", 1, 0, maxValue);

            // Assert
            expect(value).toBe(maxValue);
        });
    });

    describe("getBoolean", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            let value = settings.getBoolean("boolean");

            // Assert
            expect(value).toEqual(true);
        });
        it("returns undefined if setting doesn't exist", () => {
            // Arrange

            // Act
            let value = settings.getBoolean("");

            // Assert
            expect(value).toBeUndefined();
        });
        it("returns specified default value if setting doesn't exist", () => {
            // Arrange
            let expected = false;

            // Act
            let value = settings.getBoolean("", expected);

            // Assert
            expect(value).toBe(expected);
        });
    });

    describe("getObject", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            let value = settings.getObject("object");

            // Assert
            expect(value).toEqual({ string: "s", number: 5, array: ["s", 5], boolean: true });
        });
        it("returns undefined if setting doesn't exist", () => {
            // Arrange

            // Act
            let value = settings.getObject("");

            // Assert
            expect(value).toBeUndefined();
        });
        it("returns specified default value if setting doesn't exist", () => {
            // Arrange
            let expected = { value: 123 };

            // Act
            let value = settings.getObject("", expected);

            // Assert
            expect(value).toBe(expected);
        });
    });
});
