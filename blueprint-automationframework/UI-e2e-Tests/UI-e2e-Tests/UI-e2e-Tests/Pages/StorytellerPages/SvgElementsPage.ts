/**
 * This class file will contain all elements and action on element for svg shapes 
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * 
 */

var OR = require('../../Json/OR.json');
import Promise = protractor.promise.Promise;
var ttt;

class Svgelementspages {
    private static labelHeader = element.all(by.css(OR.locators.storyteller.labelHeader));
    private static labelBody = element.all(by.css(OR.locators.storyteller.labelBody));
    private static image = element.all(By.tagName(OR.locators.storyteller.image));
    private static panelDiscussions = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelDiscussions));
    private static panelProperties = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelProperties));
    private static panelFiles = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelFiles));
    private static panelrelationships = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelrelationships));
    private static panelHistory = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelHistory));
    private static panelPreview = element.all(by.id(OR.locators.storyteller.panelModalWUtilityPanel)).all(by.id(OR.locators.storyteller.panelPreview));
    private static panelCloseButton = element(By.css(OR.locators.storyteller.panelCloseButton));
    private static panelDiscussionTextArea = element(By.id(OR.locators.storyteller.panelDiscussionTextArea));
    private static panelDiscussionTextAreaBody = browser.element(By.id(OR.locators.storyteller.panelDiscussionTextAreaBody));
    private static panelDiscussionPostButton = browser.element(By.css(OR.locators.storyteller.panelDiscussionPostButton));

    //funtion to edit header of a shape
    public static editHeader(shape, headerName) {
        Svgelementspages.labelHeader.then((elements) =>{
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

    //funtion to verify header name of a shape

    public static verifyHeaderName(shape, headername) {
        Svgelementspages.labelHeader.then((elements) =>{
            browser.driver.sleep(5000);
            console.log("inside of finding element");
            console.log("Total is label " + elements.length);
            elements[shape].all(by.tagName('div')).then((el)=> {
            console.log("inside of finding div");
            el[1].getText().then((gettext)=>{
                    console.log("inside of finding getText");
                    console.log("Header text is -----" + gettext);
                    expect(gettext).toBe(headername);
                    console.log("OUT OF FINDING TEXT");
                });

            });

        });
    }

    //funtion to edit body  of a shape

    public static editBody(shape, bodyText) {
        Svgelementspages.labelBody.then((elements) =>{
        console.log("Total is label " + elements.length);
        if (shape <= elements.length) {
               browser.actions().doubleClick(elements[shape]).perform();
               browser.actions().sendKeys(protractor.Key.DELETE).perform();
               browser.actions().sendKeys(bodyText).perform();
               browser.driver.sleep(5000);
               browser.actions().sendKeys('\n').perform();
               browser.driver.sleep(5000);
               console.log("End of edtign");

           } else {
               console.log("Your element is not in array-this is custom error message");
           }


       });

    }

      //funtion to  verify body text  of a shape

    public static verifyBodyText(shape, bodyText) {

        Svgelementspages.labelBody.then((elements1)=>{
            browser.driver.sleep(5000);
            console.log("inside of finding element");
            console.log("Total is label " + elements1.length);
            elements1[shape].all(by.tagName('div')).then((el)=>{
            console.log("inside of finding div");
                el[1].getText().then((gettext)=> {
                    console.log("inside of finding getText");
                    console.log("Body text is -----" + gettext);
                    expect(gettext).toBe(bodyText);
                    console.log("OUT OF FINDING TEXT");
                });

            });

        });
    }

    //funtion to find an element and select the shape

    public static findElementAndSelect(shape) {
        Svgelementspages.labelHeader.then((elements) =>{
            browser.driver.sleep(5000);
            console.log("inside of finding element");
            console.log("Total is label " + elements.length);
            elements[shape].click();

        });
    }


    //funtion to find footer and information icon of a shape

    public static findFooterAndInfoIcon() {
        var x = element.all(By.tagName(OR.locators.storyteller.image)); 
        Svgelementspages.image.then((el) =>{
            console.log("Finding Icon " + el.length);
            el[19].click();
            browser.driver.sleep(5000);

        });
    }

    //funtion to verify Panel Discussion

    public static verifyPanelDiscussion() {
        Svgelementspages.panelDiscussions.click();
        expect(Svgelementspages.panelDiscussions.getText()).toEqual(['Discussions']);
        
   }

    //funtion to verify Panel Properties
   public static verifyPanelProperties() {
       Svgelementspages.panelProperties.click();
       expect(Svgelementspages.panelProperties.getText()).toEqual(['Properties']);
       
   }

     //funtion to verify Panel File
   public static verifyPanelFiles() {
       Svgelementspages.panelFiles.click();
       expect(Svgelementspages.panelFiles.getText()).toEqual(['Files']);
       
   }

    //funtion to verify Panel Relationships
   public static verifyPanelRelationships() {
       Svgelementspages.panelrelationships.click();
       expect(Svgelementspages.panelrelationships.getText()).toEqual(['Relationships']);
       
   }

    //funtion to verify Panel History
   public static verifyPanelHistory() {
       Svgelementspages.panelHistory.click();
       expect(Svgelementspages.panelHistory.getText()).toEqual(['History']);
       
   }


    //funtion to verify Panel Preview
   public static verifyPanelPreview() {
       Svgelementspages.panelPreview.click();
       expect(Svgelementspages.panelPreview.getText()).toEqual(['Preview']);
       
   }


     //funtion to verify Panel CloseButton
   public static verifyPanelCloseButton() {
       Svgelementspages.panelCloseButton.click();
   }

   //funtion to verify Panel Post Comment
   public static verifyPostComment() {
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
   }
}

export = Svgelementspages;