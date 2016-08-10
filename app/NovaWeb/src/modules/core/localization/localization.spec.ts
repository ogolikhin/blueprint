//import {LocalizationService} from "./";



//describe("getFirstBrowserLanguage", () => {
//    it("returns a value", function () {
//        // Arrange
//        let language;

//        // Act
//        language = LocalizationService.getBrowserLanguage();

//        // Assert
//        expect(language).not.toBeNull();
//    });
//});

////describe("uiDatePickerFormatAdaptor", () => {
////    it("should correctly parse en-US format", () => {
////        // Arrange/Act
////        let format = Helper.uiDatePickerFormatAdaptor("MM/DD/YYYY");

////        // Assert
////        expect(format).toEqual("MM/dd/yyyy");
////    });

////    it("should correctly parse zh-TW format", () => {
////        // Arrange/Act
////        let format = Helper.uiDatePickerFormatAdaptor("YYYY年MMMD日");

////        // Assert
////        expect(format).toEqual("yyyy年MMMd日");
////    });

////    it("should correctly parse ar format", () => {
////        // Arrange/Act
////        let format = Helper.uiDatePickerFormatAdaptor("D/\u200FM/\u200FYYYY");

////        // Assert
////        expect(format).toEqual("d/M/yyyy");
////    });
////});

//describe("toStartOfTZDay", () => {
//    it("should remove the time part from a date", () => {
//        let date = new Date("2016-08-01T00:30:00.0000000");

//        let parsed = LocalizationService..toStartOfTZDay(date);

//        expect(parsed).not.toBe(date);
//        expect(parsed.getDate()).toBe(1);
//    });
//});


////describe("getDecimalSeparator", () => {
////    it("shows the decimal separator based on locale (US)", () => {
////        // Arrange/Act
////        let separator = Helper.getDecimalSeparator("en-US");

////        // Assert
////        expect(separator).toEqual(".");
////    });

////    // see https://github.com/ariya/phantomjs/issues/12581#issuecomment-166645579
////    if (!/PhantomJS/.test(window.navigator.userAgent)) {
////        it("shows the decimal separator based on locale (IT)", () => {
////            // Arrange/Act
////            let separator = Helper.getDecimalSeparator("it-IT");

////            // Assert
////            expect(separator).toEqual(",");
////        });
////    }
////});

//describe("toLocaleNumber", () => {
//    it("converts to proper number based on locale (US)", () => {
//        // Arrange/Act
//        let number = LocalizationService.toLocaleNumber(123456.789, "en-US");

//        // Assert
//        expect(number).toEqual("123456.789");
//    });

//    it("converts to proper negative number based on locale (US)", () => {
//        // Arrange/Act
//        let number = Helper.toLocaleNumber(-123456.789, "en-US");

//        // Assert
//        expect(number).toEqual("-123456.789");
//    });

//    // see https://github.com/ariya/phantomjs/issues/12581#issuecomment-166645579
//    if (!/PhantomJS/.test(window.navigator.userAgent)) {
//        it("converts to proper number based on locale (IT)", () => {
//            // Arrange/Act
//            let number = Helper.toLocaleNumber(123456.789, "it-IT");

//            // Assert
//            expect(number).toEqual("123456,789");
//        });

//        it("converts to proper negative number based on locale (IT)", () => {
//            // Arrange/Act
//            let number = Helper.toLocaleNumber(-123456.789, "it-IT");

//            // Assert
//            expect(number).toEqual("-123456,789");
//        });
//    }

//    it("doesn't convert bad formatted NaN", () => {
//        // Arrange/Act
//        let number = Helper.toLocaleNumber(NaN);

//        // Assert
//        expect(number).toBeNull();
//    });

//    it("doesn't convert null", () => {
//        // Arrange/Act
//        let number = Helper.toLocaleNumber(null);

//        // Assert
//        expect(number).toBeNull();
//    });

//    it("doesn't convert undefined", () => {
//        // Arrange/Act
//        let number = Helper.toLocaleNumber(undefined);

//        // Assert
//        expect(number).toBeNull();
//    });
//});

//describe("parseLocaleNumber", () => {
//    it("converts to proper number based on locale (US)", () => {
//        // Arrange/Act
//        let number = Helper.parseLocaleNumber("123,456.789", "en-US");

//        // Assert
//        expect(number).toEqual(123456.789);
//    });

//    it("converts to proper negative number based on locale (US)", () => {
//        // Arrange/Act
//        let number = Helper.parseLocaleNumber("-123,456.789", "en-US");

//        // Assert
//        expect(number).toEqual(-123456.789);
//    });

//    // see https://github.com/ariya/phantomjs/issues/12581#issuecomment-166645579
//    if (!/PhantomJS/.test(window.navigator.userAgent)) {
//        it("converts to proper number based on locale (IT)", () => {
//            // Arrange/Act
//            let number = Helper.parseLocaleNumber("123.456,789", "it-IT");

//            // Assert
//            expect(number).toEqual(123456.789);
//        });

//        it("converts to proper negative number based on locale (IT)", () => {
//            // Arrange/Act
//            let number = Helper.parseLocaleNumber("-123.456,789", "it-IT");

//            // Assert
//            expect(number).toEqual(-123456.789);
//        });
//    }

//    it("doesn't parse bad formatted number string", () => {
//        // Arrange/Act
//        let number = Helper.parseLocaleNumber("abcdefg");

//        // Assert
//        expect(number).toBeNaN();
//    });

//    it("doesn't parse null", () => {
//        // Arrange/Act
//        let number = Helper.parseLocaleNumber(null);

//        // Assert
//        expect(number).toBeNaN();
//    });

//    it("doesn't parse undefined", () => {
//        // Arrange/Act
//        let number = Helper.parseLocaleNumber(undefined);

//        // Assert
//        expect(number).toBeNaN();
//    });
//});

//describe("uiDatePickerFormatAdaptor", () => {
//    it("should correctly parse en-US format", () => {
//        // Arrange/Act
//        let format = Helper.uiDatePickerFormatAdaptor("MM/DD/YYYY");

//        // Assert
//        expect(format).toEqual("MM/dd/yyyy");
//    });

//    it("should correctly parse zh-TW format", () => {
//        // Arrange/Act
//        let format = Helper.uiDatePickerFormatAdaptor("YYYY年MMMD日");

//        // Assert
//        expect(format).toEqual("yyyy年MMMd日");
//    });

//    it("should correctly parse ar format", () => {
//        // Arrange/Act
//        let format = Helper.uiDatePickerFormatAdaptor("D/\u200FM/\u200FYYYY");

//        // Assert
//        expect(format).toEqual("d/M/yyyy");
//    });
//});


//describe("toStartOfTZDay", () => {
//    it("should remove the time part from a date", () => {
//        let date = new Date("2016-08-01T00:30:00.0000000");

//        let parsed = Helper.toStartOfTZDay(date);

//        expect(parsed).not.toBe(date);
//        expect(parsed.getDate()).toBe(1);
//    });
//});
