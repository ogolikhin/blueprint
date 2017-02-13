import * as angular from "angular";
import "lodash";
import {Helper} from "./helper";

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

    describe("escapeQuote", () => {
        it("escapes the text content of quotes", () => {
            // Arrange
            let text = `test"test`;
            let text2 = `test"test"`;

            // Act
            text = Helper.escapeQuot(text);
            text2 = Helper.escapeQuot(text2);

            // Assert
            expect(text).toEqual(`test&quot;test`);
            expect(text2).toEqual(`test&quot;test&quot;`);
        });
    });

    describe("stripWingdings", () => {
        it("removes CSS style definition for Wingdings font", () => {
            // Act
            const text = Helper.stripWingdings(html);

            // Assert
            expect(text).not.toContain("Wingdings");
        });
    });

    describe("tagsContainText", () => {
        it("returns true when there is text in a HTML structure", () => {
            // Act
            const bool = Helper.tagsContainText(html);

            // Assert
            expect(bool).toBeTruthy();
        });

        it("returns false when there is no text in a HTML structure", () => {
            // Act
            const bool = Helper.tagsContainText(emptyHtml);

            // Assert
            expect(bool).toBeFalsy();
        });
    });

    describe("stripTinyMceBogusChars", () => {
        it("strips bogus tags added by tinyMce", () => {
            // Arrange
            const div = document.createElement("div");
            div.innerHTML = html;

            const bogus = document.createElement("div");
            bogus.innerHTML = `<br data-mce-bogus="1">`;

            div.appendChild(bogus.firstElementChild);

            // Act
            const out = Helper.stripTinyMceBogusChars(div.innerHTML);

            // Assert
            expect(out).toBe(html);
        });
    });

    describe("getHtmlBodyContent", () => {
        it("returns the content of the HTML/BODY tag", () => {
            // Arrange
            const body = "<html><body>" + html + "</body></html>";

            // Act
            const out = Helper.getHtmlBodyContent(body);

            // Assert
            expect(out).toBe(html);
        });
    });
});

describe("addTableBorders", () => {
    it("should add default borders to cells with no borders", () => {
        // Arrange/Act
        let node = document.createElement("div");
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
        const node = document.createElement("div");
        node.innerHTML = "<table><tr><td>Table</td></tr></table>" +
            "<p style='font-family: Arial, sans-serif'>Arial Default</p>" +
            "<p style='font-family: Arial, sans-serif'><em>Arial Em</em></p>" +
            "<p style='font-family: Arial, sans-serif'><strong>Arial Strong</strong></p>" +
            "<p>Default</p>" +
            "<p><em>Em</em></p>" +
            "<p><strong>Strong</strong></p>" +
            "<p style='font-family: Arial, sans-serif'><strong><span style='font-size: 18px'>Bold 18px</span></strong></p>" +
            "<p style='font-family: Arial, sans-serif'><em><span style='font-family: Verdana, sans-serif'>Verdana Em</span></em></p>" +
            "<p style='font-family: Arial, sans-serif'><em><span style='font-family: Invalid Font'>Verdana Em</span></em></p>";

        Helper.setFontFamilyOrOpenSans(node, ["Arial", "Verdana"]);

        const td = node.querySelector("td");
        const p = node.querySelectorAll("p");

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
        expect((<HTMLElement> p[8].firstElementChild.firstElementChild).style.fontFamily).toContain("Open Sans");
    });

    it("should add Open Sans for text with images without changing the order of the content", () => {
        // Arrange/Act
        const text1 = "There is an image after me ";
        const text2 = " and another after me ";
        const imgSrc = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"; // 1x1 transparent GIF
        const node = document.createElement("div");
        node.innerHTML = `<p>${text1}<img src="${imgSrc}" />${text2}<img src="${imgSrc}" /></p>`;

        Helper.setFontFamilyOrOpenSans(node);

        const p = node.querySelector("p") as HTMLElement;

        // Assert
        expect(p.childElementCount).toBe(4);
        expect(p.textContent).toBe(text1 + text2);
        expect(p.children[0].tagName.toUpperCase()).toBe("SPAN");
        expect(p.children[0].textContent).toBe(text1);
        expect((<HTMLElement>p.children[0]).style.fontFamily).toContain("Open Sans");
        expect(p.children[2].tagName.toUpperCase()).toBe("SPAN");
        expect(p.children[2].textContent).toBe(text2);
        expect((<HTMLElement>p.children[2]).style.fontFamily).toContain("Open Sans");
        expect(p.children[1].tagName.toUpperCase()).toBe("IMG");
        expect(p.children[3].tagName.toUpperCase()).toBe("IMG");
    });
});

