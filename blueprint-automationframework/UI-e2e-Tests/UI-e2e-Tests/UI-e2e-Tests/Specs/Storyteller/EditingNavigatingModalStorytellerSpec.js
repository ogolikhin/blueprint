var svgElementsPage = require("../../Pages/StorytellerPages/SvgElementsPage");
//var json = require("./json/OR.json");
var OR = require('../../Json/OR.json');
describe("EditingNavigatingModalStoryteller", function () {
    // browser.driver.findElement(By.css('.nova-switch-label')).click();
    it("enter user name", function () {
        expect(browser.element(By.css('.nova-switch-outer-text.nova-switch-unchecked-label.ng-binding')).getText()).toEqual('Business Process');
        if ((element(by.css('.nova-switch')).isSelected())) {
            element(by.css('.nova-switch')).click();
        }
        // browser.driver.sleep(5000);
        // svgElementsPage.editHeader(0, "user1");
        browser.driver.sleep(5000);
    });
    it('Verify user can publish the artifact', function () {
        browser.driver.sleep(5000);
        browser.element(By.css('.fonticon-upload-cloud')).click();
        //SvgElementCounter.findLabelAndSelect(1);
        // SvgTitleForSystemtask.findSystemTitle(0, "ST1");
        browser.driver.sleep(5000);
        //SvgPanel.findPanel();
    });
    it('Verify user task Panel Icon', function () {
        browser.driver.sleep(5000);
        //  browser.element(By.id('label-H38')).click();
        svgElementsPage.findElementAndSelect(1);
        // SvgTitleForSystemtask.findSystemTitle(0, "ST1");
        browser.driver.sleep(5000);
        // SvgPanel.findPanel();
    });
    it('Verify infomation Icon is clickable', function () {
        browser.driver.sleep(5000);
        //  browser.element(By.id('label-H38')).click();
        svgElementsPage.findFooterAndInfoIcon();
        // SvgTitleForSystemtask.findSystemTitle(0, "ST1");
        browser.driver.sleep(5000);
    });
    it('Verify user task Panel has Properties tab', function () {
        browser.driver.sleep(5000);
        svgElementsPage.verifyPanelProperties();
        browser.driver.sleep(5000);
    });
    it('Verify user task Panel has Discussion tab', function () {
        browser.driver.sleep(5000);
        svgElementsPage.verifyPanelDiscussion();
        browser.driver.sleep(5000);
    });
    it('Verify post comment', function () {
        browser.driver.sleep(5000);
        svgElementsPage.verifyPostComment();
        ;
        browser.driver.sleep(5000);
    });
    it('Verify user task Panel has Files tab', function () {
        browser.driver.sleep(5000);
        svgElementsPage.verifyPanelFiles();
        browser.driver.sleep(5000);
    });
    it('Verify user task Panel has Relationships tab', function () {
        browser.driver.sleep(5000);
        svgElementsPage.verifyPanelRelationships();
        ;
        browser.driver.sleep(5000);
    });
    it('Verify user task Panel has History tab', function () {
        browser.driver.sleep(5000);
        svgElementsPage.verifyPanelHistory();
        browser.driver.sleep(5000);
    });
    it('Verify user task Panel has CloseButton tab', function () {
        browser.driver.sleep(5000);
        svgElementsPage.verifyPanelCloseButton();
        browser.driver.sleep(5000);
    });
});
//# sourceMappingURL=EditingNavigatingModalStorytellerSpec.js.map