/**
 * This class file will contain all elements and action on element for svg shapes 
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * last modified by:
 * Last modified on:
 */

var OR = require('../../Json/OR.json');
import Promise = protractor.promise.Promise;
import ElementFinder = protractor.ElementFinder;
import ElementArrayFinder = protractor.ElementArrayFinder;
import arrayListPresenceOfAll = require("../../Utility/ArrayListPresenceOfAll");



class Svgelementspages {
    header: string;
    body: string;
    labelForUserTaskBody: string;
    private labelHeader: ElementArrayFinder;
    private labelBody: ElementArrayFinder;
    private label: ElementArrayFinder;
    private image: ElementArrayFinder;
    private  panelDiscussions: ElementArrayFinder;
    private panelProperties: ElementArrayFinder;
    private panelFiles: ElementArrayFinder;
    private panelRelationships: ElementArrayFinder;
    private panelHistory: ElementArrayFinder;
    private panelPreview: ElementArrayFinder;
    private panelCloseButton: ElementFinder;
    private panelDiscussionTextArea: ElementFinder;
    private panelDiscussionTextAreaBody: ElementFinder;
    private panelDiscussionPostButton: ElementFinder;
    private storytellerToggleTextForBusniessProcess: ElementFinder;
    private storytellerToggleTextForUserSystemProcess: ElementFinder;
    private storytellerTogglecheckBox: ElementFinder;
    private publishArtifact: ElementFinder;
    private publishArtifactSucessMessage: ElementFinder;
    private postCommentText: ElementFinder;

    constructor() {
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
    public get getLabelHeader(): ElementArrayFinder { return this.labelHeader; }
    public get getLabelBody(): ElementArrayFinder { return this.labelBody; }
    public get getlabel(): ElementArrayFinder { return this.label; }
    public get getImage(): ElementArrayFinder { return this.image; }
    public get getPanelDiscussions(): ElementArrayFinder { return this.panelDiscussions; }
    public get getPanelProperties(): ElementArrayFinder { return this.panelProperties; }
    public get getPanelFiles(): ElementArrayFinder { return this.panelFiles; }
    public get getPanelHistory(): ElementArrayFinder { return this.panelHistory; }
    public get getpanelPreview(): ElementArrayFinder { return this.panelPreview; }
    public get getPanelCloseButton(): ElementFinder { return this.panelCloseButton; }
    public get getPanelDiscussionTextArea(): ElementFinder { return this.panelDiscussionTextArea; }
    public get getPanelDiscussionTextAreaBody(): ElementFinder { return this.panelDiscussionTextAreaBody; }
    public get getStorytellerToggleTextForBusniessProcess(): ElementFinder { return this.storytellerToggleTextForBusniessProcess; }
    public get getStorytellerToggleTextForUserSystemProcess(): ElementFinder { return this.storytellerToggleTextForUserSystemProcess; }
    public get getStorytellerTogglecheckBox(): ElementFinder { return this.storytellerTogglecheckBox; }
    public get getPublishArtifact(): ElementFinder { return this.publishArtifact; }
    public get getPublishArtifactSucessMessage(): ElementFinder { return this.publishArtifactSucessMessage; }
    public get getPostCommentText(): ElementFinder { return this.postCommentText; }

// funtion for edit shape's header
    public  editHeader(shape, headerName) {
        this.labelHeader.then((elements) =>{
            console.log("Total is label " + elements.length);
            
            if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
                browser.actions().sendKeys(protractor.Key.DELETE).perform();
                browser.actions().sendKeys(headerName).perform();
                browser.actions().sendKeys('\n').perform();
                console.log("End of editing");

            } else {
                console.log("Your element is not in array-this is custom error message");
            }


        });
    }

// function to verify shape's header name
    public verifyHeaderName(shape): Promise<string> {
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

    }

//funtion to edit body  of a shape

    public  editBody(shape, bodyText) {
        this.labelBody.then((elements) =>{
        console.log("Total is label " + elements.length);
        if (shape <= elements.length) {
               browser.actions().doubleClick(elements[shape]).perform();
               browser.actions().sendKeys(protractor.Key.DELETE).perform();
               browser.actions().sendKeys(bodyText).perform();
               browser.actions().sendKeys('\n').perform();
               console.log("End of edtign");

           } else {
               console.log("Your element is not in array-this is custom error message");
        }
        });

    }
//funtion to verify text body  of a shape

    public verifyBodyText(shape): Promise<string> {
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

    }
    
    // function to edit body text for user task
    // TO DO: this function might not be needed if we can have same body ID pattern as system task
    public editBodyForUserTask(shape, bodyText) {
        this.label.then((elements) => {
            console.log("Total is label for label " + elements.length);
            if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
                browser.actions().sendKeys(protractor.Key.DELETE).perform();
                browser.actions().sendKeys(bodyText).perform();
                // browser.driver.sleep(5000);
                browser.actions().sendKeys('\n').perform();
                // browser.driver.sleep(5000);
                console.log("End of edtign");

            } else {
                console.log("Your element is not in array-this is custom error message");
            }


        });

    }

// function to verify  body text for user task
    // TO DO: this function might not be needed if we can have same body ID pattern as system task

    public verifyUserTaskBodyText(shape): Promise<string> {
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.label), 100000);
        return this.label.then((elements) => {
            elements[shape].all(by.tagName('div')).then((el) => {
                console.log("inside of finding div");
                el[1].getText()
                    .then((gettext) => {
                        console.log(gettext);
                        this.labelForUserTaskBody = gettext;
                        console.log("inside " + this.labelForUserTaskBody);
                        return this.labelForUserTaskBody;
                    });
            });
            return this.labelForUserTaskBody;
        });

    }

//funtion to find an element and select the shape

    public  findElementAndSelect(shape): Promise<protractor.WebElement> {
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.labelHeader), 100000);
        return this.labelHeader.then((elements) =>{
          
            console.log("inside of finding element");
            console.log("Total is label " + elements.length);
            return elements[shape];
        });
    }


//funtion to find footer and information icon of a shape

    public  findFooterAndInfoIcon(icon) {
        var x = element.all(By.tagName(OR.locators.storyteller.image)); 
        this.image.then((el) =>{
            console.log("Finding Icon " + el.length);
            el[icon].click();
        });
    }

 
// function to post a comment at discussion panel
   public postComment(comment) {
       browser.driver.sleep(2000);
       this.panelDiscussions.click();
       this.panelDiscussionTextArea.click();
       browser.driver.switchTo().frame(OR.locators.storyteller.svgPageStoryteller.panelDiscussionTextAreaIframeId);
       browser.ignoreSynchronization = true;
       this.panelDiscussionTextAreaBody.isPresent()
           .then((tt) => {
               console.log(tt);
               if (tt === true) {
                   this.panelDiscussionTextAreaBody.sendKeys(comment);
                   browser.driver.switchTo().defaultContent();
               }
               if (tt === false) {
                   fail("Eelement not found");
                   browser.driver.switchTo().defaultContent();
               }
           });
   }
}

export = Svgelementspages;