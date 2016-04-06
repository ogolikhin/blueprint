import "angular";
import {ConfirmationDialogCtrl} from "./../messaging/confirmation.dialog.ctrl";
import {ILocalizationService} from "../../core/localization";
import {IConfigValueHelper} from "../../core/config.value.helper";
import {ISession} from "./session.svc";

export class SimpleDialogCtrl extends ConfirmationDialogCtrl {

    constructor(localization: ILocalizationService, $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
        super(localization, $uibModalInstance);
        this.acceptButtonName = localization.get('App_Button_Yes');
        this.cancelButtonName = localization.get('App_Button_No');
        this.msg = localization.get('Login_Session_DuplicateSession_Verbose');
    }
}

export class ILoginInfo {
    public userName: string;
    public password: string;
    public loginSuccessful: boolean;
    public samlLogin: boolean;
}

export enum LoginState {
    LoginForm,
    ForgetPasswordForm,
    ChangePasswordForm,
    SamlLoginForm
}

export class LoginCtrl {

    public labelError: boolean;
    public fieldError: boolean;

    public errorMsg: string;
    public novaUsername: string;
    public novaPassword: string;

    public novaCurrentPassword: string;
    public novaNewPassword: string;
    public novaConfirmNewPassword: string;

    public formState: LoginState;
    public transitionFromState: LoginState;
    public get isInLoginForm(): boolean {
        return this.formState === LoginState.LoginForm || this.transitionFromState === LoginState.LoginForm;
    }
    public get isInForgetPasswordScreen(): boolean {
        return this.formState === LoginState.ForgetPasswordForm || this.transitionFromState === LoginState.ForgetPasswordForm;
    }
    public get isInChangePasswordScreen(): boolean {
        return this.formState === LoginState.ChangePasswordForm || this.transitionFromState === LoginState.ChangePasswordForm;
    }
    public get isInSAMLScreen(): boolean {
        return this.formState === LoginState.SamlLoginForm || this.transitionFromState === LoginState.SamlLoginForm;
    }

    public enableForgetPasswordScreen: boolean;
    public forgetPasswordScreenError: boolean;
    public forgetPasswordScreenMessage: string;
    public forgetPasswordScreenUsername: string;

    public enableChangePasswordScreen: boolean;
    public changePasswordScreenError: boolean;
    public changePasswordScreenMessage: string;
    public changePasswordCurrentPasswordError: boolean; //if the user doesn't put the correct current password
    public changePasswordNewPasswordError: boolean; //if the new password doesn't satisfy the security criteria
    public changePasswordConfirmPasswordError: boolean; //if new password and confirm password don't match

    public enableSAMLScreen: boolean;
    public SAMLScreenMessage: string;

    static $inject: [string] = ["localization", "$uibModalInstance", "session", "$timeout", "configValueHelper"];
    constructor(private localization: ILocalizationService, private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private session: ISession, private $timeout: ng.ITimeoutService, private configValueHelper: IConfigValueHelper) {
        this.formState = LoginState.LoginForm;
        this.errorMsg = localization.get('Login_Session_EnterCredentials');

        this.enableForgetPasswordScreen = false;

        this.forgetPasswordScreenMessage = localization.get('Login_Session_EnterUsername');

        this.enableChangePasswordScreen = false;

        this.changePasswordScreenMessage = localization.get('Login_Session_PasswordHasExpired_ChangePasswordPrompt');

        this.enableSAMLScreen = true;

        this.SAMLScreenMessage = localization.get('Login_Session_EnterSamlCredentials_Verbose');
    }

    private transitionToState(state: LoginState) {
        this.transitionFromState = this.formState;
        this.formState = state;
        this.$timeout(() => {
            this.transitionFromState = state;
        }, 200); // both panels need to be visible during the transition
    }

    public goToForgetPasswordScreen(): void {
        this.forgetPasswordScreenError = false;
        this.forgetPasswordScreenUsername = this.novaUsername;
        this.transitionToState(LoginState.ForgetPasswordForm);
    }

    public goToChangePasswordScreen(): void {
        this.changePasswordScreenError = false;
        this.transitionToState(LoginState.ChangePasswordForm);
    }

    public doSamlLogin(): void {
        this.session.loginWithSaml(false).then(
            () => {
                this.labelError = false;
                this.fieldError = false;
                var result: ILoginInfo = new ILoginInfo();
                result.loginSuccessful = true;

                this.$uibModalInstance.close(result);
            },
            (error) => {
                this.handleLoginErrors(error);
                this.transitionToState(LoginState.LoginForm);
            });

    }

    public get isFederatedAuthenticationEnabled(): boolean {
        return this.configValueHelper.getBooleanValue("IsFederatedAuthenticationEnabled") === true;
    }

    public get samlPrompt(): string {
        var prompt: string = this.configValueHelper.getStringValue("FederatedAuthenticationPrompt");
        if (!prompt || prompt == "") {
            prompt = this.localization.get('Login_SamlLink');
        }
        return prompt;
    }

    public goToSAMLScreen(): void {
        this.doSamlLogin();
        this.transitionToState(LoginState.SamlLoginForm);
    }

    public goToLoginScreen(): void {
        this.transitionToState(LoginState.LoginForm);
    }

    public changePassword(): void {
        if (this.novaNewPassword.length < 8) {
            //error
        } else if (this.novaNewPassword.length > 128) {
            //error
        }
        if (this.novaNewPassword != this.novaConfirmNewPassword) {
            //error
        }

        // TODO: back-end not ready yet
    }

    public resetPassword(): void {
        // TODO: back-end not ready yet
    }

    private handleLoginErrors(error) {
        if (error.statusCode === 401) {
            if (error.errorCode === 2000) {
                this.errorMsg = this.localization.get('Login_Session_CredentialsInvalid');
                this.labelError = true;
                this.fieldError = true;
                this.transitionToState(LoginState.LoginForm);
            } else if (error.errorCode === 2001) {
                this.errorMsg = this.localization.get('Login_Session_AccountDisabled');
                this.labelError = true;
                this.fieldError = false;
                this.transitionToState(LoginState.LoginForm);
            } else if (error.errorCode === 2002) {
                this.errorMsg = this.localization.get('Login_Session_PasswordHasExpired');
                this.labelError = true;
                this.fieldError = false;
                this.enableChangePasswordScreen = true;
                this.transitionToState(LoginState.ChangePasswordForm);
            } else if (error.errorCode === 2003) {
                this.errorMsg = this.localization.get('Login_Session_CredentialsCannotBeEmpty');
                this.labelError = true;
                this.fieldError = true;
                this.transitionToState(LoginState.LoginForm);
            } else {
                this.errorMsg = error.message;
                this.labelError = true;
                this.fieldError = true;
                this.transitionToState(LoginState.LoginForm);
            }
        } else if (error.statusCode === 409) {
            this.labelError = false;
            this.fieldError = false;
            var result: ILoginInfo = new ILoginInfo();
            if (this.novaUsername) {
                result.userName = this.novaUsername;
                result.password = this.novaPassword;
            } else {
                result.samlLogin = true;
            }
            result.loginSuccessful = false;

            this.$uibModalInstance.close(result);
        } else {
            this.errorMsg = error.message;
            this.labelError = true;
            this.fieldError = true;
        }
    }

    public login(): void {
        this.session.login(this.novaUsername, this.novaPassword, false).then(
            () => {
                this.labelError = false;
                this.fieldError = false;
                var result: ILoginInfo = new ILoginInfo();
                result.loginSuccessful = true;

                this.$uibModalInstance.close(result);
            },
            (error) => {
                this.handleLoginErrors(error);
            });
    }
}