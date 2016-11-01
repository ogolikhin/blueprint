import * as angular from "angular";
import "angular-mocks";
import {HttpStatusCode} from "../../core/http";
import {LicenseService, ILicenseService} from "./license.svc";

describe("License Service", () => {

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("licenseService", LicenseService);
    }));

    describe("Get Server License Status", () => {
        it("returns true on valid server license", inject(($httpBackend: ng.IHttpBackendService, licenseService: ILicenseService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/shared/licenses/verifyStorytellerAccess", "")
                .respond(HttpStatusCode.Success);

            // Act
            let result: boolean;
            let error: any;
            licenseService.getServerLicenseValidity()
                .then((r) => result = r)
                .catch((e) => error = e);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(result).toEqual(true);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

        it("returns false on invalid server license", inject(($httpBackend: ng.IHttpBackendService, licenseService: ILicenseService) => {
            // Arrange
            $httpBackend.expectPOST("/svc/shared/licenses/verifyStorytellerAccess", "")
                .respond(HttpStatusCode.Forbidden);

            // Act
            let result: boolean;
            let error: any;
            licenseService.getServerLicenseValidity()
                .then((r) => result = r)
                .catch((e) => error = e);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(result).toEqual(false);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

    });
});
