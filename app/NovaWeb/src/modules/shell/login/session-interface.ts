import { IUser} from "./auth.svc";

export interface ISession {
    ensureAuthenticated(): ng.IPromise<any>;

    currentUser: IUser;

    logout(): ng.IPromise<any>;

    login(username: string, password: string, overrideSession: boolean): ng.IPromise<any>;

    loginWithSaml(overrideSession: boolean): ng.IPromise<any>;

    resetPassword(login: string, oldPassword: string, newPassword: string): ng.IPromise<any>;

    onExpired(): ng.IPromise<any>;

    getLoginMessage(): string;
    forceUsername(): string;
}