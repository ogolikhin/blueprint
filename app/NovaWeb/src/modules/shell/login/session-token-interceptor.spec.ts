import * as angular from "angular";
import "angular-mocks";
import {SessionTokenInterceptor} from "./session-token-interceptor";
import {SessionTokenHelper} from "./session.token.helper";

describe("SessionTokenInterceptor", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {

    }));

    it("return http config when Session-Token already defined", () => {
        // Arrange
        let tokenHelperSpy = spyOn(SessionTokenHelper, "getSessionToken");
        let config = <ng.IRequestConfig>{};
        config.headers = {};
        config.headers[SessionTokenHelper.SESSION_TOKEN_KEY] = "test";
        let interceptor = new SessionTokenInterceptor();

        // Act
        let result = interceptor.request(config);

        // Assert
        expect(result).toEqual(config, "Config is different");
        expect(tokenHelperSpy).not.toHaveBeenCalled();
    });

    it("add Session-Token when not defined", () => {
        // Arrange
        const token = "TEST-TOKEN";
        let tokenHelperSpy = spyOn(SessionTokenHelper, "getSessionToken").and.returnValue(token);
        let config = <ng.IRequestConfig>{};
        let interceptor = new SessionTokenInterceptor();

        // Act
        let result = interceptor.request(config);

        // Assert
        expect(tokenHelperSpy).toHaveBeenCalled();
        expect(result.headers).toBeDefined("headers are not defined");
        expect(result.headers[SessionTokenHelper.SESSION_TOKEN_KEY]).toEqual(token, "token is different");
    });

});
