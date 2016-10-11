export class SessionTokenHelper {
    private static SESSION_TOKEN_ID = "BLUEPRINT_SESSION_TOKEN";

    public static get SESSION_TOKEN_KEY(): string {
        return "Session-Token";
    };

    public static setToken(token: string) {
        if (window.localStorage != null) {
            window.localStorage.setItem(this.SESSION_TOKEN_ID, token);
            document.cookie = `${this.SESSION_TOKEN_ID}=${token}; Path=/;`;
        } else {
            // throw some error here
        }
    }

    public static getSessionToken() {
        if (window.localStorage != null) {
            return window.localStorage.getItem(this.SESSION_TOKEN_ID);
        } else {
            // throw some error here
        }
        return null;
    }

    //currently unused
    /*public static updateTokenCookie() {
     var token = this.getSessionToken();
     if (token) {
     this.setToken(token);
     }
     }*/
}