describe("autoLinkURLText", () => {
    it("should find and replace text URLs in nested HTML elements", () => {
        // Arrange/Act
        let node = document.createElement("div");
        /* tslint:disable:max-line-length */
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
        /* tslint:enable:max-line-length */
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

describe("Remove Attribute From Node", () => {
    it("should remove id from Node and children", () => {
        // Arrange
        let node = document.createElement("div");
        node.innerHTML = `
            <div id='mainDiv'>
                <span>This is an inline trace:&nbsp;</span>
                <div id='childDiv'>
                </div>
            </div>`;

        // Act
        const innerDiv = node.children[0].children[1];
        const mainDivId = node.children[0].id;
        const innerDivId = innerDiv.id;
        Helper.removeAttributeFromNode(node, "id");

        // Assert
        expect(mainDivId).toBe("mainDiv");
        expect(node.children[0].hasAttribute("id")).toBe(false);
        expect(innerDivId).toBe("childDiv");
        expect(innerDiv.hasAttribute("id")).toBe(false);
    });
});

describe("Remove tags from an HTML element", () => {
    const parser = new DOMParser();
    const html = document.createElement("div");
    html.innerHTML = `<p>Test</p>
                      <span>Another</span>
                      <img src="test.png" />
                      <img src="test2.png" />`;

    it("should remove all 'img' tags", () => {
        // Act
        const res = Helper.stripHtmlTags(html.outerHTML, ["img"]);

        // Assert
        const result = angular.element(res);
        expect(result.find("img").length).toBe(0);
    });

    it("should remove all 'span' tags", () => {
        // Act
        const res = Helper.stripHtmlTags(html.outerHTML, ["span"]);

        // Assert
        const result = angular.element(res);
        expect(result.find("span").length).toBe(0);
    });
});

describe("replaceImgSrc", () => {
    it("should change all 'src' of all 'img' tags (and just 'img' tags)", () => {
        // Assert
        const html = `<html>
<head>
    <script type="text/javascript" src="//dummy1.js"></script>
</head>
<body>
    <img src="//dummy1.jpg">
    <img class="dummy-class" src="//dummy2.jpg" /><img style="width: 100%" src="//dummy2.jpg" onerror="alert('Image not found!');">
</body>
<script type="text/javascript" src="//dummy2.js"></script>
</html>`;
        const expected = `<html>
<head>
    <script type="text/javascript" src="//dummy1.js"></script>
</head>
<body>
    <img data-temp-src="//dummy1.jpg">
    <img class="dummy-class" data-temp-src="//dummy2.jpg" /><img style="width: 100%" data-temp-src="//dummy2.jpg" onerror="alert('Image not found!');">
</body>
<script type="text/javascript" src="//dummy2.js"></script>
</html>`;

        // Act
        const result = Helper.replaceImgSrc(html, true);

        // Assert
        expect(result).toBe(expected);
    });

    it("should change all 'data-temp-src' of all 'img' tags (and just 'img' tags)", () => {
        // Assert
        const expected = `<html>
<head>
    <script type="text/javascript" src="//dummy1.js"></script>
</head>
<body>
    <img src="//dummy1.jpg">
    <img class="dummy-class" src="//dummy2.jpg" /><img style="width: 100%" src="//dummy2.jpg" onerror="alert('Image not found!');">
</body>
<script type="text/javascript" src="//dummy2.js"></script>
</html>`;
        const html = `<html>
<head>
    <script type="text/javascript" src="//dummy1.js"></script>
</head>
<body>
    <img data-temp-src="//dummy1.jpg">
    <img class="dummy-class" data-temp-src="//dummy2.jpg" /><img style="width: 100%" data-temp-src="//dummy2.jpg" onerror="alert('Image not found!');">
</body>
<script type="text/javascript" src="//dummy2.js"></script>
</html>`;

        // Act
        const result = Helper.replaceImgSrc(html, false);

        // Assert
        expect(result).toBe(expected);
    });

    it("doesn't change anything if input is not string", () => {
        // Assert
        let html;

        // Act
        const result = Helper.replaceImgSrc(html, true);

        // Assert
        expect(result).toBe(html);
    });
});

