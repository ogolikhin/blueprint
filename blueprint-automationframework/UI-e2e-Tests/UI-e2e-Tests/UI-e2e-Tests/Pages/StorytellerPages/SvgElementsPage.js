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
var logger = require('winston');
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
        this.generateUserStoriesMenuButton = element(By.css(OR.locators.storyteller.utilityPanelStoryteller.generateUserStoriesMenuButton));
        this.generateUserStoriesMenuIteams = element.all(By.css(OR.locators.storyteller.utilityPanelStoryteller.generateUserStoriesMenuIteams));
        this.footerModelTitle = element(By.css(OR.locators.storyteller.svgPageStoryteller.footerModelTitle));
        this.footerModelCancelButton = element(By.css(OR.locators.storyteller.svgPageStoryteller.footerModelCancelButton));
        this.footerEditDetailButton = element.all(By.css(OR.locators.storyteller.svgPageStoryteller.footerEditDetailButton));
        this.footerAddCommentButton = element.all(By.css(OR.locators.storyteller.svgPageStoryteller.footerAddCommentButton));
        this.footerReviewTracesButton = element.all(By.css(OR.locators.storyteller.svgPageStoryteller.footerReviewTracesButton));
        this.footerAddImageMockUpsScreenshotsButton = element.all(By.css(OR.locators.storyteller.svgPageStoryteller.footerAddImageMockUpsScreenshotsButton));
        this.footerViewUserStoriesButton = element.all(By.css(OR.locators.storyteller.svgPageStoryteller.footerViewUserStoriesButton));
        this.footerAddIncludesButton = element.all(By.css(OR.locators.storyteller.svgPageStoryteller.footerAddIncludesButton));
        this.addTaskButton = element.all(By.css(OR.locators.storyteller.svgPageStoryteller.addTaskButton));
        this.addTaskItems = element.all(By.css(OR.locators.storyteller.svgPageStoryteller.addTaskItems));
        this.activeTabInModal = element(By.css(OR.locators.storyteller.svgPageStoryteller.activeTabInModal));
        this.viewUserStoriesGherkinTitle = element.all(By.css(OR.locators.storyteller.svgPageStoryteller.viewUserStoriesGherkinTitle));
        this.showMoreButtonAtModel = element(By.css(OR.locators.storyteller.svgPageStoryteller.showMoreButtonAtModel));
        this.includeButtonAtModel = element(By.css(OR.locators.storyteller.svgPageStoryteller.includeButtonAtModel)).all(by.tagName("li"));
        this.includeArtifactTextBox = element(By.id(OR.locators.storyteller.svgPageStoryteller.includeArtifactTextBox));
        this.includeArtifactDropdownList = element(By.css(OR.locators.storyteller.svgPageStoryteller.includeArtifactDropdownList)).all(by.tagName("li"));
        this.modelOKButton = element(By.css(OR.locators.storyteller.svgPageStoryteller.modelOKButton));
        this.breadcurmbsList = element(By.css(OR.locators.storyteller.svgPageStoryteller.breadcurmbsList)).all(by.tagName("li"));
        this.confirmModalSaveButton = element(By.css(OR.locators.storyteller.svgPageStoryteller.confirmModalSaveButton));
        this.deleteButton = element(By.css(OR.locators.storyteller.utilityPanelStoryteller.deleteButton));
        this.warningPopUP = element(By.css(OR.locators.storyteller.utilityPanelStoryteller.warningPopUP));
        this.warningPopUpOKButton = element(By.css(OR.locators.storyteller.utilityPanelStoryteller.warningPopUpOKButton));
        this.discardButton = element(By.css(OR.locators.storyteller.utilityPanelStoryteller.discardButton));
        this.discardWarningPopUpOKButton = element(By.css(OR.locators.storyteller.utilityPanelStoryteller.discardWarningPopUpOKButton));
        this.saveButton = element(By.css(OR.locators.storyteller.utilityPanelStoryteller.saveButton));
        this.saveButtonDisable = element(By.css(OR.locators.storyteller.utilityPanelStoryteller.saveButtonDisable));
        this.userStoryLinkAtReviewTraceTab = element(By.css(OR.locators.storyteller.svgPageStoryteller.userStoryLinkAtReviewTraceTab));
        this.userStoryLinkAtFileTab = element(By.css(OR.locators.storyteller.svgPageStoryteller.userStoryLinkAtFileTab));
    }
    Object.defineProperty(Svgelementspages.prototype, "getLabelBody", {
        // public get getLabelHeader(): ElementArrayFinder { return this.labelHeader; }
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
    Object.defineProperty(Svgelementspages.prototype, "getPanelRelationships", {
        get: function () { return this.panelRelationships; },
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
    Object.defineProperty(Svgelementspages.prototype, "getPanelDiscussionPostButton", {
        get: function () { return this.panelDiscussionPostButton; },
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
    Object.defineProperty(Svgelementspages.prototype, "getGenerateUserStoriesMenuButton", {
        get: function () { return this.generateUserStoriesMenuButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getGenerateUserStoriesMenuIteams", {
        get: function () { return this.generateUserStoriesMenuIteams; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getFooterModelTitle", {
        get: function () { return this.footerModelTitle; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getFooterModelCancelButton", {
        get: function () { return this.footerModelCancelButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getFooterEditDetailButton", {
        get: function () { return this.footerEditDetailButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getFooterAddCommentButton", {
        get: function () { return this.footerAddCommentButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getFooterReviewTracesButton", {
        get: function () { return this.footerReviewTracesButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getFooterAddImageMockUpsScreenshotsButton", {
        get: function () { return this.footerAddImageMockUpsScreenshotsButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getFooterViewUserStoriesButton", {
        get: function () { return this.footerViewUserStoriesButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getFooterAddIncludesButton", {
        get: function () { return this.footerAddIncludesButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getAddTaskButton", {
        get: function () { return this.addTaskButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getAddTaskItems", {
        get: function () { return this.addTaskItems; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getActiveTabInModal", {
        get: function () { return this.activeTabInModal; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getViewUserStoriesGherkinTitle", {
        get: function () { return this.viewUserStoriesGherkinTitle; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getShowMoreButtonAtModel", {
        get: function () { return this.showMoreButtonAtModel; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getIncludeArtifactTextBox", {
        get: function () { return this.includeArtifactTextBox; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getIncludeArtifactDropdownList", {
        get: function () { return this.includeArtifactDropdownList; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getModelOKButton", {
        get: function () { return this.modelOKButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getBreadcurmbsList", {
        get: function () { return this.breadcurmbsList; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getIncludeButtonAtModel", {
        get: function () { return this.includeButtonAtModel; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getConfirmModalSaveButton", {
        get: function () { return this.confirmModalSaveButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getDeleteButton", {
        get: function () { return this.deleteButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getWarningPopUP", {
        get: function () { return this.warningPopUP; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getWarningPopUpOKButton", {
        get: function () { return this.warningPopUpOKButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getDiscardButton", {
        get: function () { return this.discardButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getDiscardWarningPopUpOKButton", {
        get: function () { return this.discardWarningPopUpOKButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getSaveButton", {
        get: function () { return this.saveButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getSaveButtonDisable", {
        get: function () { return this.saveButtonDisable; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getuserStoryLinkAtReviewTraceTab", {
        get: function () { return this.userStoryLinkAtReviewTraceTab; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Svgelementspages.prototype, "getuserStoryLinkAtFileTab", {
        get: function () { return this.userStoryLinkAtFileTab; },
        enumerable: true,
        configurable: true
    });
    // funtion for edit shape's header
    Svgelementspages.prototype.editHeader = function (shape, headerName) {
        this.labelHeader.then(function (elements) {
            console.log("Total is label from edit header " + elements.length);
            if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
                browser.actions().doubleClick(elements[shape]).perform(); //needed due to element location
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
    Svgelementspages.prototype.verifyHeaderName = function (shape) {
        var _this = this;
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.labelHeader), 100000);
        return this.labelHeader.then(function (elements) {
            elements[shape].getText().then(function (gettext) {
                _this.header = gettext;
            });
            return _this.header;
        });
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
    Svgelementspages.prototype.verifyBodyText = function (shape) {
        var _this = this;
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.labelBody), 100000);
        return this.labelBody.then(function (elements) {
            elements[shape].getText().then(function (gettext) {
                _this.body = gettext;
            });
            return _this.body;
        });
    };
    // function to edit body text for user task
    // TO DO: this funtion can be deleted but wait until dev finalized their code 
    Svgelementspages.prototype.editBodyForUserTask = function (shape, bodyText) {
        this.label.then(function (elements) {
            console.log("Total is label for label " + elements.length);
            if (shape <= elements.length) {
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
        this.image.then(function (el) {
            console.log("Finding Icon from Footer" + el.length);
            // NEED FOR FIREFOX
            el[icon].element(by.xpath('..'))
                .isDisplayed()
                .then(function (d) {
                console.log("Image display : " + d);
            });
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
    // function for generate user story menu items
    Svgelementspages.prototype.generateUserStoiesDropDownMenu = function (menuItem) {
        return this.generateUserStoriesMenuIteams.all(by.tagName('li')).then(function (elements) {
            return elements[menuItem];
        });
    };
    // function to find footer edit detail Button
    Svgelementspages.prototype.navFooterEditDetailButton = function (button) {
        return this.footerEditDetailButton.then(function (elements) {
            logger.info("Total edit detail button is  :" + elements.length);
            console.log("Total edit detail button is  :" + elements.length);
            return elements[button];
        });
    };
    // function to find footer Discussion Button
    Svgelementspages.prototype.navFooterAddCommentButton = function (button) {
        return this.footerAddCommentButton.then(function (elements) {
            logger.info("Total Add Comment button is  :" + elements.length);
            console.log("Total Add Comment button is  :" + elements.length);
            return elements[button];
        });
    };
    // function to find footer Relationships Button
    Svgelementspages.prototype.navFooterReviewTracesButton = function (button) {
        return this.footerReviewTracesButton.then(function (elements) {
            logger.info("Total Review Traces button is  :" + elements.length);
            console.log("Total Review Traces button is  :" + elements.length);
            return elements[button];
        });
    };
    // function to find footer Mockup Button
    Svgelementspages.prototype.navFooterAddImageMockUpsScreenshotsButton = function (button) {
        return this.footerAddImageMockUpsScreenshotsButton.then(function (elements) {
            logger.info("Total Add Image Mockups screenshot button is  :" + elements.length);
            console.log("Total Add Image Mockups screenshot button is  :" + elements.length);
            return elements[button];
        });
    };
    // function to find footer user story preview Button
    Svgelementspages.prototype.navFooterViewUserStoriesButton = function (button) {
        return this.footerViewUserStoriesButton.then(function (elements) {
            logger.info("Total view user stories button is  :" + elements.length);
            console.log("Total view user stories button is  :" + elements.length);
            return elements[button];
        });
    };
    // function to find footer include Button
    Svgelementspages.prototype.navFooterAddIncludesButton = function (button) {
        return this.footerAddIncludesButton.then(function (elements) {
            logger.info("Total Add Includes button is  :" + elements.length);
            console.log("Total Add Includes button is  :" + elements.length);
            return elements[button];
        });
    };
    // function to find add button (+)
    Svgelementspages.prototype.navAddTaskButton = function (button) {
        return this.addTaskButton.then(function (elements) {
            logger.info("Total Add Task button is  :" + elements.length);
            console.log("Total Add Task button is  :" + elements.length);
            return elements[button];
        });
    };
    // Function to find item in Add button
    Svgelementspages.prototype.selectAddItem = function (item) {
        return this.addTaskItems.then(function (elements) {
            logger.info("Total Add Task Items is  :" + elements.length);
            console.log("Total Add Task Items is  :" + elements.length);
            return elements[item];
        });
    };
    // Function to search Artifacts
    Svgelementspages.prototype.artifactsSearchResultCount = function (searchString) {
        this.getIncludeArtifactTextBox.sendKeys(searchString);
        return this.getIncludeArtifactDropdownList.then(function (elements) {
            logger.info("Total artifacts item in the dropdown list is :" + elements.length);
            console.log("Total artifacts item in the dropdown list is ::" + elements.length);
            return elements.length;
        });
    };
    return Svgelementspages;
})();
module.exports = Svgelementspages;
//# sourceMappingURL=SvgElementsPage.js.map