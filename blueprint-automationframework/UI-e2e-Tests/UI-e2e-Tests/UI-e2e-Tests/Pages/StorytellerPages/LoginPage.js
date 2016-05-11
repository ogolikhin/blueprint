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
    LoginPage.loginField = element(By.id('loginField'));
    LoginPage.passwordField = element(By.id('passwordField'));
    LoginPage.loginButton = element(By.id('loginButton'));
    return LoginPage;
})();
module.exports = LoginPage;
//# sourceMappingURL=LoginPage.js.map