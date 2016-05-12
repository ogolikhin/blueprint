import svgElementsPage = require("../../Pages/StorytellerPages/SvgElementsPage");
import createArtifact = require("../../Model/CreateArtifacts");
//var json = require("./json/OR.json");
var OR = require('../../Json/OR.json');

describe("EditingNavigatingModalStoryteller",
    () => {
       // browser.driver.findElement(By.css('.nova-switch-label')).click();
        it("enter user name",() => {
            expect(browser.element(By.css('.nova-switch-outer-text.nova-switch-unchecked-label.ng-binding')).getText()).toEqual('Business Process');
            if ((element(by.css('.nova-switch')).isSelected())) {
                element(by.css('.nova-switch')).click();
            }
            
            // browser.driver.sleep(5000);
            // svgElementsPage.editHeader(0, "user1");
             browser.driver.sleep(5000);
         });

        it('Verify user can publish the artifact', () => {
            browser.driver.sleep(5000);
            browser.element(By.css('.fonticon-upload-cloud')).click();
            //SvgElementCounter.findLabelAndSelect(1);

            // SvgTitleForSystemtask.findSystemTitle(0, "ST1");
            browser.driver.sleep(5000);
            //SvgPanel.findPanel();


        });

        it('Verify user task Panel Icon', () => {
            browser.driver.sleep(5000);
            //  browser.element(By.id('label-H38')).click();
            svgElementsPage.findElementAndSelect(1);

            // SvgTitleForSystemtask.findSystemTitle(0, "ST1");
            browser.driver.sleep(5000);
           // SvgPanel.findPanel();
           

        });

        it('Verify infomation Icon is clickable', () => {
            browser.driver.sleep(5000);
            //  browser.element(By.id('label-H38')).click();
            svgElementsPage.findFooterAndInfoIcon();

            // SvgTitleForSystemtask.findSystemTitle(0, "ST1");
            browser.driver.sleep(5000);
            

        });
        it('Verify user task Panel has Properties tab', ()=> {
            browser.driver.sleep(5000);
            svgElementsPage.verifyPanelProperties();
            browser.driver.sleep(5000);
        });

        it('Verify user task Panel has Discussion tab', () => {
            browser.driver.sleep(5000);
            svgElementsPage.verifyPanelDiscussion();
            browser.driver.sleep(5000);
        });


        it('Verify post comment', () => {
            browser.driver.sleep(5000);

            svgElementsPage.verifyPostComment();;
            browser.driver.sleep(5000);
        });

        it('Verify user task Panel has Files tab', () => {
            browser.driver.sleep(5000);
            svgElementsPage.verifyPanelFiles();
            browser.driver.sleep(5000);
        });

        it('Verify user task Panel has Relationships tab', () => {
            browser.driver.sleep(5000);
            svgElementsPage.verifyPanelRelationships();;
            browser.driver.sleep(5000);
        });

        it('Verify user task Panel has History tab', () => {
            browser.driver.sleep(5000);
            svgElementsPage.verifyPanelHistory();
            browser.driver.sleep(5000);
        });

        it('Verify user task Panel has CloseButton tab',
        () => {
            browser.driver.sleep(5000);
            svgElementsPage.verifyPanelCloseButton();
            browser.driver.sleep(5000);
        });


    })
    