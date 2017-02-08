import {IUser} from "./auth.svc";
import {ISession} from "./session.svc";

export class SessionSvcMock implements ISession {

    public static $inject = ["$q"];
    public currentUser: IUser = <IUser>{id: 1, displayName: "Default Instance Admin", login: "admin"};

    constructor(private $q: ng.IQService) {
    }

    public getLoginMessage(): string {
        return "";
    }

    public forceUsername(): string {
        return "";
    }

    public forceDisplayname(): string {
        return "";
    }

    public ensureAuthenticated() {
        const deferred = this.$q.defer<any>();
        this.currentUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
        deferred.resolve();
        return deferred.promise;
    }

    public logout() {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    public onExpired() {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }

    public login(username: string, password: string, overrideSession: boolean) {
        const deferred = this.$q.defer<any>();
        this.currentUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
        deferred.resolve();
        return deferred.promise;
    }

    public loginWithSaml(overrideSession: boolean) {
        const deferred = this.$q.defer<any>();
        this.currentUser = <IUser>{displayName: "Default Instance Admin", login: "admin"};
        deferred.resolve();
        return deferred.promise;
    }

    public resetPassword(login: string, oldPassword: string, newPassword: string) {
        const deferred = this.$q.defer<any>();
        deferred.resolve();
        return deferred.promise;
    }
}
