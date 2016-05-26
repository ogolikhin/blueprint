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
import WebElement = protractor.WebElement;
var logger = require('winston');

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
    private generateUserStoriesMenuButton: ElementFinder;
    private generateUserStoriesMenuIteams: ElementArrayFinder;
    private overlayDB: ElementArrayFinder;
    private footerDiscussionButton: ElementArrayFinder;
    private footerModelTitle: ElementFinder;
    private footerModelCancelButton: ElementFinder;
    private footerEditDetailButton: ElementArrayFinder;
    private footerAddCommentButton: ElementArrayFinder;
    private footerReviewTracesButton: ElementArrayFinder;
    private footerAddImageMockUpsScreenshotsButton: ElementArrayFinder;
    private footerViewUserStoriesButton: ElementArrayFinder;
    private footerAddIncludesButton: ElementArrayFinder;
    private addTaskButton: ElementArrayFinder;
    private addTaskItems: ElementArrayFinder;
    private activeTabInModal: ElementFinder;
    private viewUserStoriesGherkinTitle: ElementArrayFinder;
    private showMoreButtonAtModel: ElementFinder;
    private includeButtonAtModel: ElementArrayFinder;
    private includeArtifactTextBox: ElementFinder;
    private includeArtifactDropdownList: ElementArrayFinder;
    private modelOKButton: ElementFinder;
    private breadcurmbsList: ElementArrayFinder;
    private confirmModalSaveButton: ElementFinder;
    private deleteButton: ElementFinder;
    private warningPopUP: ElementFinder;
    private warningPopUpOKButton: ElementFinder;
    private discardButton: ElementFinder;
    private discardWarningPopUpOKButton: ElementFinder;
    private saveButton: ElementFinder;
    private saveButtonDisable: ElementFinder;
    private userStoryLinkAtReviewTraceTab: ElementFinder;
    private userStoryLinkAtFileTab: ElementFinder;


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
   // public get getLabelHeader(): ElementArrayFinder { return this.labelHeader; }
    public get getLabelBody(): ElementArrayFinder { return this.labelBody; }
    public get getlabel(): ElementArrayFinder { return this.label; }
    public get getImage(): ElementArrayFinder { return this.image; }
    public get getPanelDiscussions(): ElementArrayFinder { return this.panelDiscussions; }
    public get getPanelProperties(): ElementArrayFinder { return this.panelProperties; }
    public get getPanelFiles(): ElementArrayFinder { return this.panelFiles; }
    public get getPanelRelationships(): ElementArrayFinder { return this.panelRelationships; }
    public get getPanelHistory(): ElementArrayFinder { return this.panelHistory; }
    public get getpanelPreview(): ElementArrayFinder { return this.panelPreview; }
    public get getPanelCloseButton(): ElementFinder { return this.panelCloseButton; }
    public get getPanelDiscussionTextArea(): ElementFinder { return this.panelDiscussionTextArea; }
    public get getPanelDiscussionTextAreaBody(): ElementFinder { return this.panelDiscussionTextAreaBody; }
    public get getPanelDiscussionPostButton(): ElementFinder { return this.panelDiscussionPostButton; }
    public get getStorytellerToggleTextForBusniessProcess(): ElementFinder { return this.storytellerToggleTextForBusniessProcess; }
    public get getStorytellerToggleTextForUserSystemProcess(): ElementFinder { return this.storytellerToggleTextForUserSystemProcess; }
    public get getStorytellerTogglecheckBox(): ElementFinder { return this.storytellerTogglecheckBox; }
    public get getPublishArtifact(): ElementFinder { return this.publishArtifact; }
    public get getPublishArtifactSucessMessage(): ElementFinder { return this.publishArtifactSucessMessage; }
    public get getPostCommentText(): ElementFinder { return this.postCommentText; }
    public get getGenerateUserStoriesMenuButton(): ElementFinder { return this.generateUserStoriesMenuButton; }
    public get getGenerateUserStoriesMenuIteams(): ElementArrayFinder { return this.generateUserStoriesMenuIteams; }
    public get getFooterModelTitle(): ElementFinder { return this.footerModelTitle; }
    public get getFooterModelCancelButton(): ElementFinder { return this.footerModelCancelButton; }
    public get getFooterEditDetailButton(): ElementArrayFinder { return this.footerEditDetailButton; }
    public get getFooterAddCommentButton(): ElementArrayFinder { return this.footerAddCommentButton; }
    public get getFooterReviewTracesButton(): ElementArrayFinder { return this.footerReviewTracesButton; }
    public get getFooterAddImageMockUpsScreenshotsButton(): ElementArrayFinder { return this.footerAddImageMockUpsScreenshotsButton; }
    public get getFooterViewUserStoriesButton(): ElementArrayFinder { return this.footerViewUserStoriesButton; }
    public get getFooterAddIncludesButton(): ElementArrayFinder { return this.footerAddIncludesButton; }
    public get getAddTaskButton(): ElementArrayFinder { return this.addTaskButton; }
    public get getAddTaskItems(): ElementArrayFinder { return this.addTaskItems; }
    public get getActiveTabInModal(): ElementFinder { return this.activeTabInModal; }
    public get getViewUserStoriesGherkinTitle(): ElementArrayFinder { return this.viewUserStoriesGherkinTitle; }
    public get getShowMoreButtonAtModel(): ElementFinder { return this.showMoreButtonAtModel; }
    public get getIncludeArtifactTextBox(): ElementFinder { return this.includeArtifactTextBox; }
    public get getIncludeArtifactDropdownList(): ElementArrayFinder { return this.includeArtifactDropdownList; }
    public get getModelOKButton(): ElementFinder { return this.modelOKButton; }
    public get getBreadcurmbsList(): ElementArrayFinder { return this.breadcurmbsList; }
    public get getIncludeButtonAtModel(): ElementArrayFinder { return this.includeButtonAtModel; }
    public get getConfirmModalSaveButton(): ElementFinder { return this.confirmModalSaveButton; }
    public get getDeleteButton(): ElementFinder { return this.deleteButton; }
    public get getWarningPopUP(): ElementFinder { return this.warningPopUP; }
    public get getWarningPopUpOKButton(): ElementFinder { return this.warningPopUpOKButton; }
    public get getDiscardButton(): ElementFinder { return this.discardButton; }
    public get getDiscardWarningPopUpOKButton(): ElementFinder { return this.discardWarningPopUpOKButton; }
    public get getSaveButton(): ElementFinder { return this.saveButton; }
    public get getSaveButtonDisable(): ElementFinder { return this.saveButtonDisable; }
    public get getuserStoryLinkAtReviewTraceTab(): ElementFinder { return this.userStoryLinkAtReviewTraceTab; }
    public get getuserStoryLinkAtFileTab(): ElementFinder { return this.userStoryLinkAtFileTab; }
    
    // funtion for edit shape's header
    public editHeader(shape: number, headerName: any): void {
        this.labelHeader.then((elements) => {
            console.log("Total is label from edit header " + elements.length);

            if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
                browser.actions().doubleClick(elements[shape]).perform();//needed due to element location
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
    public verifyHeaderName(shape: number): Promise<string> {
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.labelHeader), 100000);
        return this.labelHeader.then((elements) => {
            elements[shape].getText().then((gettext) => {
                this.header = gettext; 
            });
            return this.header;
        });

    }

//funtion to edit body  of a shape

    public  editBody(shape: number, bodyText: any):void {
        this.labelBody.then((elements) =>{
        console.log("Total is label " + elements.length);
        if (shape <= elements.length) {
                browser.actions().doubleClick(elements[shape]).perform();
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

    public verifyBodyText(shape: number): Promise<string> {
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.labelBody), 100000);
        return this.labelBody.then((elements) => {
            elements[shape].getText().then((gettext) => {
                this.body = gettext;
            });
            return this.body;
        });

    }

 
    // function to edit body text for user task
    // TO DO: this funtion can be deleted but wait until dev finalized their code 
    public editBodyForUserTask(shape: number, bodyText: any) {
        this.label.then((elements) => {
            console.log("Total is label for label " + elements.length);
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

// function to verify  body text for user task
    //TO DO this funtion can be deleted but wait until dev finalized their code 

    public verifyUserTaskBodyText(shape: number): Promise<string> {
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

    public findElementAndSelect(shape: number): Promise<protractor.WebElement> {
        browser.wait(arrayListPresenceOfAll.presenceOfAll(this.label), 100000);
        return this.label.then((elements) =>{
            console.log("inside of finding element");
            console.log("Total is label from find and select " + elements.length);
            return elements[shape];
        });
    }


//funtion to find footer and information icon of a shape

    public findFooterAndInfoIcon(icon: number): void {

        this.image.then((el) =>{
            console.log("Finding Icon from Footer" + el.length);
            // NEED FOR FIREFOX
            el[icon].element(by.xpath('..'))
                .isDisplayed()
                .then((d) => {
                    console.log("Image display : "+d);
                });
            el[icon].click();

        });
    }

 
// function to post a comment at discussion panel
    public postComment(comment: any): void {
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
// function for generate user story menu items
   public generateUserStoiesDropDownMenu(menuItem: number): Promise<protractor.WebElement> {
       return this.generateUserStoriesMenuIteams.all(by.tagName('li')).then((elements) => {
           return elements[menuItem];
       });

   }


// function to find footer edit detail Button

   public navFooterEditDetailButton(button: number): Promise<protractor.WebElement>  {
      return this.footerEditDetailButton.then((elements) => {
               logger.info("Total edit detail button is  :" + elements.length);
               console.log("Total edit detail button is  :" + elements.length);
                return elements[button];
            });
   }

// function to find footer Discussion Button

   public navFooterAddCommentButton(button: number): Promise<protractor.WebElement> {
       return this.footerAddCommentButton.then((elements) => {
                logger.info("Total Add Comment button is  :" + elements.length);
                console.log("Total Add Comment button is  :" + elements.length);
               return elements[button];
           });
   }

// function to find footer Relationships Button

   public navFooterReviewTracesButton(button: number): Promise<protractor.WebElement> {
       return this.footerReviewTracesButton.then((elements) => {
                logger.info("Total Review Traces button is  :" + elements.length);
                console.log("Total Review Traces button is  :" + elements.length);
               return elements[button];
           });
   }
 // function to find footer Mockup Button

   public navFooterAddImageMockUpsScreenshotsButton(button: number): Promise<protractor.WebElement> {
       return this.footerAddImageMockUpsScreenshotsButton.then((elements) => {
                logger.info("Total Add Image Mockups screenshot button is  :" + elements.length);
                console.log("Total Add Image Mockups screenshot button is  :" + elements.length);
               return elements[button];
           });

   }

// function to find footer user story preview Button

   public navFooterViewUserStoriesButton(button: number): Promise<protractor.WebElement> {
       return this.footerViewUserStoriesButton.then((elements) => {
                logger.info("Total view user stories button is  :" + elements.length);
                console.log("Total view user stories button is  :" + elements.length);
                return elements[button];
            });

   }

// function to find footer include Button

   public navFooterAddIncludesButton(button: number): Promise<protractor.WebElement> {
       return this.footerAddIncludesButton.then((elements) => {
                 logger.info("Total Add Includes button is  :" + elements.length);
                 console.log("Total Add Includes button is  :" + elements.length);
               return elements[button];
           });
    }

// function to find add button (+)

   public navAddTaskButton(button: number): Promise<protractor.WebElement> {
       return this.addTaskButton.then((elements) => {
                logger.info("Total Add Task button is  :" + elements.length);
                console.log("Total Add Task button is  :" + elements.length);
                return elements[button];

            });
    }

// Function to find item in Add button

   public selectAddItem(item: number): Promise<protractor.WebElement> {
        return this.addTaskItems.then((elements) => {
                logger.info("Total Add Task Items is  :" + elements.length);
                console.log("Total Add Task Items is  :" + elements.length);
                return elements[item];
            });
   }

   // Function to search Artifacts

   public artifactsSearchResultCount(searchString: string): Promise<number> {
       this.getIncludeArtifactTextBox.sendKeys(searchString);
       return this.getIncludeArtifactDropdownList.then((elements) => {
                logger.info("Total artifacts item in the dropdown list is :" + elements.length);
                console.log("Total artifacts item in the dropdown list is ::" + elements.length);
           return elements.length;
       });
   }


}

export = Svgelementspages;