import './';
import "angular-mocks";
import {SettingsService, ISettingsService} from "./settings.service";
describe("Settings", () => {
    let settings: ISettingsService;

    beforeEach(angular.mock.module("configuration"));

    beforeEach(inject(($rootScope: ng.IRootScopeService) => {
        settings = new SettingsService($rootScope);
        $rootScope["config"] = {
            settings: {
                "string": "stringValue",
                "number": "5",
                "negativeNumber": "-5",
                "invalidNumber": "0x5",
                "boolean": "false",
                "booleanFalseMixedCase": "FaLse",
                "booleanTrueMixedCase": "TrUe",
                "invalidBoolean": "NaN",
                "object": "{\"string\": \"s\", \"number\": 5, \"array\": [\"s\", 5], \"boolean\": true}",
                "invalidObject": "{'string': 's', 'number': 5, 'array': ['s', 5], 'boolean': true}"
            }
        };
    }));

    describe("get", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            const value = settings.get("string");

            // Assert
            expect(value).toEqual("stringValue");
        });
        it("returns undefined if setting doesn't exist", () => {
            // Arrange

            // Act
            const value = settings.get("");

            // Assert
            expect(value).toBeUndefined();
        });
        it("returns specified default value if setting doesn't exist", () => {
            // Arrange
            const defaultValue = "123";

            // Act
            const value = settings.get("", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
    });

    describe("getNumber", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            const value = settings.getNumber("number");

            // Assert
            expect(value).toEqual(5);
        });
        it("returns undefined if setting doesn't exist", () => {
            // Arrange

            // Act
            const value = settings.getNumber("");

            // Assert
            expect(value).toBeUndefined();
        });
        it("returns specified default value if setting doesn't exist", () => {
            // Arrange
            const defaultValue = 123;

            // Act
            const value = settings.getNumber("", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("returns specified minValue if setting value is lower", () => {
            // Arrange
            const minValue = 10;

            // Act
            const value = settings.getNumber("number", 100, minValue);

            // Assert
            expect(value).toBe(minValue);
        });
        it("returns specified minValue if setting value is lower - minValue is 0", () => {
            // Arrange
            const minValue = 0;

            // Act
            const value = settings.getNumber("negativeNumber", 100, minValue);

            // Assert
            expect(value).toBe(minValue);
        });
        it("returns specified maxValue if setting value is higher", () => {
            // Arrange
            const maxValue = 2;

            // Act
            const value = settings.getNumber("number", 1, undefined, maxValue);

            // Assert
            expect(value).toBe(maxValue);
        });
        it("returns specified maxValue if setting value is higher - maxValue is 0", () => {
            // Arrange
            const maxValue = 0;

            // Act
            const value = settings.getNumber("number", 1, undefined, maxValue);

            // Assert
            expect(value).toBe(maxValue);
        });
        it("returns specified default value if setting is not a valid number", () => {
            // Arrange
            const defaultValue = 123;

            // Act
            const value = settings.getNumber("invalidNumber", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("returns specified value if between min and max", () => {
            // Arrange
            const minValue = 4;
            const maxValue = 6;

            // Act
            const value = settings.getNumber("number", 1, minValue, maxValue);

            // Assert
            expect(value).toEqual(5);
        });
        it("throws error if strict is specified and setting is not a valid number", () => {
            // Arrange

            // Act
            const action = () => settings.getNumber("invalidNumber", undefined, undefined, undefined, true);

            // Assert
            expect(action).toThrowError("Value '0x5' for key 'invalidNumber' is not a valid number");
        });
    });

    describe("getBoolean", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            const value = settings.getBoolean("boolean");

            // Assert
            expect(value).toEqual(false);
        });
        it("returns undefined if setting doesn't exist", () => {
            // Arrange

            // Act
            const value = settings.getBoolean("");

            // Assert
            expect(value).toBeUndefined();
        });
        it("returns specified default value if setting doesn't exist", () => {
            // Arrange
            const defaultValue = false;

            // Act
            const value = settings.getBoolean("", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("returns specified default value if setting is not a valid boolean", () => {
            // Arrange
            const defaultValue = false;

            // Act
            const value = settings.getBoolean("invalidBoolean", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("throws error if strict is specified and setting is not a valid boolean", () => {
            // Arrange

            // Act
            const action = () => settings.getBoolean("invalidBoolean", undefined, true);

            // Assert
            expect(action).toThrowError("Value 'NaN' for key 'invalidBoolean' is not a valid boolean");
        });
        it("is case-insensitive for false checks", () => {
            //Arrange

            //Act
            const value = settings.getBoolean("booleanFalseMixedCase", undefined, true);

            //Expect
            expect(value).toBe(false);
        });
        it("is case-insensitive for true checks", () => {
            //Arrange

            //Act
            const value = settings.getBoolean("booleanTrueMixedCase", undefined, true);

            //Expect
            expect(value).toBe(true);
        });
    });

    describe("getObject", () => {
        it("returns existing value", () => {
            // Arrange

            // Act
            const value = settings.getObject("object");

            // Assert
            expect(value).toEqual({string: "s", number: 5, array: ["s", 5], boolean: true});
        });
        it("returns undefined if setting doesn't exist", () => {
            // Arrange

            // Act
            const value = settings.getObject("");

            // Assert
            expect(value).toBeUndefined();
        });
        it("returns specified default value if setting doesn't exist", () => {
            // Arrange
            const defaultValue = {value: 123};

            // Act
            const value = settings.getObject("", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("returns specified default value if setting is not a valid object", () => {
            // Arrange
            const defaultValue = {value: 123};

            // Act
            const value = settings.getObject("invalidObject", defaultValue);

            // Assert
            expect(value).toBe(defaultValue);
        });
        it("throws error if strict is specified and setting is not a valid object", () => {
            // Arrange

            // Act
            const action = () => settings.getObject("invalidObject", undefined, true);

            // Assert
            expect(action)
                .toThrowError("Value '{'string': 's', 'number': 5, 'array': ['s', 5], 'boolean': true}' for key 'invalidObject' is not a valid object");
        });
    });
});
