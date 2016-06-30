
/**
 * This spec file will contain tests on  login page of storyteller 
 * Assumption: Project and user need to be predefined.
 */
import Artifact = require("../../Model/CreateArtifacts");
import Page = require("../../Pages/StorytellerPages/LoginPage");

let mockData = require('../../CustomConfig/MockData.json');
var artifact: Artifact;
var storytellerLocator = require('../../Locator/StorytellerLocator.json');
var loginPage;

describe("LoginPage- Storyteller",
    () => {
        beforeAll(() => {
            // Arrange
            artifact = new Artifact();
            var ID = artifact.createArtifact();
            var site = mockData.serverArtifactsInfo.siteUrl + ID;
            
            browser.get(site);
            loginPage = new Page();
           
        });

  
        it("Should be able to login ", () => {
            //Act
            loginPage.login(storytellerLocator.locators.storyteller.testdata.TName, storytellerLocator.locators.storyteller.testdata.lPass);
            
            //Act
            loginPage.isSessionDialogBox()
                .then((presence) => {
                    console.log("Session Dialog Box appears: " + presence);
                    if (presence) {
                       // Assert
                        expect(loginPage.getSessionDialogBoxWarningMessageText()).toBe("This user is already logged into Blueprint in another browser/session. \n" + " Do you want to override the previous session?");
                            loginPage.sessionDialogBoxYesButton.click();

                        } else {
                            console.log("Session Dialog Box appears: " + presence);
                        }
                });
            //Assert user name display
            expect(loginPage.getdisplayNameFinder.getText()).toBe("Default Instance Admin");

        });

    })
    