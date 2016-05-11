var svgElementspage = require("../../Pages/StorytellerPages/SvgElementsPage");
//var json = require("./json/OR.json");
var OR = require('../../Json/OR.json');
describe("EditingNavigatingModalStoryteller", function () {
    it("enter user name", function () {
        browser.driver.sleep(5000);
        var q;
        svgElementspage.editUsertaskHeader(0, "user1");
        browser.driver.sleep(5000);
    });
});
//# sourceMappingURL=EditingNavigatingModalStoryteller.js.map