/**
 * This class file will contain all elements and action on element for storyteller login page
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * last modified by:
 * Last modified on:
 */
var OR = require('../../Json/OR.json');
var LoginPage = (function () {
    function LoginPage() {
        this.loginField = element(By.id(OR.locators.storyteller.loginPageStoryteller.loginField));
        this.passwordField = element(By.id(OR.locators.storyteller.loginPageStoryteller.passwordField));
        this.loginButton = element(By.id(OR.locators.storyteller.loginPageStoryteller.loginButton));
        this.sessionDialogBox = element(By.css(OR.locators.storyteller.loginPageStoryteller.sessionDialogBox));
        this.sessionDialogBoxYesButton = element(By.buttonText(OR.locators.storyteller.loginPageStoryteller.sessionDialogBoxYesButton));
        this.sessionDialogBoxWarningMessage = element(By.css(OR.locators.storyteller.loginPageStoryteller.sessionDialogBoxWarningMessage));
        this.displayNameFinder = element(by.exactBinding(OR.locators.storyteller.loginPageStoryteller.displayNameFinder));
    }
    Object.defineProperty(LoginPage.prototype, "getLoginField", {
        get: function () { return this.loginField; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(LoginPage.prototype, "getPasswordField", {
        get: function () { return this.passwordField; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(LoginPage.prototype, "getLoginButton", {
        get: function () { return this.loginButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(LoginPage.prototype, "getSessionDialogBox", {
        get: function () { return this.sessionDialogBox; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(LoginPage.prototype, "getSessionDialogBoxYesButton", {
        get: function () { return this.sessionDialogBoxYesButton; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(LoginPage.prototype, "getSessionDialogBoxWarningMessage", {
        get: function () { return this.sessionDialogBoxWarningMessage; },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(LoginPage.prototype, "getdisplayNameFinder", {
        get: function () { return this.displayNameFinder; },
        enumerable: true,
        configurable: true
    });
    //login function 
    LoginPage.prototype.login = function (login, password) {
        this.loginField.sendKeys(login);
        this.passwordField.sendKeys(password);
        this.loginButton.click();
    };
    //function to verify sessionDialogBox presence or not- return promise
    LoginPage.prototype.sessionDialofBox = function () {
        return this.sessionDialogBox.isPresent()
            .then(function (present) {
            if (present) {
                return true;
            }
            else {
                return false;
            }
        });
    };
    //function to get warning message- return promise
    LoginPage.prototype.getSessionDialofBoxWarningMessage = function () {
        return this.sessionDialogBoxWarningMessage.getText();
    };
    return LoginPage;
})();
module.exports = LoginPage;
//# sourceMappingURL=LoginPage.js.map