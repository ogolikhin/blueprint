import * as angular from "angular";
import "angular-mocks";
import {BpFileUploadStatusController, IUploadStatusDialogData} from "./bp-file-upload-status";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
import {DialogServiceMock} from "../bp-dialog/bp-dialog";
import {SettingsService} from "../../../core/configuration/settings";
import {MessageService} from "../../../core/messages/message.svc";

class ModalServiceInstanceMock implements ng.ui.bootstrap.IModalServiceInstance {
    public close(result?: any): void {
        return;
    }

    public dismiss(reason?: any): void {
        return;
    }

    public result: angular.IPromise<any>;
    public opened: angular.IPromise<any>;
    public rendered: angular.IPromise<any>;
    public closed: angular.IPromise<any>;
}

describe("File Upload Status", () => {
    let $scope, $q, localization, $filter;
    let controller: BpFileUploadStatusController;
    let dialogService: DialogServiceMock;

    function createController(dialogData: IUploadStatusDialogData): BpFileUploadStatusController {
        const dialogSettings = {
            okButton: "Attach",
            template: "test--bp-file-upload-status.html",
            controller: null,
            css: "nova-file-upload-status",
            header: "File Upload"
        };

        return new BpFileUploadStatusController(
            $q,
            localization,
            $filter,
            new ModalServiceInstanceMock(),
            dialogSettings,
            dialogData
        );
    }

    const mockBpFilesizeFilter = () => {
        return "0 KB";
    };

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("settings", SettingsService);
        $provide.service("messageService", MessageService);
        $provide.service("fileUploadStatus", BpFileUploadStatusController);
        $provide.service("dialogService", DialogServiceMock);
        $provide.value("bpFilesizeFilter", mockBpFilesizeFilter);
    }));

    beforeEach(inject((_$q_: ng.IQService,
                       _$filter_: ng.IFilterService,
                       $rootScope: ng.IRootScopeService,
                       _localization_: LocalizationServiceMock,
                       $compile: ng.ICompileService,
                       _dialogService_: DialogServiceMock) => {
        $scope = $rootScope.$new();
        $q = _$q_;
        localization = _localization_;
        $filter = _$filter_;
        dialogService = _dialogService_;
    }));

    afterEach(() => {
        controller = null;
    });

    it("should upload 1 file", inject(() => {
        // Arrange
        const uploadFileSpy = jasmine.createSpy("uploadFileAction").and.returnValue($q.resolve({
            guid: "test", uriToFile: "test"
        }));
        const dialogData = {
            files: [<File>{name: "testName"}],
            maxAttachmentFilesize: 10 * 1024 * 1024,
            maxNumberAttachments: 5,
            fileUploadAction: uploadFileSpy
        };

        // Act
        controller = createController(dialogData);
        $scope.$digest();

        // Assert
        expect(controller.returnValue.length).toBe(1);
        expect(controller.totalFailedFiles).toBe(0);
        expect(controller.files.length).toBe(1);
        expect(controller.files[0].isFailed).toBe(false);
    }));

    it("should not upload files if there's more than the limit", inject(() => {
        // Arrange
        const uploadFileSpy = jasmine.createSpy("uploadFileAction").and.returnValue($q.resolve({
            guid: "test", uriToFile: "test"
        }));
        const dialogData = {
            files: [
                <File>{name: "testName1"},
                <File>{name: "testName2"},
                <File>{name: "testName3"}],
            maxAttachmentFilesize: 10 * 1024 * 1024,
            maxNumberAttachments: 1,
            fileUploadAction: uploadFileSpy
        };

        // Act
        controller = createController(dialogData);
        $scope.$digest();

        // Assert
        expect(controller.returnValue.length).toBe(1);
        expect(controller.totalFailedFiles).toBe(2);
        expect(controller.files.length).toBe(3);
    }));

    it("should not upload files they're over filesize limit", inject(() => {
        // Arrange
        const uploadFileSpy = jasmine.createSpy("uploadFileAction").and.returnValue($q.resolve({
            guid: "test", uriToFile: "test"
        }));
        const dialogData = {
            files: [
                <File>{name: "testName1", size: 123},
                <File>{name: "testName2", size: 10485761},
                <File>{name: "testName3", size: 55555555}],
            maxAttachmentFilesize: 10485760, // 10 MB
            maxNumberAttachments: 5,
            fileUploadAction: uploadFileSpy
        };

        // Act
        controller = createController(dialogData);
        $scope.$digest();

        // Assert
        expect(controller.returnValue.length).toBe(1);
        expect(controller.totalFailedFiles).toBe(2);
        expect(controller.files.length).toBe(3);
    }));

    it("should remove a file when it's cancelled", inject(() => {
        const uploadFileSpy = jasmine.createSpy("uploadFileAction").and.returnValue($q.resolve({
            guid: "test", uriToFile: "test"
        }));
        const dialogData = {
            files: [
                <File>{name: "testName1", size: 123},
                <File>{name: "testName2", size: 10485},
                <File>{name: "testName3", size: 10000}],
            maxAttachmentFilesize: 10485760, // 10 MB
            maxNumberAttachments: 5,
            fileUploadAction: uploadFileSpy
        };

        // Act
        controller = createController(dialogData);
        controller.cancelUpload(controller.files[0]);
        $scope.$digest();

        // Assert
        expect(controller.returnValue.length).toBe(2);
        expect(controller.totalFailedFiles).toBe(0);
        expect(controller.files.length).toBe(2);
    }));

    it("should return all uploaded files on OK", inject(() => {
        const uploadFileSpy = jasmine.createSpy("uploadFileAction").and.returnValue($q.resolve({
            guid: "test", uriToFile: "test"
        }));
        const dialogData = {
            files: [
                <File>{name: "testName1", size: 12345},
                <File>{name: "testName2", size: 10485},
                <File>{name: "testName3", size: 10000}],
            maxAttachmentFilesize: 10485760, // 10 MB
            maxNumberAttachments: 5,
            fileUploadAction: uploadFileSpy
        };

        // Act
        controller = createController(dialogData);
        controller.ok();
        $scope.$digest();

        // Assert
        expect(controller.returnValue.length).toBe(3);
        expect(controller.totalFailedFiles).toBe(0);
        expect(controller.files.length).toBe(3);
    }));
});
