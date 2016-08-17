import "angular";
import {
    ILocalizationService,
    ISettingsService } from "../../core";
import { ISession } from "./session.svc";

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

    public isLabelErrorStyleShowing: boolean;
    public isTextFieldErrorStyleShowing: boolean;

    public errorMessage: string;
    public novaUserName: string;
    public novaPassword: string;

    public novaCurrentPassword: string;
    public novaNewPassword: string;
    public novaConfirmNewPassword: string;

    public currentFormState: LoginState;
    public lastFormState: LoginState;
    public get isInLoginForm(): boolean {
        return this.currentFormState === LoginState.LoginForm || this.lastFormState === LoginState.LoginForm;
    }
    public get isInForgetPasswordScreen(): boolean {
        return this.currentFormState === LoginState.ForgetPasswordForm || this.lastFormState === LoginState.ForgetPasswordForm;
    }
    public get isInChangePasswordScreen(): boolean {
        return this.currentFormState === LoginState.ChangePasswordForm || this.lastFormState === LoginState.ChangePasswordForm;
    }
    public get isInSAMLScreen(): boolean {
        return this.currentFormState === LoginState.SamlLoginForm || this.lastFormState === LoginState.SamlLoginForm;
    }
    public get isUsernameDisabled(): boolean {
        return !!this.session.forceUsername();
    }
    
    public isForgetPasswordScreenEnabled: boolean;
    public hasForgetPasswordScreenError: boolean;
    public forgetPasswordScreenMessage: string;
    public forgetPasswordScreenUsername: string;

    public isChangePasswordScreenEnabled: boolean;
    public hasChangePasswordScreenError: boolean;
    public changePasswordScreenMessage: string;

    public isCurrentPasswordFieldErrorStyleShowing: boolean; 
    public isNewPasswordFieldErrorStyleShowing: boolean;
    public isConfirmPasswordFieldErrorStyleShowing: boolean;

    public SAMLScreenMessage: string;

    public isLoginInProgress: boolean;

    static $inject: [string] = ["localization", "$uibModalInstance", "session", "$timeout", "settings"];
    /* tslint:disable */
    constructor(private localization: ILocalizationService, private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private session: ISession, private $timeout: ng.ITimeoutService, private settings: ISettingsService) {
        /* tslint:enable */
        this.currentFormState = LoginState.LoginForm;
        this.errorMessage = session.getLoginMessage();
        this.novaUserName = session.forceUsername();

        this.isForgetPasswordScreenEnabled = false;

        this.forgetPasswordScreenMessage = localization.get("Login_Session_EnterUsername");

        this.isChangePasswordScreenEnabled = false;

        this.changePasswordScreenMessage = localization.get("Login_Session_PasswordHasExpired_ChangePasswordPrompt");

        this.SAMLScreenMessage = localization.get("Login_Session_EnterSamlCredentials_Verbose");

        this.isLoginInProgress = false;

        if (!this.novaCurrentPassword) {
            this.novaCurrentPassword = "";
        }
        if (!this.novaNewPassword) {
            this.novaNewPassword = "";
        }
        if (!this.novaConfirmNewPassword) {
            this.novaConfirmNewPassword = "";
        }
    }

    private transitionToState(state: LoginState) {
        this.lastFormState = this.currentFormState;
        this.currentFormState = state;
        this.$timeout(() => {
            this.lastFormState = state;
        }, 200); // both panels need to be visible during the transition
    }

    public goToForgetPasswordScreen(): void {
        this.hasForgetPasswordScreenError = false;
        this.forgetPasswordScreenUsername = this.novaUserName;
        this.transitionToState(LoginState.ForgetPasswordForm);
    }

    public goToChangePasswordScreen(): void {
        this.hasChangePasswordScreenError = false;
        this.transitionToState(LoginState.ChangePasswordForm);
    }

    public goToChangePasswordScreenBecauseExpired(): void {
        this.hasChangePasswordScreenError = true;
        this.transitionToState(LoginState.ChangePasswordForm);
    }

    public doSamlLogin(): void {
        this.session.loginWithSaml(false).then(
            () => {
                this.isLabelErrorStyleShowing = false;
                this.isTextFieldErrorStyleShowing = false;
                let result: ILoginInfo = new ILoginInfo();
                result.loginSuccessful = true;

                this.$uibModalInstance.close(result);
            },
            (error) => {
                this.handleLoginErrors(error);
            });

    }

    public get isFederatedAuthenticationEnabled(): boolean {
        return this.settings.getBoolean("IsFederatedAuthenticationEnabled") === true;
    }

    public get samlPrompt(): string {
        let prompt: string = this.settings.get("FederatedAuthenticationPrompt");
        if (!prompt || prompt === "") {
            prompt = this.localization.get("Login_SamlLink");
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
        this.hasChangePasswordScreenError = false;
        this.isCurrentPasswordFieldErrorStyleShowing = false;
        this.isNewPasswordFieldErrorStyleShowing = false;
        this.isConfirmPasswordFieldErrorStyleShowing = false;

        if (this.novaCurrentPassword.length === 0) {
            this.changePasswordScreenMessage = this.localization.get("Login_Session_CurrentPasswordCannotBeEmpty");
            this.hasChangePasswordScreenError = true;
            this.isCurrentPasswordFieldErrorStyleShowing = true;
            return;
        } else if (this.novaNewPassword.length < 8) {
            this.changePasswordScreenMessage = this.localization.get("Login_Session_NewPasswordMinLength");
            this.hasChangePasswordScreenError = true;
            this.isNewPasswordFieldErrorStyleShowing = true;
            return;
        } else if (this.novaNewPassword.length > 128) {
            this.changePasswordScreenMessage = this.localization.get("Login_Session_NewPasswordMaxLength");
            this.hasChangePasswordScreenError = true;
            this.isNewPasswordFieldErrorStyleShowing = true;
            return;
        }
        if (this.novaNewPassword !== this.novaConfirmNewPassword) {
            this.changePasswordScreenMessage = this.localization.get("Login_Session_PasswordConfirmMismatch");
            this.hasChangePasswordScreenError = true;
            this.isNewPasswordFieldErrorStyleShowing = true;
            this.isConfirmPasswordFieldErrorStyleShowing = true;
            return;
        }

        this.session.resetPassword(this.novaUserName, this.novaCurrentPassword, this.novaNewPassword).then(
            () => {
                this.changePasswordScreenMessage = this.localization.get("Login_Session_PasswordChangedSuccessfully");
                this.errorMessage = this.localization.get("Login_Session_PasswordChangedSuccessfully");
                this.isLabelErrorStyleShowing = false;
                this.isTextFieldErrorStyleShowing = false;

                this.transitionToState(LoginState.LoginForm);

                this.isChangePasswordScreenEnabled = false;
            },
            (error) => {
                this.handlePasswordResetErrors(error);
            }
        );
    }

    public resetPassword(): void {
        // TODO: back-end not ready yet
    }

    private handlePasswordResetErrors(error) {
        this.hasChangePasswordScreenError = true;
        if (error.statusCode === 401) {
            if (error.errorCode === 2000) {
                this.changePasswordScreenMessage = this.localization.get("Login_Session_EnterCurrentPassword");
            } else if (error.errorCode === 1001) {
                this.changePasswordScreenMessage = this.localization.get("Login_Auth_FederatedFallbackDisabled");
            } else if (error.errorCode === 2001) {
                this.changePasswordScreenMessage = this.localization.get("Login_Session_AccountDisabled");
            } else if (error.errorCode === 2003) {
                this.changePasswordScreenMessage = this.localization.get("Login_Session_CurrentPasswordCannotBeEmpty");
            } else {
                this.changePasswordScreenMessage = "authorization exception: " + error.message;
            }
            this.isCurrentPasswordFieldErrorStyleShowing = true;
            this.isNewPasswordFieldErrorStyleShowing = false;
            this.isConfirmPasswordFieldErrorStyleShowing = false;
        } else if (error.statusCode === 400) {
            this.isCurrentPasswordFieldErrorStyleShowing = false;
            this.isNewPasswordFieldErrorStyleShowing = false;
            this.isConfirmPasswordFieldErrorStyleShowing = false;
            if (error.errorCode === 4000) {
                this.changePasswordScreenMessage = this.localization.get("Login_Session_NewPasswordCannotBeEmpty");
                this.isNewPasswordFieldErrorStyleShowing = true;
            } else if (error.errorCode === 4001) {
                this.changePasswordScreenMessage = this.localization.get("Login_Session_NewPasswordSameAsOld");
                this.isCurrentPasswordFieldErrorStyleShowing = true;
                this.isNewPasswordFieldErrorStyleShowing = true;
            } else if (error.errorCode === 4002) {
                this.changePasswordScreenMessage = this.localization.get("Login_Session_NewPasswordCriteria");
                this.isNewPasswordFieldErrorStyleShowing = true;
            } else {
                this.changePasswordScreenMessage = "bad request: " + error.message;
            }
        } else {
            this.changePasswordScreenMessage = error.message;
            this.isCurrentPasswordFieldErrorStyleShowing = false;
            this.isNewPasswordFieldErrorStyleShowing = false;
            this.isConfirmPasswordFieldErrorStyleShowing = false;
        }
    }

    private handleLoginErrors(error) {
        if (error.statusCode === 401) {
            if (error.errorCode === 2000) {
                if (this.currentFormState === LoginState.SamlLoginForm) {
                    this.errorMessage = this.localization.get("Login_Session_ADUserNotInDB");
                    this.isTextFieldErrorStyleShowing = false;
                } else {
                    this.errorMessage = this.localization.get("Login_Session_CredentialsInvalid");
                    this.isTextFieldErrorStyleShowing = true;
                }
                this.transitionToState(LoginState.LoginForm);
            } else if (error.errorCode === 1001) {
                this.errorMessage = this.localization.get("Login_Auth_FederatedFallbackDisabled");
                this.isTextFieldErrorStyleShowing = false;
                this.transitionToState(LoginState.LoginForm);
            } else if (error.errorCode === 2001) {
                this.errorMessage = this.localization.get("Login_Session_AccountDisabled");
                this.isTextFieldErrorStyleShowing = false;
                this.transitionToState(LoginState.LoginForm);
            } else if (error.errorCode === 2002) {
                this.errorMessage = this.localization.get("Login_Session_PasswordHasExpired");
                this.isTextFieldErrorStyleShowing = false;
                if (this.isChangePasswordScreenEnabled) {
                    this.transitionToState(LoginState.ChangePasswordForm);
                }
                this.isChangePasswordScreenEnabled = true;
                this.hasChangePasswordScreenError = true;
            } else if (error.errorCode === 2003) {
                this.errorMessage = this.localization.get("Login_Session_CredentialsCannotBeEmpty");
                this.isTextFieldErrorStyleShowing = true;
                this.transitionToState(LoginState.LoginForm);
            } else if (error.errorCode === 2004) {
                this.errorMessage = this.localization.get("Login_Auth_FederatedAuthFailed");
                this.isTextFieldErrorStyleShowing = true;
                this.transitionToState(LoginState.LoginForm);
            } else {
                this.errorMessage = error.message;
                this.isTextFieldErrorStyleShowing = true;
                this.transitionToState(LoginState.LoginForm);
            }
            this.isLabelErrorStyleShowing = true;
        } else if (error.statusCode === 409) {
            this.isLabelErrorStyleShowing = false;
            this.isTextFieldErrorStyleShowing = false;
            let result: ILoginInfo = new ILoginInfo();
            if (this.novaUserName) {
                result.userName = this.novaUserName;
                result.password = this.novaPassword;
            } else {
                result.samlLogin = true;
            }
            result.loginSuccessful = false;

            this.$uibModalInstance.close(result);
        } else if (error.statusCode === 400) {
            this.isTextFieldErrorStyleShowing = true;
            if (error.errorCode === 2004) {
                this.errorMessage = this.localization.get("Login_Auth_FederatedAuthFailed");
                this.transitionToState(LoginState.LoginForm);
            } else {
                this.errorMessage = error.message;
                this.isLabelErrorStyleShowing = true;
            }
        } else if (error.statusCode === 404) {
            this.errorMessage = error.message;
            this.isLabelErrorStyleShowing = true;
            this.isTextFieldErrorStyleShowing = false;
        } else if (error.statusCode === 403) {
            this.errorMessage = error.message;
            this.isLabelErrorStyleShowing = true;
            this.isTextFieldErrorStyleShowing = false;
        } else {
            this.errorMessage = error.message;
            this.isLabelErrorStyleShowing = true;
            this.isTextFieldErrorStyleShowing = true;
        }
    }

    public login(): void {
        this.isLoginInProgress = true;

        this.session.login(this.novaUserName, this.novaPassword, false).then(
            () => {
                this.isLabelErrorStyleShowing = false;
                this.isTextFieldErrorStyleShowing = false;
                this.isLoginInProgress = false;
                let result: ILoginInfo = new ILoginInfo();
                result.loginSuccessful = true;

                this.$uibModalInstance.close(result);
            },
            (error) => {
                this.isLoginInProgress = false;
                this.handleLoginErrors(error);
            });
    }
}