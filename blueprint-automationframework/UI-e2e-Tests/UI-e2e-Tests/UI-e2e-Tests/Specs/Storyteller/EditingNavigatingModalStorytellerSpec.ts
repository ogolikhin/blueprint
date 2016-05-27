/**
 * This spec file will contain All major locator and action that can be performed on storyteller svg(graphic)
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * last modified by:
 * Last modified on:
 */
import Page = require("../../Pages/StorytellerPages/SvgElementsPage");
import createArtifact = require("../../Model/CreateArtifacts");
var OR = require('../../Json/OR.json');
var logger = require('winston');
var svgElementsPage: Page;

describe("Storyteller end to end test", () => {
    beforeAll(() => {
        //Arrange-global
    
        svgElementsPage = new Page();

    });

    afterAll(() => {
        //cleanup
        browser.clearMockModules['svgElementsPage'];
    });

    describe("Editing-Navigating- Shapes and Info panel of Storyteller", () => {
        // logger.info("=======Start Editing-Navigating- Shapes and Info panel of Storyteller=======");
        

        it("Should be able to toggle to user system process", () => {
            //Act           
            if (svgElementsPage.getStorytellerTogglecheckBox.isSelected()) {
                svgElementsPage.getStorytellerTogglecheckBox.click();
            }
            //Assert
            expect(svgElementsPage.getStorytellerToggleTextForUserSystemProcess.getText()).toBe('User-System Process');

        });

        it("Should be able to publish the artifact", () => {
            //Act
            browser.driver.sleep(5000);
            svgElementsPage.getPublishArtifact.click();
            
            // Assert
            expect(svgElementsPage.getPublishArtifactSucessMessage.getText()).toBe("The Process has been published.");

        });
        it("Should be able generate user story", () => {
            //Act
            svgElementsPage.getGenerateUserStoriesMenuButton.click();
            svgElementsPage.generateUserStoiesDropDownMenu(1).then((el) => { el.click(); });//@parm 1 for generate All, 0 for generate from user task
            //Assert
            //TODO need to find the elment for sucess message
            //expect(svgElementsPage.getPublishArtifactSucessMessage.getText()).toBe("The Process has been published.");
            //browser.driver.sleep(5000);
            //expect(element(by.css("[data-ng-repeat=\"m in messages | filter:{ messageType: 2 } as results\"]"))).toBe("The Process has been published.");
            
            
          });
          it("Should be able to edit system precondition shape header", () => {
            //Act
            browser.driver.sleep(5000);
            svgElementsPage.editHeader(0, "s1");
            browser.driver.sleep(5000);
            svgElementsPage.verifyHeaderName(0).then((t) => { });
            //Assert
            expect(svgElementsPage.verifyHeaderName(0)).toBe("s1");
                        
           });
                 
            it("Should be able to edit user task shape header", () => {
                //Act
                svgElementsPage.editHeader(1, "User_T1");
                svgElementsPage.verifyHeaderName(1).then((t) => { });
                // Assert
                expect(svgElementsPage.verifyHeaderName(1)).toBe("User_T1");
            });
              
            it("Should be able to edit system task shape header", () => {
                //Act
                svgElementsPage.editHeader(2, "Sys_T1");
                svgElementsPage.verifyHeaderName(2).then((t) => { });
                // Assert
                expect(svgElementsPage.verifyHeaderName(2)).toBe("Sys_T1");
            });
                    
            it("Should be able to edit system precondition shape body", () => {
                //Act
                svgElementsPage.editBody(0, "Sys_P0B");
                svgElementsPage.verifyBodyText(0).then((t) => { });
                // Assert
                expect(svgElementsPage.verifyBodyText(0)).toBe("Sys_P0B");
                       
            });
            it("Should be able to edit user task shape body", () => {
                //Act
                svgElementsPage.editBody(1, "User_T1B");
                svgElementsPage.verifyBodyText(1).then((t) => { });
                // Assert 
                expect(svgElementsPage.verifyBodyText(1)).toBe("User_T1B");
                        
            });
             
            
            it("Should be able to edit system task shape body", () => {
                //Act
                svgElementsPage.editBody(2, "Sys_T2B");
                svgElementsPage.verifyBodyText(2).then((t) => { });
                //Assert
                expect(svgElementsPage.verifyBodyText(2)).toBe("Sys_T2B");
                        
            });
                    
                    
            it("Should be able to navigate to info Panel when click a user task", () => {
                //Act
                svgElementsPage.findElementAndSelect(2).then((el) => {
                browser.actions().click(el).perform();           
                }); 
                //Assert
                svgElementsPage.findFooterAndInfoIcon(19);

             });
             
                  
            it("Should be able to navigate Properties tab", ()=> {
                //Act
                svgElementsPage.getPanelProperties.click();
                //Assert
                expect(svgElementsPage.getPanelProperties.getText()).toEqual(['Properties']);
                        
            });
            
            it("Should be able to navigate Discussion tab", () => {
                     
                //Act
                svgElementsPage.getPanelDiscussions.click();
                //Assert
                expect(svgElementsPage.getPanelDiscussions.getText()).toEqual(['Discussions']);
                       
            });
            
            it("Should be able to post comment", () => {
                //Act
                svgElementsPage.postComment("This test case  need to be updated");
                svgElementsPage.getPanelDiscussionPostButton.click();
                //Assert
                expect(svgElementsPage.getPostCommentText.getText()).toBe("This test case  need to be updated");
                    browser.driver.sleep(1000);
            });
                   
            
            it("Should be able to navigate Files tab", () => {
                //Act
                svgElementsPage.getPanelFiles.click();
                //Assert
                expect(svgElementsPage.getPanelFiles.getText()).toEqual(['Files']);
                      
            });
            
            it("Should be able to navigate Relationships tab", () => {
                //Act
                svgElementsPage.getPanelRelationships.click();
                //Assert
                expect(svgElementsPage.getPanelRelationships.getText()).toEqual(['Relationships']);
                        
            });
            
            it("Should be able to navigate History tab", () => {
                //Act
                svgElementsPage.getPanelHistory.click();
                //Assert
                expect(svgElementsPage.getPanelHistory.getText()).toEqual(['History']);
            });
            
            it("Should be able to close info Panel",() => {
                //Act
                svgElementsPage.getPanelCloseButton.click();
                     
                //TO DO Assert
            });
                    

        });// end of test suite
    
        
    describe("Shape footer navigating", () => {
        logger.info("======= START Shape footer navigating Storyteller =======");
                
            it("Should be able to open Edit Detail modal at footer", () => {
                //Act
                svgElementsPage.navFooterEditDetailButton(2).then((el) => { el.click(); });//@parm shape index 
                //Assert
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalTitle.getWebElement()), 5000).then(() => {
                    expect(svgElementsPage.getFooterModalTitle.getText()).toBe("Sys_T2B");
                });

                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalCancelButton.getWebElement()), 5000).then(() => {
                    svgElementsPage.getFooterModalCancelButton.click();
                });       
            });
                    
            it("Should be able to open Add comment modal at footer",() => {
                //Act
                browser.driver.sleep(5000);
                svgElementsPage.navFooterAddCommentButton(2).then((el) => { el.click(); });//@parm shape index 
                //Assert
                browser.driver.sleep(5000);
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getActiveTabInModal.getWebElement()), 5000).then(() => {
                    expect(svgElementsPage.getActiveTabInModal.getText()).toBe("Discussions");
                });

                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getPanelCloseButton.getWebElement()), 5000).then(() => {
                    svgElementsPage.getPanelCloseButton.click();
                });

             });
             
            it("Should be able to open review traces modal at footer",() => {
                //Act
                svgElementsPage.navFooterReviewTracesButton(2).then((el) => { el.click(); });//@parm shape index 
                //Assert
                browser.driver.sleep(5000);
                expect(svgElementsPage.getActiveTabInModal.getText()).toBe("Relationships");
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getPanelCloseButton.getWebElement()), 5000).then(() => {
                    svgElementsPage.getPanelCloseButton.click();
                });
                
             });
              
            it("Should be able to open add Images-Mockups-Screenshots modal at footer",() => {
                //Act
                svgElementsPage.navFooterAddImageMockUpsScreenshotsButton(1).then((el) => { el.click(); });//@parm shape index 
                //Assert

                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalTitle.getWebElement()), 5000).then(() => {
                    expect(svgElementsPage.getFooterModalTitle.getText()).toBe("Sys_T2B");
                });

                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalCancelButton.getWebElement()), 5000).then(() => {
                    svgElementsPage.getFooterModalCancelButton.click();
                });

             });
                 
            it("Should be able to open view user stories modal at footer ", () => {
                //Act
                svgElementsPage.navFooterViewUserStoriesButton(0).then((el) => { el.click(); });//@parm shape index 
                //Assert
                browser.driver.sleep(5000);
                svgElementsPage.getViewUserStoriesGherkinTitle.then((el) => {
                    logger.info("Length of Gherkin title array is : " + + el.length);
                    console.log("Length of Gherkin title array is : " + el.length);
                    expect(el[0].getText()).toBe("Given");
                    expect(el[1].getText()).toBe("When");
                    expect(el[2].getText()).toBe("Then");
                });
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalCancelButton.getWebElement()), 5000).then(() => {
                    svgElementsPage.getFooterModalCancelButton.click();
                });

                
              });
                
          });// end of test suite
            
          
    describe("Include Artifacts and navigate to the attached artifacts thru Bread curmbs", () => {
        logger.info("======= START Include Artifacts and navigate to the attached artifacts thru Bread curmbs =======");
        
            it("Should be able  include Artifacts at Edit Detail modal at footer", () => {
                //Act
                //Open edit detail modal
                svgElementsPage.navFooterEditDetailButton(2).then((el) => { el.click(); });//@parm edit detail button index
                browser.driver.sleep(5000);
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getShowMoreButtonAtModal.getWebElement()), 5000).then(() => {
                    svgElementsPage.getShowMoreButtonAtModal.click();
                });
                
                    svgElementsPage.getIncludeButtonAtModal.then((el) => {
                        console.log("Total button at Edit detail modal is : " + el.length);
                        browser.driver.sleep(5000);
                        el[1].click();//@parm 0 for addtional info and 1 for include
                    
                });
                //Assert
                expect(svgElementsPage.artifactsSearchResultCount("fro")).toBeGreaterThan(0);
        
                //Act
                browser.driver.sleep(5000);
                svgElementsPage.getIncludeArtifactDropdownList.then((el) => { 
                    el[0].click(); //@parm search item index
                });
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getModalOKButton.getWebElement()), 5000).then(() => {

                    svgElementsPage.getModalOKButton.click();
                });

                        
                //Assert
                browser.driver.sleep(5000);
                svgElementsPage.getFooterAddIncludesButton.then((el) => {
                    el[2].getOuterHtml().then((imageTag) => { //@parm shape index 
                        var link = imageTag.match(/xlink:href="(.*?)"/);
                        var linkUrl = OR.mockData.baseURL + "/Areas/Web/Style/images/Storyteller/include-active.svg";
                        console.log(link[1]);
                    expect(link[1]).toBe(linkUrl);
        
                    });
                });
            });
        /*
        TODO: Need to refactor
           it("Should be able navigate to inclued Artifacts and navigate to parent artifact thru Breadcurmbs", () => { 
               //Act
               svgElementsPage.getFooterAddIncludesButton.then((el) => {
                   el[2].click(); //@parm shape index
               }); 
               browser.driver.sleep(5000);
               svgElementsPage.getConfirmModalSaveButton.click();// save button before navigate to inculded artifacts

               //Assert
               svgElementsPage.getBreadcurmbsList.then((el) => {
                   browser.driver.sleep(5000);
                   expect(el.length).toEqual(2);
                   browser.driver.sleep(5000);
                   el[0].click();//@parm Breadcurm item index
                   browser.driver.sleep(5000);
               });
               browser.driver.sleep(5000);
               svgElementsPage.getBreadcurmbsList.then((el) => {
                   expect(el.length).toEqual(1);
                   browser.driver.sleep(5000);
               });
           });
        */
        }); //end of test suite
      
         describe("User Story File download", () => {
             it("should be able to download the file", () => {
                 //Arrange
                 var fs = require('fs');
                 var directory = 'C:/DownloadFile';
                 var filename = 'C:/DownloadFile/UT.feature';
                 var content = 'Feature: UT\r\nAs a User, I want to UT\r\n\r\n\tScenario: 1\r\n\t\tGiven1 System is in Precondition\r\n\tWhen User attempts to UT\r\n\tThen System should ST\r\n\r\n'
                 fs.existsSync(directory) || fs.mkdirSync(directory);
                 //Act
                 svgElementsPage.navFooterReviewTracesButton(1).then((el) => { el.click(); });
                 browser.driver.sleep(5000);
                 //Assert
                 expect(svgElementsPage.getuserStoryLinkAtReviewTraceTab.isDisplayed()).toBeTruthy();
                 browser.driver.sleep(5000);
                 svgElementsPage.getuserStoryLinkAtReviewTraceTab.isDisplayed().then((el) => { console.log(el) });
                 svgElementsPage.getuserStoryLinkAtReviewTraceTab.click();
                 browser.driver.sleep(5000);
                 svgElementsPage.getPanelFiles.click();
                 browser.driver.sleep(5000);
                 svgElementsPage.getuserStoryLinkAtFileTab.click();
                 browser.driver.wait(() => {
    
                     return fs.existsSync(filename);
                 }, 30000).then(() => {
    
                     console.log(fs.existsSync(filename) ? "found" : "Not found");
                     expect(fs.existsSync(filename)).toBeTruthy();
                 });
    
                 //fs.unlinkSync(filename);
                 svgElementsPage.getPanelCloseButton.click();
                 browser.driver.sleep(5000);
                 //TODO This function is tem solution. Need to refactor
             });
         }); //end of test suite
    
        describe("Add-Delete-Discard-Save user task and add decision Point", () => {
            logger.info("======= Add-Delete-Discard-Save user task and add decision Point =======");
    
            it("Should be able to delete user task ", () => {
                //Act
               svgElementsPage.navAddTaskButton(2).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(1).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getDeleteButton.getWebElement()), 1000).then(() => { svgElementsPage.getDeleteButton.click(); });

                //Assert
                expect(svgElementsPage.getWarningPopUP.getText()).toBe("Please confirm the deletion of the selected user task.");//A warning pop up windows display with following contents
                svgElementsPage.getWarningPopUpOKButton.click();
                browser.driver.sleep(5000);
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body label  array is : " + + el.length);
                    console.log("Length of body label  array is: " + el.length);
                 expect(el.length).toEqual(3);//@parm is initial shape count. After delete total shape count should not increase.
                });
           }); 
    /*TODO: discard make the Artifacts read only. might be bug introduce on May 25
            it("Should be able discard user task ", () => {
                //Act
                svgElementsPage.navAddTaskButton(2).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(1).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                svgElementsPage.getDiscardButton.click();
                //Assert
                expect(svgElementsPage.getWarningPopUP.getText()).toBe("After discarding your changes, each artifact is restored to its last published version.");//A warning pop up windows display with following contents
                svgElementsPage.getDiscardWarningPopUpOKButton.click();
                //TODO
                //expect(svgElementsPage.getPublishArtifactSucessMessage.getText()).toBe("Changes Were Discarded");//should displaying sucess message
                browser.driver.sleep(1000);
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body level array is : " + + el.length);
                    console.log("Length of body level array is : " + el.length);
                 expect(el.length).toEqual(3);//@parm is initial shape count. After discard total shape count should not be increased.
                });
            });
    */
         it("Should be able to save user task ", () => {
                //Act
                svgElementsPage.navAddTaskButton(2).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(1).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                browser.driver.sleep(5000);
                svgElementsPage.getSaveButton.click();
                //Assert
                expect(svgElementsPage.getSaveButtonDisable.isDisplayed()).toBeTruthy();//should disable save button
                //TODO
               // element(By.css(".ng-binding.btn.button-branded-action.button-branded-warning")).click();
                browser.driver.sleep(1000);
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body lebel array is : " + + el.length);
                    console.log("Length of body lebel array is : " + el.length);
                 expect(el.length).toEqual(5);//@parm is modifided shape count. After Save total shape count should be increased.
                });
            });
    
    
            it("Should be able to click '+' to add user task ", () => {
            //Act
                browser.driver.sleep(1000);
                svgElementsPage.navAddTaskButton(4).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(1).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                //Assert
                browser.driver.sleep(1000);
                svgElementsPage.getLabelBody.then((el) => {
                        logger.info("Length of body lebel array is : " + + el.length);
                        console.log("Length of body lebel array is : " + el.length);
                expect(el.length).toEqual(7);//@parm is modifided shape count. After add total shape count should be increased.
                });    
            });
            
    
            it("Should be able to click '+' to add user decision point ", () => {
                //Act
                browser.driver.sleep(1000);
                svgElementsPage.navAddTaskButton(6).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(4).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                //Assert
                browser.driver.sleep(1000);
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body lebel array is : " + + el.length);
                    console.log("Length of body lebel array is : " + el.length);
                expect(el.length).toEqual(12);//@parm is modifided shape count(this also include condition body). After Save total shape count should be increased.
                });        
            });
     
            it("Should be able to click '+' to add system decision point ", () => {
                //Act
                browser.driver.sleep(1000);
                svgElementsPage.navAddTaskButton(2).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(0).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                //Assert
                browser.driver.sleep(1000);
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body lebel array is : " + + el.length);
                    console.log("Length of body lebel array is : " + el.length);
                expect(el.length).toEqual(16);//@parm is modifided shape count(this also include condition body). After Save total shape count should be increased.
                });
                browser.driver.sleep(9000);
            });
    
         });//end of test suite
        
    });

    //TODO: overool wait.until need to be refactor