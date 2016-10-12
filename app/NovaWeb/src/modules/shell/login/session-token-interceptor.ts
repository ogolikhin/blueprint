import "angular";
import {SessionTokenHelper} from "./session.token.helper";

export class SessionTokenInterceptor {
    constructor() {
        //fixme: empty constructors are not needed and should be removed
    }

    public request = (config: ng.IRequestConfig) => {
        if (config.headers && config.headers[SessionTokenHelper.SESSION_TOKEN_KEY]) {
            return config;
        }

        const token = SessionTokenHelper.getSessionToken();
        if (token) {
            if (!config.headers) {
                config.headers = {};
            }
            config.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = token;
        }
        return config;
    };
}

