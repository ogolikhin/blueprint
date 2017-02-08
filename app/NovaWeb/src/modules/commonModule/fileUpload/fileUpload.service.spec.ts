import "./"; //imports the index.ts file with module and dependencies
import "angular-mocks";
import {Helper} from "../../shared";
import {HttpStatusCode} from "../httpInterceptor/http-status-code";
import {IFileUploadService, IFileResult} from "./";

describe("File Upload", () => {
    beforeEach(angular.mock.module("fileUpload"));

    describe("uploadToFileStore", () => {
        it("calls server and returns result when expirationDate is specified",
            inject(($httpBackend: ng.IHttpBackendService, fileUploadService: IFileUploadService) => {
                // Arrange
                const file = {name: "empty.txt"};
                const expirationDate = new Date();
                const data = {guid: Helper.UID, uriToFile: "http://example.com/"};
                $httpBackend.when("POST", `/svc/bpfilestore/files/?expired=${expirationDate.toISOString()}`)
                    .respond(data);

                // Act
                let error: any;
                let result: IFileResult;
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
                const file = {name: "empty.txt"};
                const data = {guid: Helper.UID, uriToFile: "http://example.com/"};
                $httpBackend.when("POST", `/svc/bpfilestore/files/`)
                    .respond(data);

                // Act
                let error: any;
                let result: IFileResult;
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
                const file = {name: "empty.txt"};
                const status = HttpStatusCode.ServerError;
                const data = {message: "Internal Server Error"};
                const expirationDate = new Date();
                $httpBackend.when("POST", `/svc/bpfilestore/files/?expired=${expirationDate.toISOString()}`)
                    .respond(status, data);

                // Act
                let error: any;
                let result: IFileResult;
                fileUploadService.uploadToFileStore(file, expirationDate).then(response => result = response, response => error = response);
                $httpBackend.flush();

                // Assert
                expect(result).toBeUndefined();
                expect(error).toEqual({statusCode: status, message: data.message});
                $httpBackend.verifyNoOutstandingExpectation();
                $httpBackend.verifyNoOutstandingRequest();
            }));
    });

    describe("uploadImageToFileStore", () => {
        it("calls server and returns result when expirationDate is specified",
            inject(($httpBackend: ng.IHttpBackendService, fileUploadService: IFileUploadService) => {
                // Arrange
                const file = {name: "empty.png"};
                const data = {guid: Helper.UID, uriToFile: "http://example.com/"};
                $httpBackend.when("POST", "/svc/bpartifactstore/images/").respond(data);

                // Act
                let error: any;
                let result: IFileResult;
                fileUploadService.uploadImageToFileStore(file).then(response => result = response, response => error = response);
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
                const file = {name: "empty.png"};
                const status = HttpStatusCode.ServerError;
                const data = {message: "Internal Server Error", errorCode: 131};
                $httpBackend.when("POST", "/svc/bpartifactstore/images/").respond(status, data);

                // Act
                let error: any;
                let result: IFileResult;
                fileUploadService.uploadImageToFileStore(file).then(response => result = response, response => error = response);
                $httpBackend.flush();

                // Assert
                expect(result).toBeUndefined();
                expect(error).toEqual({statusCode: status, message: data.message, errorCode: 131});
                $httpBackend.verifyNoOutstandingExpectation();
                $httpBackend.verifyNoOutstandingRequest();
            }));
    });
});
