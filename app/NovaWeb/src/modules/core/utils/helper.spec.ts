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
    it("changes 'Bp-Tooltip' into 'bpTooltip'", () => {
        // Arrange
        let string = "Bp-Tooltip";

        // Act
        string = Helper.camelCase(string);

        // Assert
        expect(string).toEqual("bpTooltip");
    });
});

describe("getDecimalSeparator", () => {
    it("shows the decimal separator based on locale (US)", () => {
        // Arrange/Act
        let separator = Helper.getDecimalSeparator("en-US");

        // Assert
        expect(separator).toEqual(".");
    });

    // see https://github.com/ariya/phantomjs/issues/12581#issuecomment-166645579
    if (!/PhantomJS/.test(window.navigator.userAgent)) {
        it("shows the decimal separator based on locale (IT)", () => {
            // Arrange/Act
            let separator = Helper.getDecimalSeparator("it-IT");

            // Assert
            expect(separator).toEqual(",");
        });
    }
});

describe("parseLocaleNumber", () => {
    it("convert to proper number based on locale (US)", () => {
        // Arrange/Act
        let number = Helper.parseLocaleNumber("123,456.789", "en-US");

        // Assert
        expect(number).toEqual(123456.789);
    });

    it("convert to proper negative number based on locale (US)", () => {
        // Arrange/Act
        let number = Helper.parseLocaleNumber("-123,456.789", "en-US");

        // Assert
        expect(number).toEqual(-123456.789);
    });

    // see https://github.com/ariya/phantomjs/issues/12581#issuecomment-166645579
    if (!/PhantomJS/.test(window.navigator.userAgent)) {
        it("convert to proper number based on locale (IT)", () => {
            // Arrange/Act
            let number = Helper.parseLocaleNumber("123.456,789", "it-IT");

            // Assert
            expect(number).toEqual(123456.789);
        });

        it("convert to proper negative number based on locale (IT)", () => {
            // Arrange/Act
            let number = Helper.parseLocaleNumber("-123.456,789", "it-IT");

            // Assert
            expect(number).toEqual(-123456.789);
        });
    }

    it("doesn't parse bad formatted number string", () => {
        // Arrange/Act
        let number = Helper.parseLocaleNumber("abcdefg");

        // Assert
        expect(number).toBeNaN();
    });

    it("doesn't parse null", () => {
        // Arrange/Act
        let number = Helper.parseLocaleNumber(null);

        // Assert
        expect(number).toBeNaN();
    });

    it("doesn't parse undefined", () => {
        // Arrange/Act
        let number = Helper.parseLocaleNumber(undefined);

        // Assert
        expect(number).toBeNaN();
    });
});