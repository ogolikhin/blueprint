
//import Promise = protractor.promise.Promise;

var OR = require('../../Json/OR.json');
class LoginPage {

    private static loginField = element(By.id('loginField'));
    private static passwordField = element(By.id('passwordField'));
    private static loginButton = element(By.id('loginButton'));
    private static sessionDialogBox = element(By.css('.new-line.ng-binding'));
   // private static sessionDialogBoxYesButton = element(By.css('.btn.action-.btn.ng-binding'));
    private static sessionDialogBoxYesButton = element(By.buttonText('Yes'));

    
   
    public static expect(): LoginPage {
        
        expect(LoginPage.loginField.isPresent()).toBeTruthy();
        expect(LoginPage.passwordField.isPresent()).toBeTruthy();

        return new LoginPage();
    }

    public static login(login: string, password: string) {
        LoginPage.loginField.sendKeys(login);
        LoginPage.passwordField.sendKeys(password);
        LoginPage.loginButton.click();

    }
    public login(login: string, password: string, override?: boolean) {
        LoginPage.loginField.sendKeys(login);
        LoginPage.passwordField.sendKeys(password);
        LoginPage.loginButton.click();
    }
    public static sessionDialofBox() {
       // expect(LoginPage.sessionDialogBox.isPresent()).toBeFalsy();
        if ((LoginPage.sessionDialogBox.isPresent())) {
            LoginPage.sessionDialogBoxYesButton.click();
        }
    }
}
export = LoginPage;