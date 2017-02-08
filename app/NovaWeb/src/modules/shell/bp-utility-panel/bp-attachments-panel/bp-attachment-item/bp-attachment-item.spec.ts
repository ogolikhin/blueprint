import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "../../../";
import {ComponentTest} from "../../../../util/component.test";
import {BPAttachmentItemController} from "./bp-attachment-item";
import {LocalizationServiceMock} from "../../../../commonModule/localization/localization.service.mock";
import {ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {SelectionManagerMock} from "../../../../managers/selection-manager/selection-manager.mock";
import {IStatefulArtifactFactory} from "../../../../managers/artifact-manager";
import {StatefulArtifactFactoryMock} from "../../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {IDownloadService} from "../../../../commonModule/download/download.service";
import {DownloadServiceMock} from "../../../../commonModule/download/download.service.mock";
import {LicenseServiceMock} from "../../../license/license.svc.mock";

describe("Component BP Artifact Attachment Item", () => {

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("downloadService", DownloadServiceMock);
        $provide.service("licenseService", LicenseServiceMock);
    }));

    let componentTest: ComponentTest<BPAttachmentItemController>;
    let template = `
        <bp-attachment-item
            attachment-info="attachment" delete-item="delete()">
        </bp-attachment-item>
    `;
    let vm: BPAttachmentItemController;
    let downloadService: IDownloadService;

    beforeEach(inject(( $window: ng.IWindowService,
                        _downloadService_: IDownloadService) => {
        let bindings: any = {
            attachment: {
                userId: 1,
                userName: "admin",
                fileName: "test.png",
                attachmentId: 1093,
                uploadedDate: "2016-06-23T14:54:27.273Z"
            },
            delete: () => {
                $window.alert("Test Alert");
            }
        };
        componentTest = new ComponentTest<BPAttachmentItemController>(template, "bp-attachment-item");
        vm = componentTest.createComponent(bindings);
        downloadService = _downloadService_;
    }));

    it("should be visible by default", () => {
        //Assert
        expect(componentTest.element.find(".author").length).toBe(1);
        expect(componentTest.element.find(".button-bar").length).toBe(1);
        expect(componentTest.element.find("h6").length).toBe(1);
        expect(componentTest.element.find(".ext-image").length).toBe(1);
    });

    it("should try to download an attachment without Guid",
        inject(($rootScope: ng.IRootScopeService,
                selectionManager: ISelectionManager,
                statefulArtifactFactory: IStatefulArtifactFactory) => {

            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "My"});

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            const spyDownload = spyOn(downloadService, "downloadFile");
            vm.downloadItem();

            //Assert
            expect(spyDownload).toHaveBeenCalled();
            expect(spyDownload).toHaveBeenCalledWith("/svc/bpartifactstore/artifacts/22/attachments/1093");
        }));

    it("should try to download historical version",
        inject(($rootScope: ng.IRootScopeService,
                selectionManager: ISelectionManager,
                statefulArtifactFactory: IStatefulArtifactFactory) => {

            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({
                id: 22,
                name: "Artifact",
                prefix: "My",
                version: 14
            });
            artifact.artifactState.historical = true;

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            const spyDownload = spyOn(downloadService, "downloadFile");
            vm.downloadItem();

            //Assert
            expect(spyDownload).toHaveBeenCalled();
            expect(spyDownload).toHaveBeenCalledWith("/svc/bpartifactstore/artifacts/22/attachments/1093?versionId=14");
        }));

    it("should try to download an attachment with Guid",
        inject(($rootScope: ng.IRootScopeService,
                selectionManager: ISelectionManager,
                statefulArtifactFactory: IStatefulArtifactFactory) => {

            //Arrange
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "My"});
            vm.attachmentInfo.guid = "newid";

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            const spyDownload = spyOn(downloadService, "downloadFile");
            vm.downloadItem();

            //Assert
            expect(spyDownload).toHaveBeenCalled();
            expect(spyDownload).toHaveBeenCalledWith("/svc/bpfilestore/file/newid");
        }));

    it("should try to delete an attachment",
        inject(($window: ng.IWindowService) => {

            // Arrange
            spyOn($window, "alert").and.callFake(() => true);

            // Act
            vm.deleteItem();

            //Assert
            expect($window.alert).toHaveBeenCalled();
        }));
});
