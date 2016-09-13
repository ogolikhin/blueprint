import "angular";
import "angular-mocks";

import {LocalizationService, BPLocale } from "./";

// Some of the following tests can be only run in interactive mode (i.e. with Chrome) as PhantomJS
// doesn't properly support locale-aware functions. These tests are marked as /*NOT PHANTOMJS*/
// See https://github.com/ariya/phantomjs/issues/12581#issuecomment-166645579

describe("Localization", () => {
    
    describe("Language", () => {
        it("is set", function () {
            // Arrange
                        // Act
            let language = LocalizationService.getBrowserLanguage();

            // Assert
            expect(language).not.toBeNull();
        });
    });

    describe("Check current browser settings", () => {
        it("is set", inject(($rootScope: ng.IRootScopeService) => {
            // Arrange
            let service = new LocalizationService($rootScope);

            // Act
            let current = service.current;

            // Assert
            expect(current).toBeDefined();
        }));
    });

    describe("Check locale settings", () => {
        it("EN locale", inject(($rootScope: ng.IRootScopeService) => {
            // Arrange
            let locale = new BPLocale("en-US");
            
            // Act

            // Assert
            expect(locale.decimalSeparator).toBe(".");
            expect(locale.thousandSeparator).toBe(",");
            expect(locale.shortDateFormat).toBe("MM/DD/YYYY");
            expect(locale.longDateFormat).toBe("MM/DD/YYYY h:mm A");
            expect(locale.datePickerDayTitle).toBe("MMMM yyyy");
            expect(locale.datePickerFormat).toBe("MM/dd/yyyy");
        }));

        it("RU locale", inject(($rootScope: ng.IRootScopeService) => {
            // Arrange
            let locale = new BPLocale("ru-RU");
            
            // Act

            // Assert
            /*NOT PHANTOMJS*/
            if (!/PhantomJS/.test(window.navigator.userAgent)) {
                expect(locale.decimalSeparator).toBe(",");
                expect(locale.thousandSeparator).toBe(".");
            }
            expect(locale.shortDateFormat).toBe("DD.MM.YYYY");
            expect(locale.longDateFormat).toBe("DD.MM.YYYY HH:mm");
            expect(locale.datePickerDayTitle).toBe("MMMM yyyy");
            expect(locale.datePickerFormat).toBe("dd.MM.yyyy");
        }));

        it("IT locale", inject(($rootScope: ng.IRootScopeService) => {
            // Arrange
            let locale = new BPLocale("it-IT");

            // Act

            // Assert
            /*NOT PHANTOMJS*/
            if (!/PhantomJS/.test(window.navigator.userAgent)) {
                expect(locale.decimalSeparator).toBe(",");
                expect(locale.thousandSeparator).toBe(".");
            }
            expect(locale.shortDateFormat).toBe("DD/MM/YYYY");
            expect(locale.longDateFormat).toBe("DD/MM/YYYY HH:mm");
            expect(locale.datePickerDayTitle).toBe("MMMM yyyy");
            expect(locale.datePickerFormat).toBe("dd/MM/yyyy");
        }));
        
        it("zh-TW locale", inject(($rootScope: ng.IRootScopeService) => {
            // Arrange
            let locale = new BPLocale("zh-TW");
            
            // Act

            // Assert
            expect(locale.decimalSeparator).toBe(".");
            expect(locale.thousandSeparator).toBe(",");
            expect(locale.shortDateFormat).toBe("YYYY年MMMD日");
            expect(locale.longDateFormat).toBe("YYYY年MMMD日 Ah點mm分");
            expect(locale.datePickerDayTitle).toBe("yyyy MMMM");
            expect(locale.datePickerFormat).toBe("yyyy年MMMd日");
        }));
    });

    describe("Check values", () => {
        it("to number - valid - US", inject(($rootScope: ng.IRootScopeService) => {
            // Arrange
            let locale = new BPLocale("en-US");
            
            // Act

            // Assert
            expect(locale.toNumber("555")).toBe(555);
            expect(locale.toNumber("555.55")).toBe(555.55);
            expect(locale.toNumber("12345.6789")).toBe(12345.6789);
            expect(locale.toNumber("-22.25")).toBe(-22.25);
            expect(locale.toNumber("123,895")).toBe(123895);
            expect(locale.toNumber("123,555.44")).toBe(123555.44);
            expect(locale.toNumber("1,123,555.38")).toBe(1123555.38);
            expect(locale.toNumber("0.35")).toBe(0.35);
        }));

        it("to number - invalid - US", inject(($rootScope: ng.IRootScopeService) => {
            // Arrange
            let locale = new BPLocale("en-US");
            
            // Act

            // Assert
            expect(locale.toNumber("")).toBeNull();
            expect(locale.toNumber("a555")).toBeNull();
            expect(locale.toNumber("555a")).toBeNull();
            expect(locale.toNumber("55ab5")).toBeNull();
            expect(locale.toNumber("12..34")).toBeNull();
            expect(locale.toNumber(".34")).toBeNull();
            expect(locale.toNumber("12,22")).toBeNull();
            expect(locale.toNumber("12,22.22")).toBeNull();
            expect(locale.toNumber("12,234.")).toBeNull();
            expect(locale.toNumber("12,234,22")).toBeNull();
            expect(locale.toNumber("-1,2234")).toBeNull();
            expect(locale.toNumber("-1,2234.00")).toBeNull();
            expect(locale.toNumber("-1234.222,222")).toBeNull();
        }));

        /*NOT PHANTOMJS*/
        if (!/PhantomJS/.test(window.navigator.userAgent)) {
            it("to number - valid - IT", inject(($rootScope: ng.IRootScopeService) => {
                // Arrange
                let locale = new BPLocale("it-IT");

                // Act

                // Assert
                expect(locale.toNumber("555")).toBe(555);
                expect(locale.toNumber("555,55")).toBe(555.55);
                expect(locale.toNumber("12345,6789")).toBe(12345.6789);
                expect(locale.toNumber("-22,25")).toBe(-22.25);
                expect(locale.toNumber("123.895")).toBe(123895);
                expect(locale.toNumber("123.555,44")).toBe(123555.44);
                expect(locale.toNumber("1.123.555,38")).toBe(1123555.38);
                expect(locale.toNumber("0,35")).toBe(0.35);
            }));

            it("to number - invalid - IT", inject(($rootScope: ng.IRootScopeService) => {
                // Arrange
                let locale = new BPLocale("it-IT");

                // Act

                // Assert
                expect(locale.toNumber("")).toBeNull();
                expect(locale.toNumber("a555")).toBeNull();
                expect(locale.toNumber("555a")).toBeNull();
                expect(locale.toNumber("55ab5")).toBeNull();
                expect(locale.toNumber("12,,34")).toBeNull();
                expect(locale.toNumber(",34")).toBeNull();
                expect(locale.toNumber("12.22")).toBeNull();
                expect(locale.toNumber("12.22,22")).toBeNull();
                expect(locale.toNumber("12.234,")).toBeNull();
                expect(locale.toNumber("12.234.22")).toBeNull();
                expect(locale.toNumber("-1.2234")).toBeNull();
                expect(locale.toNumber("-1.2234,00")).toBeNull();
                expect(locale.toNumber("-1234,222.222")).toBeNull();
            }));
        }
    });

    /*NOT PHANTOMJS*/
    if (!/PhantomJS/.test(window.navigator.userAgent)) {
        describe("Convert values", () => {
            it("format number - valid - US", inject(($rootScope: ng.IRootScopeService) => {
                // Arrange
                let locale = new BPLocale("en-US");

                // Assert
                expect(locale.formatNumber(123)).toBe("123");
                expect(locale.formatNumber(567, 2)).toBe("567");
                expect(locale.formatNumber(12345, 2, true)).toBe("12,345");
                expect(locale.formatNumber(1234567, 0, true)).toBe("1,234,567");
                expect(locale.formatNumber(1234567.123, 2, true)).toBe("1,234,567.12");
                expect(locale.formatNumber(-67, 0, true)).toBe("-67");
                expect(locale.formatNumber(1.15896, 3)).toBe("1.159");
                expect(locale.formatNumber(0.154, 2)).toBe("0.15");
                expect(locale.formatNumber(0.1500, 4)).toBe("0.15");
            }));

            it("format number - invalid - US", inject(($rootScope: ng.IRootScopeService) => {
                // Arrange
                let locale = new BPLocale("en-US");

                // Assert
                expect(locale.formatNumber(null)).toBeNull();
            }));

            it("format number - valid - IT", inject(($rootScope: ng.IRootScopeService) => {
                // Arrange
                let locale = new BPLocale("it-IT");

                // Assert
                expect(locale.formatNumber(123)).toBe("123");
                expect(locale.formatNumber(567, 2)).toBe("567");
                expect(locale.formatNumber(12345, 2, true)).toBe("12.345");
                expect(locale.formatNumber(1234567, 0, true)).toBe("1.234.567");
                expect(locale.formatNumber(1234567.123, 2, true)).toBe("1.234.567,12");
                expect(locale.formatNumber(-67, 0, true)).toBe("-67");
                expect(locale.formatNumber(1.15896, 3)).toBe("1,159");
                expect(locale.formatNumber(0.154, 2)).toBe("0,15");
                expect(locale.formatNumber(0.1500, 4)).toBe("0,15");
            }));

            it("format number - invalid - IT", inject(($rootScope: ng.IRootScopeService) => {
                // Arrange
                let locale = new BPLocale("it-IT");

                // Assert
                expect(locale.formatNumber(null)).toBeNull();
            }));
        });
    }

});
