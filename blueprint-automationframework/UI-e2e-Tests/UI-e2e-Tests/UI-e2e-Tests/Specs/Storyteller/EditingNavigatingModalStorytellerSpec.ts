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
var svgElementsPage;

describe("EditingNavigatingModalStoryteller",
    () => {
        beforeAll(() => {
            //Arrange
            svgElementsPage = new Page();
        });

      
        it("Should be able to toggle to user system process", () => {
           
            //expect(svgElementsPage.getStorytellerToggleTextForBusniessProcess.getText()).toBe('Business Process');
            //Act
           if (svgElementsPage.getStorytellerTogglecheckBox.isSelected()) {
               svgElementsPage.getStorytellerTogglecheckBox.click();  
            }
            //Assert
           expect(svgElementsPage.getStorytellerToggleTextForUserSystemProcess.getText()).toBe('User-System Process');
        
        });

        it("Should be able to publish the artifact", () => {
            //Act
            svgElementsPage.getPublishArtifact.click();
           // Assert
            expect(svgElementsPage.getPublishArtifactSucessMessage.getText()).toBe("The Process has been published.");
            //browser.driver.sleep(5000);
        });

        it("Should be able to edit system precondition shape header", () => {
            //Act
            svgElementsPage.editHeader(0, "s1");
            svgElementsPage.verifyHeaderName(0).then((t) => { });
            // Assert
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
            svgElementsPage.editBody(1, "Sys_T2B");
            svgElementsPage.verifyBodyText(1).then((t) => { });
            // Assert
            expect(svgElementsPage.verifyBodyText(1)).toBe("Sys_T2B");
           
        });


        it("Should be able to edit user task shape body", () => {
            //Act
            svgElementsPage.editBodyForUserTask(3, "User_T1B");
            svgElementsPage.verifyUserTaskBodyText(3).then((t) => { });
            //Assert
            expect(svgElementsPage.verifyUserTaskBodyText(3)).toBe("User_T1B");
            
        });

        it("Should be able to navigate to info Panel when click a user task", () => {
            //Act
           svgElementsPage.findElementAndSelect(2).then((el) => {
                el.click();
           }); 
           //Assert
            svgElementsPage.findFooterAndInfoIcon(19);
           
        });

        
        it("Should be able to navigate Properties tab", ()=> {
            //Act
            svgElementsPage.panelProperties.click();
            //Assert
            expect(svgElementsPage.panelProperties.getText()).toEqual(['Properties']);
            
        });

        it("Should be able to navigate Discussion tab", () => {
         
            //Act
            svgElementsPage.panelDiscussions.click();
            //Assert
            expect(svgElementsPage.panelDiscussions.getText()).toEqual(['Discussions']);
           
        });

        it("Should be able to post comment", () => {
            //Act
            svgElementsPage.postComment("This test case  need to be updated");
            svgElementsPage.panelDiscussionPostButton.click();
            //Assert
            expect(svgElementsPage.getPostCommentText.getText()).toBe("This test case  need to be updated");
             browser.driver.sleep(1000);
        });

        it("Should be able to navigate Files tab", () => {
            //Act
            svgElementsPage.panelFiles.click();
            //Assert
            expect(svgElementsPage.panelFiles.getText()).toEqual(['Files']);
          
        });

        it("Should be able to navigate Relationships tab", () => {
            //Act
            svgElementsPage.panelRelationships.click();
            //Assert
            expect(svgElementsPage.panelRelationships.getText()).toEqual(['Relationships']);
            
        });

        it("Should be able to navigate History tab", () => {
            //Act
            svgElementsPage.panelHistory.click();
            //Assert
            expect(svgElementsPage.panelHistory.getText()).toEqual(['History']);
        });

        it("Should be able to close info Panel",() => {
            //Act
            svgElementsPage.panelCloseButton.click();
         
            //TO DO Assert
        });


    })
    