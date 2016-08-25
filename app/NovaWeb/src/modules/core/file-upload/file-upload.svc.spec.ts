import "angular";
import "angular-mocks";
import { IFileUploadService, FileUploadService, IFileResult } from "./";
import { Helper } from "../../shared";

describe("File Upload", () => {
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("fileUploadService", FileUploadService);
    }));

    describe("uploadToFileStore", () => {
        it("calls server and returns result when expirationDate is specified",
            inject(($httpBackend: ng.IHttpBackendService, fileUploadService: IFileUploadService) => {
            // Arrange
            const file = { name: "empty.txt" };
            const expirationDate = new Date();
            const data = { guid: Helper.UID, uriToFile: "http://example.com/" };
            $httpBackend.when("POST", `/svc/bpfilestore/files/${file.name}?expired=${expirationDate.toISOString()}`)
                .respond(data);

            // Act
            var error: any;
            var result: IFileResult;
            fileUploadService.uploadToFileStore(file, expirationDate).then(response => result = response, response => error = response);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(result).toEqual(data);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
        it("calls server and returns result when expirationDate is not specified",
            inject(($httpBackend: ng.IHttpBackendService, fileUploadService: IFileUploadService) => {
            // Arrange
            const file = { name: "empty.txt" };
            const data = { guid: Helper.UID, uriToFile: "http://example.com/" };
            $httpBackend.when("POST", `/svc/bpfilestore/files/${file.name}`)
                .respond(data);

            // Act
            var error: any;
            var result: IFileResult;
            fileUploadService.uploadToFileStore(file).then(response => result = response, response => error = response);
            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(result).toEqual(data);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
        it("calls server and returns error when the server returns an error",
            inject(($httpBackend: ng.IHttpBackendService, fileUploadService: IFileUploadService) => {
            // Arrange
            const file = { name: "empty.txt" };
            const status = 500;
            const data = { message: "Internal Server Error" };
            const expirationDate = new Date();
            $httpBackend.when("POST", `/svc/bpfilestore/files/${file.name}?expired=${expirationDate.toISOString()}`)
                .respond(status, data);

            // Act
            var error: any;
            var result: IFileResult;
            fileUploadService.uploadToFileStore(file, expirationDate).then(response => result = response, response => error = response);
            $httpBackend.flush();

            // Assert
            expect(result).toBeUndefined();
            expect(error).toEqual({ statusCode: status, message: data.message });
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
    });
});
