import {Helper} from "./helper";

describe("toDashCase", () => {
    it("changes 'bpTooltip' into 'bp-tooltip'", () => {
        // Arrange
        let string = "bpTooltip";

        // Act
        string = Helper.toDashCase(string);

        // Assert
        expect(string).toEqual("bp-tooltip");
    });

    it("changes 'UIMockup' into 'ui-mockup'", () => {
        // Arrange
        let string = "UIMockup";

        // Act
        string = Helper.toDashCase(string);

        // Assert
        expect(string).toEqual("ui-mockup");
    });

    it("changes 'ArtifactReviewPackage' into 'artifact-review-package'", () => {
        // Arrange
        let string = "ArtifactReviewPackage";

        // Act
        string = Helper.toDashCase(string);

        // Assert
        expect(string).toEqual("artifact-review-package");
    });
});

describe("toCamelCase", () => {
    it("changes 'bp-tooltip' into 'bpTooltip'", () => {
        // Arrange
        let string = "bp-tooltip";

        // Act
        string = Helper.toCamelCase(string);

        // Assert
        expect(string).toEqual("bpTooltip");
    });

    it("changes 'ui-mockup' into 'uiMockup'", () => {
        // Arrange
        let string = "ui-mockup";

        // Act
        string = Helper.toCamelCase(string);

        // Assert
        expect(string).toEqual("uiMockup");
    });

    it("changes 'artifact-review-package' into 'artifactReviewPackage'", () => {
        // Arrange
        let string = "artifact-review-package";

        // Act
        string = Helper.toCamelCase(string);

        // Assert
        expect(string).toEqual("artifactReviewPackage");
    });
});

describe("findAncestorByCssClass", () => {
    let html = `<div class="root">
        <div class="grandparent">
            <div class="parent">
                <span id="child"></span>
            </div>
            <div class="uncle">
                <span id="cousin"></span>
            </div>    
        </div>
    </div>`;

    it("finds the immediate parent", () => {
        // Arrange
        document.body.innerHTML = html;
        let child = document.querySelector("#child");

        // Act
        let elem = Helper.findAncestorByCssClass(child, "parent");

        // Assert
        expect(elem).toBeDefined();
        expect(elem.className).toEqual("parent");
    });

    it("finds the grand-parent", () => {
        // Arrange
        document.body.innerHTML = html;
        let child = document.querySelector("#child");

        // Act
        let elem = Helper.findAncestorByCssClass(child, "grandparent");

        // Assert
        expect(elem).toBeDefined();
        expect(elem.className).toEqual("grandparent");
    });

    it("doesn't return any element if no ancestor has the specified class", () => {
        // Arrange
        document.body.innerHTML = html;
        let child = document.querySelector("#child");

        // Act
        let elem = Helper.findAncestorByCssClass(child, "uncle");

        // Assert
        expect(elem).toBeNull();
    });
});

describe("to and from HTML", () => {
    let html = `<div><h3 class="heading">Labels</h3>
<a href="/folder1/accepted" class="label" title="Accepted">Accepted</a>
<a href="/folder2/declined" class="label" title="Declined">Declined</a>
<a href="#" onclick="javascript:alert('Popup!')" class="popup" title="Popup">Popup</a></div>`;

    describe("stripHTMLTags", () => {
        it("retrieves the text content of an HTML structure", () => {
            // Arrange
            let text;

            // Act
            text = Helper.stripHTMLTags(html);

            // Assert
            expect(text).toEqual(`Labels
Accepted
Declined
Popup`);
        });
    });

    describe("escapeHTMLText", () => {
        it("escapes the text content of an HTML structure", () => {
            // Arrange
            let text;

            // Act
            text = Helper.escapeHTMLText(html);

            // Assert
            expect(text).toEqual(`&lt;div&gt;&lt;h3 class="heading"&gt;Labels&lt;/h3&gt;
&lt;a href="/folder1/accepted" class="label" title="Accepted"&gt;Accepted&lt;/a&gt;
&lt;a href="/folder2/declined" class="label" title="Declined"&gt;Declined&lt;/a&gt;
&lt;a href="#" onclick="javascript:alert('Popup!')" class="popup" title="Popup"&gt;Popup&lt;/a&gt;&lt;/div&gt;`);
        });
    });
});

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

describe("uiDatePickerFormatAdaptor", () => {
    it("should correctly parse en-US format", () => {
        // Arrange/Act
        let format = Helper.uiDatePickerFormatAdaptor("MM/DD/YYYY");

        // Assert
        expect(format).toEqual("MM/dd/yyyy");
    });

    it("should correctly parse zh-TW format", () => {
        // Arrange/Act
        let format = Helper.uiDatePickerFormatAdaptor("YYYY年MMMD日");

        // Assert
        expect(format).toEqual("yyyy年MMMd日");
    });

    it("should correctly parse ar format", () => {
        // Arrange/Act
        let format = Helper.uiDatePickerFormatAdaptor("D/\u200FM/\u200FYYYY");

        // Assert
        expect(format).toEqual("d/M/yyyy");
    });
});

