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
    let html = `<div><h3 class="heading" style="font-family: Wingdings">Labels</h3>
<a href="/folder1/accepted" class="label" title="Accepted">Accepted</a>
<a href="/folder2/declined" class="label" title="Declined">Declined</a>
<a href="#" onclick="javascript:alert('Popup!')" style="font-family:'Wingdings';" class="popup" title="Popup">Popup</a></div>`;
    let emptyHtml = `<div><h3 class="heading" style="font-family: Wingdings"></h3>
<a href="/folder1/accepted" class="label" title="Accepted"></a>
<a href="/folder2/declined" class="label" title="Declined"></a>
<a href="#" onclick="javascript:alert('Popup!')" style="font-family:'Wingdings';" class="popup" title="Popup"></a></div>`;

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
            expect(text).toEqual(`&lt;div&gt;&lt;h3 class="heading" style="font-family: Wingdings"&gt;Labels&lt;/h3&gt;
&lt;a href="/folder1/accepted" class="label" title="Accepted"&gt;Accepted&lt;/a&gt;
&lt;a href="/folder2/declined" class="label" title="Declined"&gt;Declined&lt;/a&gt;
&lt;a href="#" onclick="javascript:alert('Popup!')" style="font-family:'Wingdings';" class="popup" title="Popup"&gt;Popup&lt;/a&gt;&lt;/div&gt;`);
        });
    });

    describe("stripWingdings", () => {
        it("removes CSS style definition for Wingdings font", () => {
            // Arrange
            let text;

            // Act
            text = Helper.stripWingdings(html);

            // Assert
            expect(text).not.toContain("Wingdings");
        });
    });

    describe("tagsContainText", () => {
        it("returns true when there is text in a HTML structure", () => {
            // Arrange
            let bool;

            // Act
            bool = Helper.tagsContainText(html);

            // Assert
            expect(bool).toBeTruthy();
        });

        it("returns false when there is no text in a HTML structure", () => {
            // Arrange
            let bool;

            // Act
            bool = Helper.tagsContainText(emptyHtml);

            // Assert
            expect(bool).toBeFalsy();
        });
    });
});

describe("addTableBorders", () => {
    it("should add default borders to cells with no borders", () => {
        // Arrange/Act
        let node = document.createElement("div");
        /* tslint:disable */
        node.innerHTML = `<table>
<tr>
    <td>A</td>
    <td style="border-color: transparent; border-width: 0 10px 10pt">B</td>
</tr>
<tr>
    <td style="border-color: red; border-width: 2px">C</td>
    <td style="border-color: green;">C</td>
</tr>
</table>`;
        /* tslint:enable */

        Helper.addTableBorders(node);

        let td = node.querySelectorAll("td");
        // Assert
        expect(td[0].style.borderWidth).toBe("1px");
        expect(td[1].style.borderWidth).toBe("1px");
        expect(td[2].style.borderWidth).toBe("2px");
        expect(td[3].style.borderWidth).toBe("1px");
        expect(td[0].style.borderColor).toBe("black");
        expect(td[1].style.borderColor).toBe("black");
        expect(td[2].style.borderColor).toBe("red");
        expect(td[3].style.borderColor).toBe("green");
    });
});

describe("setFontFamilyOrOpenSans", () => {
    it("should add Open Sans if the tags don't have a font definition", () => {
        // Arrange/Act
        let node = document.createElement("div");
        /* tslint:disable */
        node.innerHTML = "<table><tr><td>Table</td></tr></table>" +
            "<p style='font-family: Arial, sans-serif'>Arial Default</p>" +
            "<p style='font-family: Arial, sans-serif'><em>Arial Em</em></p>" +
            "<p style='font-family: Arial, sans-serif'><strong>Arial Strong</strong></p>" +
            "<p>Default</p>" +
            "<p><em>Em</em></p>" +
            "<p><strong>Strong</strong></p>" +
            "<p style='font-family: Arial, sans-serif'><strong><span style='font-size: 18px'>Bold 18px</span></strong></p>" +
            "<p style='font-family: Arial, sans-serif'><em><span style='font-family: Verdana'>Verdana Em</span></em></p>";
        /* tslint:enable */

        Helper.setFontFamilyOrOpenSans(node);

        let td = node.querySelector("td");
        let p = node.querySelectorAll("p");
        // Assert
        expect((<HTMLElement> td.firstElementChild).style.fontFamily).toContain("Open Sans");
        expect((<HTMLElement> p[0].firstElementChild).style.fontFamily).toContain("Arial");
        expect((<HTMLElement> p[1].firstElementChild.firstElementChild).style.fontFamily).toContain("Arial");
        expect((<HTMLElement> p[2].firstElementChild.firstElementChild).style.fontFamily).toContain("Arial");
        expect((<HTMLElement> p[3].firstElementChild).style.fontFamily).toContain("Open Sans");
        expect((<HTMLElement> p[4].firstElementChild.firstElementChild).style.fontFamily).toContain("Open Sans");
        expect((<HTMLElement> p[5].firstElementChild.firstElementChild).style.fontFamily).toContain("Open Sans");
        expect((<HTMLElement> p[6].firstElementChild.firstElementChild).style.fontFamily).toContain("Arial");
        expect((<HTMLElement> p[7].firstElementChild.firstElementChild).style.fontFamily).toContain("Verdana");
    });
});

describe("autoLinkURLText", () => {
    it("should find and replace text URLs in nested HTML elements", () => {
        // Arrange/Act
        let node = document.createElement("div");
        /* tslint:disable */
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
        /* tslint:enable */
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
        expect(node.querySelectorAll("a")[1].getAttribute("href")).toContain("http://");
    });
});

