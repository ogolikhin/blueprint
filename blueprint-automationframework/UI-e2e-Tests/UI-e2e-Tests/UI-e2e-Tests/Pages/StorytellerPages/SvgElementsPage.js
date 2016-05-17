/**
 * This class file will contain all elements and action on element for svg shapes
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * last modified by:
 * Last modified on:
 */
var OR = require('../../Json/OR.json');
var arrayListPresenceOfAll = require("../../Utility/ArrayListPresenceOfAll");
var Svgelementspages = (function () {
    function Svgelementspages() {
        this.labelHeader = element.all(by.css(OR.locators.storyteller.svgPageStoryteller.labelHeader));
        this.labelBody = element.all(by.css(OR.locators.storyteller.svgPageStoryteller.labelBody));
        this.label = element.all(by.css(OR.locators.storyteller.svgPageStoryteller.labelForUserTaskBody));
        this.image = element.all(By.tagName(OR.locators.storyteller.svgPageStoryteller.image));
        this.panelDiscussions = element.all(by.id(OR.locators.storyteller.svgPageStoryteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.svgPageStoryteller.panelDiscussions));
        this.panelProperties = element.all(by.id(OR.locators.storyteller.svgPageStoryteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.svgPageStoryteller.panelProperties));
        this.panelFiles = element.all(by.id(OR.locators.storyteller.svgPageStoryteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.svgPageStoryteller.panelFiles));
        this.panelRelationships = element.all(by.id(OR.locators.storyteller.svgPageStoryteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.svgPageStoryteller.panelRelationships));
        this.panelHistory = element.all(by.id(OR.locators.storyteller.svgPageStoryteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.svgPageStoryteller.panelHistory));
        this.panelPreview = element.all(by.id(OR.locators.storyteller.svgPageStoryteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.svgPageStoryteller.panelPreview));
        this.panelCloseButton = element(By.css(OR.locators.storyteller.svgPageStoryteller.panelCloseButton));
        this.panelDiscussionTextArea = element(By.id(OR.locators.storyteller.svgPageStoryteller.panelDiscussionTextArea));
        this.panelDiscussionTextAreaBody = element(By.id(OR.locators.storyteller.svgPageStoryteller.panelDiscussionTextAreaBody));
        this.panelDiscussionPostButton = element(By.css(OR.locators.storyteller.svgPageStoryteller.panelDiscussionPostButton));
        this.storytellerToggleTextForBusniessProcess = element(By.css(OR.locators.storyteller.svgPageStoryteller.storytellerToggleTextForBusniessProcess));
        this.storytellerToggleTextForUserSystemProcess = element(By.css(OR.locators.storyteller.svgPageStoryteller.storytellerToggleTextForUserSystemProcess));
        this.storytellerTogglecheckBox = element(By.css(OR.locators.storyteller.svgPageStoryteller.storytellerTogglecheckBox));
        this.publishArtifact = element(By.css(OR.locators.storyteller.utilityPanelStoryteller.publishArtifact));
        this.publishArtifactSucessMessage = element(By.id(OR.locators.storyteller.utilityPanelStoryteller.publishArtifactSucessMessage));
        this.postCommentText = element(By.css(OR.locators.storyteller.svgPageStoryteller.postCommentText));
    }
    Object.defineProperty(Svgelementspages.prototype, "getLabelHeader", {
        get: function () { return this.labelHeader; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getLabelBody", {
        get: function () { return this.labelBody; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getlabel", {
        get: function () { return this.label; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getImage", {
        get: function () { return this.image; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPanelDiscussions", {
        get: function () { return this.panelDiscussions; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPanelProperties", {
        get: function () { return this.panelProperties; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPanelFiles", {
        get: function () { return this.panelFiles; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPanelHistory", {
        get: function () { return this.panelHistory; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getpanelPreview", {
        get: function () { return this.panelPreview; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPanelCloseButton", {
        get: function () { return this.panelCloseButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPanelDiscussionTextArea", {
        get: function () { return this.panelDiscussionTextArea; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPanelDiscussionTextAreaBody", {
        get: function () { return this.panelDiscussionTextAreaBody; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getStorytellerToggleTextForBusniessProcess", {
        get: function () { return this.storytellerToggleTextForBusniessProcess; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getStorytellerToggleTextForUserSystemProcess", {
        get: function () { return this.storytellerToggleTextForUserSystemProcess; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getStorytellerTogglecheckBox", {
        get: function () { return this.storytellerTogglecheckBox; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPublishArtifact", {
        get: function () { return this.publishArtifact; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPublishArtifactSucessMessage", {
        get: function () { return this.publishArtifactSucessMessage; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getPostCommentText", {
        get: function () { return this.postCommentText; },
        enumerable: true,
        configurable: true
    });
    // funtion for edit shape's header
    Svgelementspages.prototype.editHeader = function (shape, headerName) {
        this.labelHeader.then(function (elements) {
            console.log("Total is label from edit header " + elements.length);
            if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
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
    // function to verify shape's header name
    /* TO DO this funtion can be deleted but wait until dev finalized their code
  /*  public verifyHeaderName(shape): Promise<string> {
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.labelHeader), 100000);
        return this.labelHeader.then((elements) => {
            elements[shape].all(by.tagName('div')).then((el) => {
                console.log("inside of finding div");
                el[1].getText()
                    .then((gettext) => {
                        console.log(gettext);
                        this.header = gettext;
                        console.log("inside "+this.header);
                        return this.header;
                    });
            });
            return this.header;
        });

    }*/
    Svgelementspages.prototype.verifyHeaderName = function (shape) {
        var _this = this;
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.labelHeader), 100000);
        return this.labelHeader.then(function (elements) {
            elements[shape].getText().then(function (gettext) {
                _this.header = gettext;
            });
            return _this.header;
        });
        //return this.header;
        //  });
    };
    //funtion to edit body  of a shape
    Svgelementspages.prototype.editBody = function (shape, bodyText) {
        this.labelBody.then(function (elements) {
            console.log("Total is label " + elements.length);
            if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
                browser.actions().doubleClick(elements[shape]).perform();
                browser.actions().sendKeys(protractor.Key.DELETE).perform();
                browser.actions().sendKeys(bodyText).perform();
                browser.actions().sendKeys('\n').perform();
                console.log("End of edtign");
            }
            else {
                console.log("Your element is not in array-this is custom error message");
            }
        });
    };
    //funtion to verify text body  of a shape
    /* TO DO this funtion can be deleted but wait until dev finalized their code
        public verifyBodyText1(shape): Promise<string> {
            browser.wait(arrayListPresenceOfAll.presenceOfAll(this.labelBody), 100000);
            return this.labelBody.then((elements) => {
                elements[shape].all(by.tagName('div')).then((el) => {
                    console.log("inside of finding div");
                    el[1].getText()
                        .then((gettext) => {
                            console.log(gettext);
                            this.body = gettext;
                            console.log("inside " + this.body);
                            return this.body;
                        });
                });
                return this.body;
            });
    
        }*/
    //===
    Svgelementspages.prototype.verifyBodyText = function (shape) {
        var _this = this;
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.labelBody), 100000);
        return this.labelBody.then(function (elements) {
            elements[shape].getText().then(function (gettext) {
                _this.body = gettext;
            });
            return _this.body;
        });
        //return this.header;
        //  });
    };
    // function to edit body text for user task
    // TO DO this funtion can be deleted but wait until dev finalized their code 
    Svgelementspages.prototype.editBodyForUserTask = function (shape, bodyText) {
        this.label.then(function (elements) {
            console.log("Total is label for label " + elements.length);
            if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
                browser.actions().sendKeys(protractor.Key.DELETE).perform();
                browser.actions().sendKeys(bodyText).perform();
                // browser.driver.sleep(5000);
                browser.actions().sendKeys('\n').perform();
                // browser.driver.sleep(5000);
                console.log("End of edtign");
            }
            else {
                console.log("Your element is not in array-this is custom error message");
            }
        });
    };
    // function to verify  body text for user task
    //TO DO this funtion can be deleted but wait until dev finalized their code 
    Svgelementspages.prototype.verifyUserTaskBodyText = function (shape) {
        var _this = this;
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.label), 100000);
        return this.label.then(function (elements) {
            elements[shape].all(by.tagName('div')).then(function (el) {
                console.log("inside of finding div");
                el[1].getText()
                    .then(function (gettext) {
                    console.log(gettext);
                    _this.labelForUserTaskBody = gettext;
                    console.log("inside " + _this.labelForUserTaskBody);
                    return _this.labelForUserTaskBody;
                });
            });
            return _this.labelForUserTaskBody;
        });
    };
    //funtion to find an element and select the shape
    Svgelementspages.prototype.findElementAndSelect = function (shape) {
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.label), 100000);
        return this.label.then(function (elements) {
            console.log("inside of finding element");
            console.log("Total is label from find and select " + elements.length);
            return elements[shape];
        });
    };
    //funtion to find footer and information icon of a shape
    Svgelementspages.prototype.findFooterAndInfoIcon = function (icon) {
        var x = element.all(By.tagName(OR.locators.storyteller.image));
        this.image.then(function (el) {
            console.log("Finding Icon " + el.length);
            el[icon].click();
        });
    };
    // function to post a comment at discussion panel
    Svgelementspages.prototype.postComment = function (comment) {
        var _this = this;
        browser.driver.sleep(2000);
        this.panelDiscussions.click();
        this.panelDiscussionTextArea.click();
        browser.driver.switchTo().frame(OR.locators.storyteller.svgPageStoryteller.panelDiscussionTextAreaIframeId);
        browser.ignoreSynchronization = true;
        this.panelDiscussionTextAreaBody.isPresent()
            .then(function (tt) {
            console.log(tt);
            if (tt === true) {
                _this.panelDiscussionTextAreaBody.sendKeys(comment);
                browser.driver.switchTo().defaultContent();
            }
            if (tt === false) {
                fail("Eelement not found");
                browser.driver.switchTo().defaultContent();
            }
        });
    };
    return Svgelementspages;
})();
module.exports = Svgelementspages;
//# sourceMappingURL=SvgElementsPage.js.map