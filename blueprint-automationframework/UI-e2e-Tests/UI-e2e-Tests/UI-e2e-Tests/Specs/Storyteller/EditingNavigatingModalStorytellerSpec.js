/**
 * This spec file will contain All major locator and action that can be performed on storyteller svg(graphic)
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * last modified by:
 * Last modified on:
 */
var Page = require("../../Pages/StorytellerPages/SvgElementsPage");
var OR = require('../../Json/OR.json');
var svgElementsPage;
describe("EditingNavigatingModalStoryteller", function () {
    beforeAll(function () {
        //Arrange
        svgElementsPage = new Page();
    });
    it("Should be able to toggle to user system process", function () {
        //expect(svgElementsPage.getStorytellerToggleTextForBusniessProcess.getText()).toBe('Business Process');
        //Act
        if (svgElementsPage.getStorytellerTogglecheckBox.isSelected()) {
            svgElementsPage.getStorytellerTogglecheckBox.click();
        }
        //Assert
        expect(svgElementsPage.getStorytellerToggleTextForUserSystemProcess.getText()).toBe('User-System Process');
    });
    it("Should be able to publish the artifact", function () {
        //Act
        svgElementsPage.getPublishArtifact.click();
        // Assert
        expect(svgElementsPage.getPublishArtifactSucessMessage.getText()).toBe("The Process has been published.");
        //browser.driver.sleep(5000);
    });
    it("Should be able to edit system precondition shape header", function () {
        //Act
        svgElementsPage.editHeader(0, "s1");
        svgElementsPage.verifyHeaderName(0).then(function (t) { });
        //Assert
        expect(svgElementsPage.verifyHeaderName(0)).toBe("s1");
    });
    it("Should be able to edit user task shape header", function () {
        //Act
        svgElementsPage.editHeader(1, "User_T1");
        svgElementsPage.verifyHeaderName(1).then(function (t) { });
        // Assert
        expect(svgElementsPage.verifyHeaderName(1)).toBe("User_T1");
    });
    it("Should be able to edit system task shape header", function () {
        //Act
        svgElementsPage.editHeader(2, "Sys_T1");
        svgElementsPage.verifyHeaderName(2).then(function (t) { });
        // Assert
        expect(svgElementsPage.verifyHeaderName(2)).toBe("Sys_T1");
    });
    it("Should be able to edit system precondition shape body", function () {
        //Act
        svgElementsPage.editBody(0, "Sys_P0B");
        svgElementsPage.verifyBodyText(0).then(function (t) { });
        // Assert
        expect(svgElementsPage.verifyBodyText(0)).toBe("Sys_P0B");
    });
    it("Should be able to edit user task shape body", function () {
        //Act
        svgElementsPage.editBody(1, "User_T1B");
        svgElementsPage.verifyBodyText(1).then(function (t) { });
        // Assert 
        expect(svgElementsPage.verifyBodyText(1)).toBe("User_T1B");
    });
    it("Should be able to edit system task shape body", function () {
        //Act
        svgElementsPage.editBody(2, "Sys_T2B");
        svgElementsPage.verifyBodyText(2).then(function (t) { });
        //Assert
        expect(svgElementsPage.verifyBodyText(2)).toBe("Sys_T2B");
    });
    it("Should be able to navigate to info Panel when click a user task", function () {
        //Act
        svgElementsPage.findElementAndSelect(2).then(function (el) {
            el.click();
        });
        //Assert
        svgElementsPage.findFooterAndInfoIcon(19);
    });
    it("Should be able to navigate Properties tab", function () {
        //Act
        svgElementsPage.panelProperties.click();
        //Assert
        expect(svgElementsPage.panelProperties.getText()).toEqual(['Properties']);
    });
    it("Should be able to navigate Discussion tab", function () {
        //Act
        svgElementsPage.panelDiscussions.click();
        //Assert
        expect(svgElementsPage.panelDiscussions.getText()).toEqual(['Discussions']);
    });
    it("Should be able to post comment", function () {
        //Act
        svgElementsPage.postComment("This test case  need to be updated");
        svgElementsPage.panelDiscussionPostButton.click();
        //Assert
        expect(svgElementsPage.getPostCommentText.getText()).toBe("This test case  need to be updated");
        browser.driver.sleep(1000);
    });
    it("Should be able to navigate Files tab", function () {
        //Act
        svgElementsPage.panelFiles.click();
        //Assert
        expect(svgElementsPage.panelFiles.getText()).toEqual(['Files']);
    });
    it("Should be able to navigate Relationships tab", function () {
        //Act
        svgElementsPage.panelRelationships.click();
        //Assert
        expect(svgElementsPage.panelRelationships.getText()).toEqual(['Relationships']);
    });
    it("Should be able to navigate History tab", function () {
        //Act
        svgElementsPage.panelHistory.click();
        //Assert
        expect(svgElementsPage.panelHistory.getText()).toEqual(['History']);
    });
    it("Should be able to close info Panel", function () {
        //Act
        svgElementsPage.panelCloseButton.click();
        //TO DO Assert
    });
});
//# sourceMappingURL=EditingNavigatingModalStorytellerSpec.js.map