import "angular";
import "angular-mocks";
import {SettingsService, ISettingsService} from "./settings";

describe("Settings", () => {
    let settings: ISettingsService;

    beforeEach(inject(($rootScope: ng.IRootScopeService) => {
        settings = new SettingsService($rootScope);
        $rootScope["config"] = {
            settings: {
                "string": "stringValue",
                "number": "5",
                "invalidNumber": "0x5",
                "boolean": "false",
                "invalidBoolean": "NaN",
                "object": "{ \"string\": \"s\", \"number\": 5, \"array\": [\"s\", 5], \"boolean\": true }",
                "invalidObject": "{ 'string': 's', 'number': 5, 'array': ['s', 5], 'boolean': true }"
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
            let defaultValue = "123";

            // Act
            let value = settings.get("", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
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
            let defaultValue = 123;

            // Act
            let value = settings.getNumber("", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
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
            let value = settings.getNumber("number", 1, undefined, maxValue);

            // Assert
            expect(value).toBe(maxValue);
        });
        it("returns specified default value if setting is not a valid number", () => {
            // Arrange
            let defaultValue = 123;

            // Act
            let value = settings.getNumber("invalidNumber", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("throws error if strict is specified and setting is not a valid number", () => {
            // Arrange

            // Act
            let action = () => settings.getNumber("invalidNumber", undefined, undefined, undefined, true);

            // Assert
            expect(action).toThrowError("Value '0x5' for key 'invalidNumber' is not a valid number");
        });
    });

    describe("getBoolean", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            let value = settings.getBoolean("boolean");

            // Assert
            expect(value).toEqual(false);
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
            let defaultValue = false;

            // Act
            let value = settings.getBoolean("", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("returns specified default value if setting is not a valid boolean", () => {
            // Arrange
            let defaultValue = false;

            // Act
            let value = settings.getBoolean("invalidBoolean", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("throws error if strict is specified and setting is not a valid boolean", () => {
            // Arrange

            // Act
            let action = () => settings.getBoolean("invalidBoolean", undefined, true);

            // Assert
            expect(action).toThrowError("Value 'NaN' for key 'invalidBoolean' is not a valid boolean");
        });
    });

    describe("getObject", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            let value = settings.getObject("object");

            // Assert
            expect(value).toEqual({string: "s", number: 5, array: ["s", 5], boolean: true});
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
            let defaultValue = {value: 123};

            // Act
            let value = settings.getObject("", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("returns specified default value if setting is not a valid object", () => {
            // Arrange
            let defaultValue = {value: 123};

            // Act
            let value = settings.getObject("invalidObject", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("throws error if strict is specified and setting is not a valid object", () => {
            // Arrange

            // Act
            let action = () => settings.getObject("invalidObject", undefined, true);

            // Assert
            expect(action)
                .toThrowError("Value '{ 'string': 's', 'number': 5, 'array': ['s', 5], 'boolean': true }' for key 'invalidObject' is not a valid object");
        });
    });
});
