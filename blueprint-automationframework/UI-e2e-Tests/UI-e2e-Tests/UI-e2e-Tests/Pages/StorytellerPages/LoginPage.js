//import Promise = protractor.promise.Promise;
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
        expect(LoginPage.sessionDialogBox.isPresent()).toBeFalsy();
        if (LoginPage.sessionDialogBox.isPresent()) {
            LoginPage.sessionDialogBoxYesButton.click();
        }
    };
    LoginPage.loginField = element(By.id('loginField'));
    LoginPage.passwordField = element(By.id('passwordField'));
    LoginPage.loginButton = element(By.id('loginButton'));
    LoginPage.sessionDialogBox = element(By.css('.new-line.ng-binding'));
    // private static sessionDialogBoxYesButton = element(By.css('.btn.action-.btn.ng-binding'));
    LoginPage.sessionDialogBoxYesButton = element(By.buttonText('Yes'));
    return LoginPage;
})();
module.exports = LoginPage;
//# sourceMappingURL=LoginPage.js.map