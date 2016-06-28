/**
 * This spec file will contain All major locator and action that can be performed on storyteller svg(graphic)
 * Assumption: Project and user need to be predefined.
 */
import Page = require("../../Pages/StorytellerPages/SvgElementsPage");
import Artifact = require("../../Model/CreateArtifacts");
import ArrayListPresenceOfAll = require("../../Utility/ArrayListPresenceOfAll");
let mockData = require('../../CustomConfig/MockData.json');
var artifact: Artifact;
var storytellerLocator = require('../../Locator/StorytellerLocator.json');
var customConfigStoryteller = require("../../CustomConfig/CustomConfigStoryteller.json");
var logger = require('winston');
var svgElementsPage: Page;

describe("Storyteller end to end test", () => {
    beforeAll(() => {
        //Arrange-global
        artifact = new Artifact();
        svgElementsPage = new Page();

    });

    afterAll(() => {
        //cleanup
        browser.clearMockModules['svgElementsPage'];
        browser.clearMockModules['createArtifact'];
    });

    describe("Editing-Navigating- Shapes and Info panel of Storyteller", () => {
        // logger.info("=======Start Editing-Navigating- Shapes and Info panel of Storyteller=======");
        

        it("Should be able to toggle to user system process", () => {
            //Act           
            if (svgElementsPage.getStorytellerTogglecheckBox.isSelected()) {
                svgElementsPage.getStorytellerTogglecheckBox.click();
            }
            //Assert toggle's text
            expect(svgElementsPage.getStorytellerToggleTextForUserSystemProcess.getText()).toBe('User-System Process');

        });

        it("Should be able to publish the artifact", () => {
            //Act
            svgElementsPage.getPublishArtifact.click();
            
            // Assert confirmation message
            expect(svgElementsPage.getPublishArtifactSucessMessage.getText()).toBe("The Process has been published.");

        });
        it("Should be able generate user story", () => {
            //Act
            svgElementsPage.getGenerateUserStoriesMenuButton.click();
            svgElementsPage.generateUserStoiesDropDownMenu(1).then((el) => { el.click(); });//@parm 1 for generate All, 0 for generate from user task
            //Assert sucess message
            //TODO need to find the elment for sucess message
            //expect(svgElementsPage.getPublishArtifactSucessMessage.getText()).toBe("The Process has been published.");
            //expect(element(by.css("[data-ng-repeat=\"m in messages | filter:{ messageType: 2 } as results\"]"))).toBe("The Process has been published.");
            
            
          });
          it("Should be able to edit system precondition shape header", () => {
            //Act
            svgElementsPage.editHeader(customConfigStoryteller.storyteller.systemPreconditionShapeIndex, "s1");//@parm shape index and header text
            svgElementsPage.verifyHeaderName(customConfigStoryteller.storyteller.systemPreconditionShapeIndex).then((t) => { });
            //Assert shape header text
            expect(svgElementsPage.verifyHeaderName(customConfigStoryteller.storyteller.systemPreconditionShapeIndex)).toBe("s1");
                        
           });
                 
            it("Should be able to edit user task shape header", () => {
                //Act
                svgElementsPage.editHeader(customConfigStoryteller.storyteller.userTaskShapeIndex, "User_T1");//@parm shape index and header text
                svgElementsPage.verifyHeaderName(customConfigStoryteller.storyteller.userTaskShapeIndex).then((t) => { });
                // Assert shape header text
                expect(svgElementsPage.verifyHeaderName(customConfigStoryteller.storyteller.userTaskShapeIndex)).toBe("User_T1");
            });
              
            it("Should be able to edit system task shape header", () => {
                //Act
                svgElementsPage.editHeader(customConfigStoryteller.storyteller.systemTaskShapeIndex, "Sys_T1");//@parm shape index and header text
                svgElementsPage.verifyHeaderName(customConfigStoryteller.storyteller.systemTaskShapeIndex).then((t) => { });
                // Assert shape header text
                expect(svgElementsPage.verifyHeaderName(customConfigStoryteller.storyteller.systemTaskShapeIndex)).toBe("Sys_T1");
            });
                    
            it("Should be able to edit system precondition shape body", () => {
                //Act
                svgElementsPage.editBody(customConfigStoryteller.storyteller.systemPreconditionShapeIndex, "Sys_P0B");//@parm shape index and body text
                svgElementsPage.verifyBodyText(customConfigStoryteller.storyteller.systemPreconditionShapeIndex).then((t) => { });
                // Assert shape body text
                expect(svgElementsPage.verifyBodyText(customConfigStoryteller.storyteller.systemPreconditionShapeIndex)).toBe("Sys_P0B");
                       
            });
            it("Should be able to edit user task shape body", () => {
                //Act
                svgElementsPage.editBody(customConfigStoryteller.storyteller.userTaskShapeIndex, "User_T1B");//@parm shape index and body text
                svgElementsPage.verifyBodyText(customConfigStoryteller.storyteller.userTaskShapeIndex).then((t) => { });
                // Assert shape body text
                expect(svgElementsPage.verifyBodyText(customConfigStoryteller.storyteller.userTaskShapeIndex)).toBe("User_T1B");
                        
            });
             
            
            it("Should be able to edit system task shape body", () => {
                //Act
                svgElementsPage.editBody(customConfigStoryteller.storyteller.systemTaskShapeIndex, "Sys_T2B");//@parm shape index and body text
                svgElementsPage.verifyBodyText(customConfigStoryteller.storyteller.systemTaskShapeIndex).then((t) => { });
                //Assert shape body text
                expect(svgElementsPage.verifyBodyText(customConfigStoryteller.storyteller.systemTaskShapeIndex)).toBe("Sys_T2B");
                        
            });
                    
                    
            it("Should be able to navigate to info Panel when click a user task", () => {
                //Act
                svgElementsPage.findElementAndSelect(customConfigStoryteller.storyteller.shapeIndexForInfoIcon).then((el) => {
                browser.actions().click(el).perform();           
                }); 
                //Assert icon display and clickable
                svgElementsPage.findFooterAndInfoIcon(customConfigStoryteller.storyteller.infoIconIndexIndex);//@parm info icon index

             });
             
                  
            it("Should be able to navigate Properties tab", ()=> {
                //Act
                svgElementsPage.getPanelProperties.click();
                //Assert tab text
                expect(svgElementsPage.getPanelProperties.getText()).toEqual(['Properties']);
                        
            });
            
            it("Should be able to navigate Discussion tab", () => {
                     
                //Act
                svgElementsPage.getPanelDiscussions.click();
                //Assert tab text
                expect(svgElementsPage.getPanelDiscussions.getText()).toEqual(['Discussions']);
                       
            });
            
            it("Should be able to post comment", () => {
                //Act
                svgElementsPage.postComment("This test case  need to be updated");
                svgElementsPage.getPanelDiscussionPostButton.click();
                //Assert tab text
                expect(svgElementsPage.getPostCommentText.getText()).toBe("This test case  need to be updated");

            });
                   
            
            it("Should be able to navigate Files tab", () => {
                //Act
                svgElementsPage.getPanelFiles.click();
                //Assert tab text
                expect(svgElementsPage.getPanelFiles.getText()).toEqual(['Files']);
                      
            });
            
            it("Should be able to navigate Relationships tab", () => {
                //Act
                ArrayListPresenceOfAll.presenceOfAll(svgElementsPage.getPanelRelationships);
                svgElementsPage.getPanelRelationships.click();
                //Assert tab text
                expect(svgElementsPage.getPanelRelationships.getText()).toEqual(['Relationships']);
                        
            });
            
            it("Should be able to navigate History tab", () => {
                //Act
                ArrayListPresenceOfAll.presenceOfAll(svgElementsPage.getPanelHistory);
                svgElementsPage.getPanelHistory.click();
                //Assert tab text
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
                svgElementsPage.navFooterEditDetailButton(customConfigStoryteller.storyteller.footerEditDetailButtonIndex).then((el) => { el.click(); });//@parm index 
                //Assert modal header text
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalTitle.getWebElement()), 9000).then(() => {
                    expect(svgElementsPage.getFooterModalTitle.getText()).toBe("Sys_T2B");
                });

                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalCancelButton.getWebElement()), 30000).then(() => {
                    svgElementsPage.getFooterModalCancelButton.click();
                });       
            });
                   
            it("Should be able to open Add comment modal at footer",() => {
                //Act
                svgElementsPage.navFooterAddCommentButton(customConfigStoryteller.storyteller.footerAddCommentButtonIndex).then((el) => { el.click(); });//@parm index 
                //Assert modal header text
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getActiveTabInModal.getWebElement()), 9000).then(() => {
                    expect(svgElementsPage.getActiveTabInModal.getText()).toBe("Discussions");
                });

                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getPanelCloseButton.getWebElement()), 9000).then(() => {
                    svgElementsPage.getPanelCloseButton.click();
                });

             });
             
            it("Should be able to open review traces modal at footer",() => {
                //Act
                svgElementsPage.navFooterReviewTracesButton(customConfigStoryteller.storyteller.footerReviewTracesButtonIndex).then((el) => { el.click(); });//@parm index 
                //Assert modal header text
                expect(svgElementsPage.getActiveTabInModal.getText()).toBe("Relationships");
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getPanelCloseButton.getWebElement()), 9000).then(() => {
                    svgElementsPage.getPanelCloseButton.click();
                });
               
             });
              
            it("Should be able to open add Images-Mockups-Screenshots modal at footer",() => {
                //Act
                svgElementsPage.navFooterAddImageMockUpsScreenshotsButton(customConfigStoryteller.storyteller.footerAddImageMockUpsScreenshotsButtonIndex).then((el) => { el.click(); });//@parm index 
                //Assert modal header text

                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalTitle.getWebElement()), 9000).then(() => {
                    expect(svgElementsPage.getFooterModalTitle.getText()).toBe("Sys_T2B");
                });

                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalCancelButton.getWebElement()), 30000).then(() => {
                    svgElementsPage.getFooterModalCancelButton.click();
                });

             });
                 
            it("Should be able to open view user stories modal at footer ", () => {
                //Act
                svgElementsPage.navFooterViewUserStoriesButton(customConfigStoryteller.storyteller.footerViewUserStoriesButtonIndex).then((el) => { el.click(); });//@parm index 
                //Assert modal header text
                svgElementsPage.getViewUserStoriesGherkinTitle.then((el) => {
                    logger.info("Length of Gherkin title array is : " + + el.length);
                    console.log("Length of Gherkin title array is : " + el.length);
                    expect(el[0].getText()).toBe("Given");
                    expect(el[1].getText()).toBe("When");
                    expect(el[2].getText()).toBe("Then");
                });
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getFooterModalCancelButton.getWebElement()), 9000).then(() => {
                    svgElementsPage.getFooterModalCancelButton.click();
                });

                
              });
                
          });// end of test suite
            
          
    describe("Include Artifacts and navigate to the attached artifacts thru Bread curmbs", () => {
        logger.info("======= START Include Artifacts and navigate to the attached artifacts thru Bread curmbs =======");
        
            it("Should be able  include Artifacts at Edit Detail modal at footer", () => {
                //Arrange 
                artifact.publishArtifact();//Creating new artifacts and publish it to ensure artifact is avaiable to be include
                //Act
                //Open edit detail modal
                svgElementsPage.navFooterEditDetailButton(customConfigStoryteller.storyteller.footerEditDetailButtonIndex).then((el) => { el.click(); });//@parm edit detail button index
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getShowMoreButtonAtModal.getWebElement()), 9000).then(() => {
                    svgElementsPage.getShowMoreButtonAtModal.click();
                });
                
                    svgElementsPage.getIncludeButtonAtModal.then((el) => {
                        console.log("Total button at Edit detail modal is : " + el.length);
                        el[customConfigStoryteller.storyteller.includeTabAtSystemTaskIndex].click();//@parm 0 for addtional info tab and 1 for include tab
                    
                });
                //Assert search result display
                expect(svgElementsPage.artifactsSearchResultCount("fro")).toBeGreaterThan(0);
        
                //Act
                ArrayListPresenceOfAll.presenceOfAll(svgElementsPage.getIncludeArtifactDropdownList);
                svgElementsPage.getIncludeArtifactDropdownList.then((el) => { 
                    el[customConfigStoryteller.storyteller.artifactSearchItemAtSystemTaskIndex].click(); //@parm search item index
                });
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getModalOKButton.getWebElement()), 9000).then(() => {

                    svgElementsPage.getModalOKButton.click();
                });

                        
                //Assert include-process button become active
                ArrayListPresenceOfAll.presenceOfAll(svgElementsPage.getFooterAddIncludesButton);
                svgElementsPage.getFooterAddIncludesButton.then((el) => {
                    el[customConfigStoryteller.storyteller.footerAddIncludesButtonIndex].getOuterHtml().then((imageTag) => { //@parm index 
                        var link = imageTag.match(/xlink:href="(.*?)"/);
                        var linkUrl = mockData.serverArtifactsInfo.baseURL + "/Areas/Web/Style/images/Storyteller/include-active.svg";
                        console.log("include-active svg link : " +link[1]);
                        expect(link[customConfigStoryteller.storyteller.includeActiveSvgLinkIndex]).toBe(linkUrl);//@parm index of include-active.svg
        
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
                 //Assert user story link available 
                 expect(svgElementsPage.getuserStoryLinkAtReviewTraceTab.isDisplayed()).toBeTruthy();
                 svgElementsPage.getuserStoryLinkAtReviewTraceTab.isDisplayed().then((el) => { console.log(el) });
                 svgElementsPage.getuserStoryLinkAtReviewTraceTab.click();
                 svgElementsPage.getPanelFiles.click();
                 svgElementsPage.getuserStoryLinkAtFileTab.click();
                 browser.driver.wait(() => {
    
                     return fs.existsSync(filename);
                 }, 30000).then(() => {
    
                     console.log(fs.existsSync(filename) ? "found" : "Not found");
                     //Assert file was downloaded and file exist
                     expect(fs.existsSync(filename)).toBeTruthy();
                 });
    
                 //fs.unlinkSync(filename);
                 svgElementsPage.getPanelCloseButton.click();
                 //TODO This function is temp solution. Need to refactor
             });
         }); //end of test suite
    
        describe("Add-Delete-Discard-Save user task and add decision Point", () => {
            logger.info("======= Add-Delete-Discard-Save user task and add decision Point =======");
    
            it("Should be able to delete user task ", () => {
                //Act
                svgElementsPage.navAddTaskButton(customConfigStoryteller.storyteller.addTaskButtonIndex).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(customConfigStoryteller.storyteller.addUserTaskItemsIndex).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getDeleteButton.getWebElement()), 1000).then(() => { svgElementsPage.getDeleteButton.click(); });

                //Assert delete confirm meesage 
                browser.wait(protractor.until.elementIsVisible(svgElementsPage.getWarningPopUP.getWebElement()), 5000).then(() => {
                    expect(svgElementsPage.getWarningPopUP.getText()).toBe("Please confirm the deletion of the selected user task.");
                });
                //expect(svgElementsPage.getWarningPopUP.getText()).toBe("Please confirm the deletion of the selected user task.");//A warning pop up windows display with following contents
                svgElementsPage.getWarningPopUpOKButton.click();
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body label  array is : " + + el.length);
                    console.log("Length of body label  array is: " + el.length);
                     //Assert shape count . count should Not be increased 
                 expect(el.length).toEqual(3);//@parm is initial shape count. After delete total shape count should not increase.
                });
           }); 
   
            it("Should be able discard user task ", () => {
                //Act
                svgElementsPage.navAddTaskButton(customConfigStoryteller.storyteller.addTaskButtonIndex).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(customConfigStoryteller.storyteller.addUserTaskItemsIndex).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                svgElementsPage.getDiscardButton.click();
                //Assert confirmation message
                expect(svgElementsPage.getWarningPopUP.getText()).toBe("After discarding your changes, each artifact is restored to its last published version.");//A warning pop up windows display with following contents
                svgElementsPage.getDiscardWarningPopUpOKButton.click();
                //TODO
                //expect(svgElementsPage.getPublishArtifactSucessMessage.getText()).toBe("Changes Were Discarded");//should displaying sucess message
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body lebel array is : " + + el.length);
                    console.log("Length of body lebel array is : " + el.length);
                    //Assert shape count . count should Not be increased
                 expect(el.length).toEqual(3);//@parm is initial shape count. After discard total shape count should not be increased.
                });
            });
    
         it("Should be able to save user task ", () => {
                //Act
             svgElementsPage.navAddTaskButton(customConfigStoryteller.storyteller.addTaskButtonIndex).then((el) => { el.click(); });//@parm index for '+' icon
             svgElementsPage.selectAddItem(customConfigStoryteller.storyteller.addUserTaskItemsIndex).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                svgElementsPage.getSaveButton.click();
                //Assert save button display
                expect(svgElementsPage.getSaveButtonDisable.isDisplayed()).toBeTruthy();//should disable save button
                //TODO
               // element(By.css(".ng-binding.btn.button-branded-action.button-branded-warning")).click();
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body lebel array is : " + + el.length);
                    console.log("Length of body lebel array is : " + el.length);
                    //Assert shape count. count should  be increased
                 expect(el.length).toEqual(5);//@parm is modifided shape count. After Save total shape count should be increased.
                });
            });
    
    
            it("Should be able to click '+' to add user task ", () => {
               //Act
                svgElementsPage.navAddTaskButton(customConfigStoryteller.storyteller.addTaskButtonAfterSaveUserTaskIndex).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(customConfigStoryteller.storyteller.addUserTaskItemsIndex).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                //Assert shape count. count should  be increased
                svgElementsPage.getLabelBody.then((el) => {
                        logger.info("Length of body lebel array is : " + + el.length);
                        console.log("Length of body lebel array is : " + el.length);
                expect(el.length).toEqual(7);//@parm is modifided shape count. After add total shape count should be increased.
                });    
            });
            
    
            it("Should be able to click '+' to add user decision point ", () => {
                //Act
                svgElementsPage.navAddTaskButton(customConfigStoryteller.storyteller.addTaskButtonAfterAddUserTaskIndex).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(customConfigStoryteller.storyteller.addUserDecisionPointItemsIndex).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                //Assert shape count. count should  be increased
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body lebel array is : " + + el.length);
                    console.log("Length of body lebel array is : " + el.length);
                expect(el.length).toEqual(12);//@parm is modifided shape count(this also include condition body). After Save total shape count should be increased.
                });        
            });
     
            it("Should be able to click '+' to add system decision point ", () => {
                //Act
                svgElementsPage.navAddTaskButton(customConfigStoryteller.storyteller.addTaskButtonIndex).then((el) => { el.click(); });//@parm index for '+' icon
                svgElementsPage.selectAddItem(customConfigStoryteller.storyteller.addSystemDecisionPointItemsIndex).then((el) => { el.click(); });//@parm if array has more than 1 elements,then 1 for add user task , 4 for add user decision point, else 0 for system decision
                //Assert shape count. count should  be increased
                svgElementsPage.getLabelBody.then((el) => {
                    logger.info("Length of body lebel array is : " + + el.length);
                    console.log("Length of body lebel array is : " + el.length);
                expect(el.length).toEqual(16);//@parm is modifided shape count(this also include condition body). After Save total shape count should be increased.
                });
                browser.driver.sleep(5000);//This is only for monitoring purpose whether test reach at the end
            });
    
         });//end of test suite
        
    });

    //TODO: overool wait.until need to be refactor