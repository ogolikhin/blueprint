import loginPage = require("../../Pages/StorytellerPages/LoginPage");
import svgElementspage = require("../../Pages/StorytellerPages/SvgElementsPage");
import createArtifact = require("../../Model/CreateArtifacts");
//var json = require("./json/OR.json");
var OR = require('../../Json/OR.json');
describe("LoginPage",
    () => {

        var ID = createArtifact.createArt();
        beforeEach(
            () => {
                //Arrange
                // ID = 
                browser.ignoreSynchronization = true;
            });

        var site = 'http://52.202.237.164/Web/#/Storyteller/' + ID;
        browser.get(site);


        it("enter user name",
            () => {
                console.log("OR TEST ");
                console.log("OR TEST " + OR.locators.storyteller.testdata.TName);
                browser.driver.sleep(5000);
               // loginPage.login(OR.locators.storyteller.testdata.TName);
                loginPage.login(OR.locators.storyteller.testdata.TName, OR.locators.storyteller.testdata.lPass);
                browser.driver.sleep(5000);
                var q;
                //svgElementspage.editHeader(0, "user1");
                browser.driver.sleep(5000);
            });

    })
    