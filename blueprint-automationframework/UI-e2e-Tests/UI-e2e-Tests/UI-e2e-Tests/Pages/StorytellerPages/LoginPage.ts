/**
 * This class file will contain all elements and action on element for storyteller login page 
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * last modified by:
 * Last modified on:
 */


import Promise = protractor.promise.Promise;
import ElementFinder = protractor.ElementFinder;
import WebElementPromise = protractor.WebElementPromise;
var OR = require('../../Locator/StorytellerLocator.json');
class LoginPage {
 
    private loginField: ElementFinder;
    private passwordField: ElementFinder;
    private loginButton: ElementFinder;
    private sessionDialogBox: ElementFinder;
    private sessionDialogBoxYesButton: ElementFinder;
    private sessionDialogBoxWarningMessage: ElementFinder;
    private displayNameFinder: ElementFinder;

    constructor() {
        this.loginField = element(By.id(OR.locators.storyteller.loginPageStoryteller.loginField));
        this.passwordField = element(By.id(OR.locators.storyteller.loginPageStoryteller.passwordField));
        this.loginButton = element(By.id(OR.locators.storyteller.loginPageStoryteller.loginButton));
        this.sessionDialogBox = element(By.css(OR.locators.storyteller.loginPageStoryteller.sessionDialogBox));
        this.sessionDialogBoxYesButton = element(By.buttonText(OR.locators.storyteller.loginPageStoryteller.sessionDialogBoxYesButton));
        this.sessionDialogBoxWarningMessage = element(By.css(OR.locators.storyteller.loginPageStoryteller.sessionDialogBoxWarningMessage));
        this.displayNameFinder = element(by.exactBinding(OR.locators.storyteller.loginPageStoryteller.displayNameFinder));

    }
    
    public get getLoginField(): ElementFinder { return this.loginField; }
    public get getPasswordField(): ElementFinder { return this.passwordField; }
    public get getLoginButton(): ElementFinder { return this.loginButton; }
    public get getSessionDialogBox(): ElementFinder { return this.sessionDialogBox; }
    public get getSessionDialogBoxYesButton(): ElementFinder { return this.sessionDialogBoxYesButton; }
    public get getSessionDialogBoxWarningMessage(): ElementFinder { return this.sessionDialogBoxWarningMessage; }
    public get getdisplayNameFinder(): ElementFinder { return this.displayNameFinder; }
 
    //login function 
    public login(login: string, password: string) {
        this.loginField.sendKeys(login);
        this.passwordField.sendKeys(password);
        this.loginButton.click();
    }
    
    //function to verify sessionDialogBox presence or not- return promise
    public  sessionDialofBox(): Promise<boolean> {
       return this.sessionDialogBox.isPresent()
            .then((present) => {
                if (present) {
                    return true;
                    } else {
                    return false;
               }
           });
    }

     //function to get warning message- return promise
    public getSessionDialofBoxWarningMessage(): Promise<string> {
        return this.sessionDialogBoxWarningMessage.getText();
    }

}
export = LoginPage;