
//import Promise = protractor.promise.Promise;

var OR = require('../../Json/OR.json');
class LoginPage {

    public static loginField = element(By.id('loginField'));
    private static passwordField = element(By.id('passwordField'));
    private static loginButton = element(By.id('loginButton'));
   
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
}
export = LoginPage;