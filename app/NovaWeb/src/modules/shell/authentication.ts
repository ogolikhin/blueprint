import {ISession} from "./login/session.svc";
//TODO: move to other file
export class AuthenticationRequired {
    private static key = "authenticated";
    public resolve = {};

    constructor() {
        this.resolve[AuthenticationRequired.key] = [
            "$log", "session", ($log: ng.ILogService, session: ISession): ng.IPromise<any> => {
                $log.debug("AuthenticationRequired...called");
                return session.ensureAuthenticated();
            }
        ];
    }
}
