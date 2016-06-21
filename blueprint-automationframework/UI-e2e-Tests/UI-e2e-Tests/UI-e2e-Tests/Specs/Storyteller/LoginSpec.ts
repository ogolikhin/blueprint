
/**
 * This spec file will contain tests on  login page of storyteller 
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * last modified by:
 * Last modified on:
 */
import CreateArtifact = require("../../Model/CreateArtifacts");
import Page = require("../../Pages/StorytellerPages/LoginPage");
var createArtifact: CreateArtifact;
var OR = require('../../Locator/StorytellerLocator.json');
var loginPage;

describe("LoginPage- Storyteller",
    () => {
        beforeAll(() => {
            // Arrange
            createArtifact = new CreateArtifact();
            var ID = createArtifact.createArt();
            var site = OR.mockData.siteUrl + ID;
            
            browser.get(site);
            loginPage = new Page();
           
        });

  
        it("Should be able to login ", () => {
            //Act
            loginPage.login(OR.locators.storyteller.testdata.TName, OR.locators.storyteller.testdata.lPass);
            
            //Act
            loginPage.sessionDialofBox()
                .then((presence) => {
                    console.log("Session Dialog Box appears: " + presence);
                    if (presence) {
                       // Assert
                            expect(loginPage.getSessionDialofBoxWarningMessage()).toBe("This user is already logged into Blueprint in another browser/session. \n" + " Do you want to override the previous session?");
                            loginPage.sessionDialogBoxYesButton.click();
                            //browser.driver.sleep(1000);
                        } else {
                            console.log("Session Dialog Box appears: " + presence);
                        }
                });
            //Assert user name display
            expect(loginPage.getdisplayNameFinder.getText()).toBe("Default Instance Admin");

        });

    })
    