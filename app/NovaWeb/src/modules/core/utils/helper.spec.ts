import {Helper} from "./helper";

describe("getFirstBrowserLanguage", () => {
    it("returns a value", function () {
        // Arrange
        let language;

        // Act
        language = Helper.getFirstBrowserLanguage();

        // Assert
        expect(language).not.toBeNull();
    });
});

describe("isFontFaceSupported", () => {
    it("is true", function () {
        // Arrange
        let isSupported;

        // Act
        isSupported = Helper.isFontFaceSupported();

        // Assert
        expect(isSupported).toBeTruthy();
    });
});

describe("isWebfontAvailable", () => {
    it("returns false for non-existing fonts", function () {
        // Arrange
        let isAvailable;

        // Act
        isAvailable = Helper.isWebfontAvailable("%$%^$&^$&");

        // Assert
        expect(isAvailable).toBeFalsy();
    });

    it("returns true for non-standard fonts", function () {
        // Arrange
        let isAvailable;

        // Act
        isAvailable = Helper.isWebfontAvailable("symbol");

        // Assert
        expect(isAvailable).toBeTruthy();
    });
});