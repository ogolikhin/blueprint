/**
 * This class file will contain all elements and action on element for svg shapes
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 *
 */
var OR = require('../../Json/OR.json');
var ttt;
var Svgelementspages = (function () {
    function Svgelementspages() {
    }
    //funtion to edit header of a shape
    Svgelementspages.editHeader = function (shape, headerName) {
        Svgelementspages.labelHeader.then(function (elements) {
            console.log("Total is label " + elements.length);
            if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
                browser.actions().sendKeys(protractor.Key.DELETE).perform();
                browser.actions().sendKeys(headerName).perform();
                browser.actions().sendKeys('\n').perform();
                console.log("End of editing");
            }
            else {
                console.log("Your element is not in array-this is custom error message");
            }
        });
    };
    //funtion to verify header name of a shape
    Svgelementspages.verifyHeaderName = function (shape, headername) {
        Svgelementspages.labelHeader.then(function (elements) {
            browser.driver.sleep(5000);
            console.log("inside of finding element");
            console.log("Total is label " + elements.length);
            elements[shape].all(by.tagName('div')).then(function (el) {
                console.log("inside of finding div");
                el[1].getText().then(function (gettext) {
                    console.log("inside of finding getText");
                    console.log("Header text is -----" + gettext);
                    expect(gettext).toBe(headername);
                    console.log("OUT OF FINDING TEXT");
                });
            });
        });
    };
    //funtion to edit body  of a shape
    Svgelementspages.editBody = function (shape, bodyText) {
        Svgelementspages.labelBody.then(function (elements) {
            console.log("Total is label " + elements.length);
            if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
                browser.actions().sendKeys(protractor.Key.DELETE).perform();
                browser.actions().sendKeys(bodyText).perform();
                browser.driver.sleep(5000);
                browser.actions().sendKeys('\n').perform();
                browser.driver.sleep(5000);
                console.log("End of edtign");
            }
            else {
                console.log("Your element is not in array-this is custom error message");
            }
        });
    };
    //funtion to  verify body text  of a shape
    Svgelementspages.verifyBodyText = function (shape, bodyText) {
        Svgelementspages.labelBody.then(function (elements1) {
            browser.driver.sleep(5000);
            console.log("inside of finding element");
            console.log("Total is label " + elements1.length);
            elements1[shape].all(by.tagName('div')).then(function (el) {
                console.log("inside of finding div");
                el[1].getText().then(function (gettext) {
                    console.log("inside of finding getText");
                    console.log("Body text is -----" + gettext);
                    expect(gettext).toBe(bodyText);
                    console.log("OUT OF FINDING TEXT");
                });
            });
        });
    };
    //funtion to find an element and select the shape
    Svgelementspages.findElementAndSelect = function (shape) {
        Svgelementspages.labelHeader.then(function (elements) {
            browser.driver.sleep(5000);
            console.log("inside of finding element");
            console.log("Total is label " + elements.length);
            elements[shape].click();
        });
    };
    //funtion to find footer and information icon of a shape
    Svgelementspages.findFooterAndInfoIcon = function () {
        var x = element.all(By.tagName(OR.locators.storyteller.image));
        Svgelementspages.image.then(function (el) {
            console.log("Finding Icon " + el.length);
            el[19].click();
            browser.driver.sleep(5000);
        });
    };
    //funtion to verify Panel Discussion
    Svgelementspages.verifyPanelDiscussion = function () {
        Svgelementspages.panelDiscussions.click();
        expect(Svgelementspages.panelDiscussions.getText()).toEqual(['Discussions']);
    };
    //funtion to verify Panel Properties
    Svgelementspages.verifyPanelProperties = function () {
        Svgelementspages.panelProperties.click();
        expect(Svgelementspages.panelProperties.getText()).toEqual(['Properties']);
    };
    //funtion to verify Panel File
    Svgelementspages.verifyPanelFiles = function () {
        Svgelementspages.panelFiles.click();
        expect(Svgelementspages.panelFiles.getText()).toEqual(['Files']);
    };
    //funtion to verify Panel Relationships
    Svgelementspages.verifyPanelRelationships = function () {
        Svgelementspages.panelrelationships.click();
        expect(Svgelementspages.panelrelationships.getText()).toEqual(['Relationships']);
    };
    //funtion to verify Panel History
    Svgelementspages.verifyPanelHistory = function () {
        Svgelementspages.panelHistory.click();
        expect(Svgelementspages.panelHistory.getText()).toEqual(['History']);
    };
    //funtion to verify Panel Preview
    Svgelementspages.verifyPanelPreview = function () {
        Svgelementspages.panelPreview.click();
        expect(Svgelementspages.panelPreview.getText()).toEqual(['Preview']);
    };
    //funtion to verify Panel CloseButton
    Svgelementspages.verifyPanelCloseButton = function () {
        Svgelementspages.panelCloseButton.click();
    };
    //funtion to verify Panel Post Comment
    Svgelementspages.verifyPostComment = function () {
        Svgelementspages.panelDiscussionTextArea.click();
        browser.driver.switchTo().frame(OR.locators.storyteller.panelDiscussionTextAreaIframeId);
        browser.ignoreSynchronization = true;
        browser.waitForAngular();
        browser.sleep(500);
        browser.driver.sleep(5000);
        console.log("After iframe Post method");
        Svgelementspages.panelDiscussionTextAreaBody.sendKeys("This process need to be edited");
        browser.driver.sleep(5000);
        browser.driver.switchTo().defaultContent();
        browser.driver.sleep(5000);
        Svgelementspages.panelDiscussionPostButton.click();
        browser.driver.sleep(5000);
    };
    Svgelementspages.labelHeader = element.all(by.css(OR.locators.storyteller.labelHeader));
    Svgelementspages.labelBody = element.all(by.css(OR.locators.storyteller.labelBody));
    Svgelementspages.image = element.all(By.tagName(OR.locators.storyteller.image));
    Svgelementspages.panelDiscussions = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelDiscussions));
    Svgelementspages.panelProperties = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelProperties));
    Svgelementspages.panelFiles = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelFiles));
    Svgelementspages.panelrelationships = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelrelationships));
    Svgelementspages.panelHistory = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelHistory));
    Svgelementspages.panelPreview = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelPreview));
    Svgelementspages.panelCloseButton = element(By.css(OR.locators.storyteller.panelCloseButton));
    Svgelementspages.panelDiscussionTextArea = element(By.id(OR.locators.storyteller.panelDiscussionTextArea));
    Svgelementspages.panelDiscussionTextAreaBody = browser.element(By.id(OR.locators.storyteller.panelDiscussionTextAreaBody));
    Svgelementspages.panelDiscussionPostButton = browser.element(By.css(OR.locators.storyteller.panelDiscussionPostButton));
    return Svgelementspages;
})();
module.exports = Svgelementspages;
//# sourceMappingURL=SvgElementsPage.js.map