var OR = require('../../Json/OR.json');
var LoginPage = (function () {
    function LoginPage() {
    }
    LoginPage.expect = function () {
        expect(LoginPage.loginField.isPresent()).toBeTruthy();
        expect(LoginPage.passwordField.isPresent()).toBeTruthy();
        return new LoginPage();
    };
    LoginPage.login = function (login, password) {
        LoginPage.loginField.sendKeys(login);
        LoginPage.passwordField.sendKeys(password);
        LoginPage.loginButton.click();
    };
    LoginPage.prototype.login = function (login, password, override) {
        LoginPage.loginField.sendKeys(login);
        LoginPage.passwordField.sendKeys(password);
        LoginPage.loginButton.click();
    };
    LoginPage.sessionDialofBox = function () {
        // expect(LoginPage.sessionDialogBox.isPresent()).toBeFalsy();
        LoginPage.sessionDialogBox.isPresent()
            .then(function (p) {
            if (p === true) {
                LoginPage.sessionDialogBoxYesButton.click();
            }
        });
    };
    // return promise
    LoginPage.prototype.sessionDialofBox1 = function () {
        return LoginPage.sessionDialogBox.isPresent().then(function (p) {
            if (p === true)
                return true;
            else
                return false;
        });
    };
    LoginPage.loginField = browser.element(By.id('loginField'));
    LoginPage.passwordField = browser.element(By.id('passwordField'));
    LoginPage.loginButton = browser.element(By.id('loginButton'));
    LoginPage.sessionDialogBox = element(By.css('.new-line.ng-binding'));
    // private static sessionDialogBoxYesButton = element(By.css('.btn.action-.btn.ng-binding'));
    LoginPage.sessionDialogBoxYesButton = element(By.buttonText('Yes'));
    return LoginPage;
})();
module.exports = LoginPage;
//# sourceMappingURL=LoginPage.js.map