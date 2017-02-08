import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "../../../";
import {ComponentTest} from "../../../../util/component.test";
import {BPDocumentItemController} from "./bp-document-item";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {IArtifactAttachmentsService} from "../../../../managers/artifact-manager";
import {ArtifactAttachmentsMock} from "../../../../managers/artifact-manager/attachments/attachments.svc.mock";
import {IMessageService} from "../../../../main/components/messages/message.svc";
import {MessageServiceMock} from "../../../../main/components/messages/message.mock";
import {IDownloadService} from "../../../../commonModule/download/download.service";
import {DownloadServiceMock} from "../../../../commonModule/download/download.service.mock";
import {LicenseServiceMock} from "../../../license/license.svc.mock";

describe("Component BP Artifact Document Item", () => {
    const template = `<bp-document-item doc-ref-info="document" delete-item="delete()"></bp-document-item>`;
    let component: ComponentTest<BPDocumentItemController>;
    let vm: BPDocumentItemController;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("downloadService", DownloadServiceMock);
        $provide.service("licenseService", LicenseServiceMock);
    }));

    beforeEach(inject(($window: ng.IWindowService) => {
        const bindings: any = {
            document: {
                artifactName: "doc",
                artifactId: 357,
                versionId: 1,
                versionsCount: 1,
                userId: 1,
                userName: "admin",
                referencedDate: "2016-06-27T21:27:57.67Z"
            },
            delete: () => $window.alert("Test Alert")
        };

        component = new ComponentTest<BPDocumentItemController>(template, "bp-document-item");
        vm = component.createComponent(bindings);
    }));

    it("should be visible by default", () => {
        //Assert
        expect(component.element.find(".author").length).toBe(1);
        expect(component.element.find(".button-bar").length).toBe(1);
        expect(component.element.find("h6").length).toBe(1);
    });

    it("should try to download a document which has an attachment",
        inject((
            $timeout: ng.ITimeoutService,
            downloadService: IDownloadService) => {

            // Arrange
            const spyDownload = spyOn(downloadService, "downloadFile");

            // Act
            vm.downloadItem();
            $timeout.flush();

            //Assert
            expect(spyDownload).toHaveBeenCalled();
            expect(spyDownload).toHaveBeenCalledWith("/svc/bpartifactstore/artifacts/306/attachments/1093");
        }));

    it("should try to download a document which has an historical attachment",
        inject((
            $timeout: ng.ITimeoutService,
            downloadService: IDownloadService) => {

            // Arrange
            const spyDownload = spyOn(downloadService, "downloadFile");

            vm.docRefInfo.versionId = 4;

            // Act
            vm.downloadItem();
            $timeout.flush();

            //Assert
            expect(spyDownload).toHaveBeenCalled();
            expect(spyDownload).toHaveBeenCalledWith("/svc/bpartifactstore/artifacts/306/attachments/1093?versionId=4");
        }));

    it("should try to download a document which has no attachment",
        inject((
            $timeout: ng.ITimeoutService,
            downloadService: IDownloadService,
            messageService: IMessageService,
            artifactAttachments: IArtifactAttachmentsService,
            $q: ng.IQService) => {

            // Arrange
            const spyDownload = spyOn(downloadService, "downloadFile");
            spyOn(messageService, "addError").and.callFake(() => true);
            spyOn(artifactAttachments, "getArtifactAttachments").and.callFake((artifactId: number) => {
                const result = {
                    artifactId: 357,
                    subartifactId: null,
                    attachments: [],
                    documentReferences: []
                };
                const defer = $q.defer<any>();
                defer.resolve(result);
                return defer.promise;
            });

            // Act
            vm.downloadItem();
            $timeout.flush();

            //Assert
            expect(spyDownload).not.toHaveBeenCalled();
            expect(messageService.addError).toHaveBeenCalled();
        }));

    it("should try to delete an item",
        inject(($window: ng.IWindowService) => {
            // Arrange
            spyOn($window, "alert").and.callFake(() => true);

            // Act
            vm.deleteItem();

            //Assert
            expect($window.alert).toHaveBeenCalled();
        }));

    it("should navigate to document",
        inject(($state: ng.ui.IStateService, $timeout: ng.ITimeoutService) => {

        //Arrange
        const routerSpy = spyOn($state, "go");

        //Act
        component.element.find("a").click();
        $timeout.flush();

        // Assert
        expect(routerSpy).toHaveBeenCalled();
        expect(routerSpy).toHaveBeenCalledWith("main.item", {id: 357}, jasmine.any(Object));
    }));
});
