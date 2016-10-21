export class SessionTokenHelper {
    private static SESSION_TOKEN_ID = "BLUEPRINT_SESSION_TOKEN";

    public static get SESSION_TOKEN_KEY(): string {
        return "Session-Token";
    };

    public static clearSessionToken(): void {
        if (window.localStorage) {
            window.localStorage.removeItem(this.SESSION_TOKEN_ID);
            document.cookie = `${this.SESSION_TOKEN_ID}=; Path=/;`;
        }
    }

    public static hasSessionToken(): boolean {
        if (window.localStorage) {
            return !!window.localStorage.getItem(this.SESSION_TOKEN_ID);
        }
    }

    public static setToken(token: string) {
        if (window.localStorage) {
            window.localStorage.setItem(this.SESSION_TOKEN_ID, token);
            document.cookie = `${this.SESSION_TOKEN_ID}=${token}; Path=/;`;
        } else {
            // throw some error here
        }
    }

    public static getSessionToken() {
        if (window.localStorage) {
            return window.localStorage.getItem(this.SESSION_TOKEN_ID);
        } else {
            // throw some error here
        }
        return undefined;
    }
}
