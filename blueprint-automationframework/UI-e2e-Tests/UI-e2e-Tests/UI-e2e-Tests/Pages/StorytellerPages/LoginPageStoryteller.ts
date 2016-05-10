
//import Promise = protractor.promise.Promise;

var OR = require('../../Json/OR.json');
class LoginPageStoryteller {

    public static loginField = element(By.id('loginField'));
    private static passwordField = element(By.id('passwordField'));
    private static loginButton = element(By.id('loginButton'));
    

    //browser.element(By.id('loginField')).sendKeys('admin');
    // browser.element(By.id('passwordField')).sendKeys('changeme');
    // browser.element(By.id('loginButton')).click();
    // browser.driver.sleep(5000);
    public static expect(): LoginPageStoryteller {
        //IndexPage.expect(); //don't think we need this when expecting login page
        expect(LoginPageStoryteller.loginField.isPresent()).toBeTruthy();
        expect(LoginPageStoryteller.passwordField.isPresent()).toBeTruthy();

        return new LoginPageStoryteller();
    }

    public static login(login: string, password: string) {
        LoginPageStoryteller.loginField.sendKeys(login);
        LoginPageStoryteller.passwordField.sendKeys(password);
        LoginPageStoryteller.loginButton.click();

    }
}
export = LoginPageStoryteller;