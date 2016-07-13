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

describe("camelCase", () => {
    it("change the case of 'Bp-Tooltip'", () => {
        // Arrange
        let string = "Bp-Tooltip";

        // Act
        string = Helper.camelCase(string);

        // Assert
        expect(string).toEqual("bpTooltip");

    });
});