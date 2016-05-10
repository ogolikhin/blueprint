//import Promise = protractor.promise.Promise;
var OR = require('../../Json/OR.json');
var LoginPageStoryteller = (function () {
    function LoginPageStoryteller() {
    }
    //browser.element(By.id('loginField')).sendKeys('admin');
    // browser.element(By.id('passwordField')).sendKeys('changeme');
    // browser.element(By.id('loginButton')).click();
    // browser.driver.sleep(5000);
    LoginPageStoryteller.expect = function () {
        //IndexPage.expect(); //don't think we need this when expecting login page
        expect(LoginPageStoryteller.loginField.isPresent()).toBeTruthy();
        expect(LoginPageStoryteller.passwordField.isPresent()).toBeTruthy();
        return new LoginPageStoryteller();
    };
    LoginPageStoryteller.login = function (login, password) {
        LoginPageStoryteller.loginField.sendKeys(login);
        LoginPageStoryteller.passwordField.sendKeys(password);
        LoginPageStoryteller.loginButton.click();
    };
    LoginPageStoryteller.loginField = element(By.id('loginField'));
    LoginPageStoryteller.passwordField = element(By.id('passwordField'));
    LoginPageStoryteller.loginButton = element(By.id('loginButton'));
    return LoginPageStoryteller;
})();
module.exports = LoginPageStoryteller;
//# sourceMappingURL=LoginPageStoryteller.js.map