describe("toStartOfTZDay", () => {
    it("should remove the time part from a date", () => {
        let date = new Date("2016-08-01T00:30:00.0000000");

        let parsed = Helper.toStartOfTZDay(date);

        expect(parsed).not.toBe(date);
        expect(parsed.getDate()).toBe(1);
    });
});

describe("autoLinkURLText", () => {
    it("should find and replace text URLs in nested HTML elements", () => {
        // Arrange/Act
        let node = document.createElement("div");
        node.innerHTML = `
<p>
    <span>This is an inline trace:&nbsp;</span>
    <a linkassemblyqualifiedname="BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, BluePrintSys.RC.Client.SL.RichText, Version=7.3.0.0, Culture=neutral, PublicKeyToken=null" canclick="True" isvalid="True" href="http://localhost:9801/?ArtifactId=392" target="_blank" artifactid="392">
        <span style="text-decoration: underline; color: #0000FF;">RQ392: TEST ARTIFACT</span>
    </a>
</p>
<p>
    <span>This is a normal hyperlink:&nbsp;</span>
    <a href="http://www.google.com">
        <span>Google</span>
    </a>
</p>
<p>
    <span>Let's see if https://www.cnn.com, http://127.<span>0.0.1</span>, http://www.google.com, or even ftp://filehippo.com get recognized</span>
</p>`;
        Helper.autoLinkURLText(node);

        // Assert
        expect(node.querySelectorAll("a").length).toBe(5);
    });

    it("should find and replace text URLs even if there is no protocol in the url", () => {
        // Arrange/Act
        let node = document.createElement("div");
        node.innerHTML = `
<p>
    <span>Let's see if https://www.cnn.com, www.google.com, or even ftp://filehippo.com get recognized</span>
</p>`;
        Helper.autoLinkURLText(node);

        // Assert
        expect(node.querySelectorAll("a").length).toBe(3);
        expect(node.querySelectorAll("a")[1].getAttribute("href")).toContain("http://")
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

describe("toLocaleNumber", () => {
    it("converts to proper number based on locale (US)", () => {
        // Arrange/Act
        let number = Helper.toLocaleNumber(123456.789, "en-US");

        // Assert
        expect(number).toEqual("123456.789");
    });

    it("converts to proper negative number based on locale (US)", () => {
        // Arrange/Act
        let number = Helper.toLocaleNumber(-123456.789, "en-US");

        // Assert
        expect(number).toEqual("-123456.789");
    });

    // see https://github.com/ariya/phantomjs/issues/12581#issuecomment-166645579
    if (!/PhantomJS/.test(window.navigator.userAgent)) {
        it("converts to proper number based on locale (IT)", () => {
            // Arrange/Act
            let number = Helper.toLocaleNumber(123456.789, "it-IT");

            // Assert
            expect(number).toEqual("123456,789");
        });

        it("converts to proper negative number based on locale (IT)", () => {
            // Arrange/Act
            let number = Helper.toLocaleNumber(-123456.789, "it-IT");

            // Assert
            expect(number).toEqual("-123456,789");
        });
    }

    it("doesn't convert bad formatted NaN", () => {
        // Arrange/Act
        let number = Helper.toLocaleNumber(NaN);

        // Assert
        expect(number).toBeNull();
    });

    it("doesn't convert null", () => {
        // Arrange/Act
        let number = Helper.toLocaleNumber(null);

        // Assert
        expect(number).toBeNull();
    });

    it("doesn't convert undefined", () => {
        // Arrange/Act
        let number = Helper.toLocaleNumber(undefined);

        // Assert
        expect(number).toBeNull();
    });
});

describe("parseLocaleNumber", () => {
    it("converts to proper number based on locale (US)", () => {
        // Arrange/Act
        let number = Helper.parseLocaleNumber("123,456.789", "en-US");

        // Assert
        expect(number).toEqual(123456.789);
    });

    it("converts to proper negative number based on locale (US)", () => {
        // Arrange/Act
        let number = Helper.parseLocaleNumber("-123,456.789", "en-US");

        // Assert
        expect(number).toEqual(-123456.789);
    });

    // see https://github.com/ariya/phantomjs/issues/12581#issuecomment-166645579
    if (!/PhantomJS/.test(window.navigator.userAgent)) {
        it("converts to proper number based on locale (IT)", () => {
            // Arrange/Act
            let number = Helper.parseLocaleNumber("123.456,789", "it-IT");

            // Assert
            expect(number).toEqual(123456.789);
        });

        it("converts to proper negative number based on locale (IT)", () => {
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