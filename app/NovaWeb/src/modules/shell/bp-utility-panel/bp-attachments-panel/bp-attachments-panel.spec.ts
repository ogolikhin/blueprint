import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import {SettingsServiceMock} from "../../../commonModule/configuration/settings.mock";
import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {Models} from "../../../main";
import {IArtifactAttachment, IArtifactAttachmentsResultSet, IArtifactDocRef} from "../../../managers/artifact-manager";
import {StatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {StatefulArtifactFactoryMock} from "../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ArtifactServiceMock} from "../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {IArtifactAttachmentsService} from "../../../managers/artifact-manager/attachments";
import {ArtifactAttachmentsMock} from "../../../managers/artifact-manager/attachments/attachments.svc.mock";
import {StatefulArtifactServices} from "../../../managers/artifact-manager/services";
import {ISelectionManager} from "../../../managers/selection-manager/selection-manager";
import {SelectionManagerMock} from "../../../managers/selection-manager/selection-manager.mock";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {ComponentTest} from "../../../util/component.test";
import {LicenseServiceMock} from "../../license/license.svc.mock";
import {BPAttachmentsPanelController} from "./bp-attachments-panel";
import {IOnPanelChangesObject, PanelType} from "../utility-panel.svc";
import {LoadingOverlayServiceMock} from "../../../commonModule/loadingOverlay/loadingOverlay.service.mock";
import {MessageServiceMock} from "../../../main/components/messages/message.mock";
import {SessionSvcMock} from "../../login/session.svc.mock";

describe("Component BP Attachments Panel", () => {

    let componentTest: ComponentTest<BPAttachmentsPanelController>;
    let template = `<bp-attachments-panel></bp-attachments-panel>`;
    let vm: BPAttachmentsPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };
    let onChangesObj: IOnPanelChangesObject;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("session", SessionSvcMock);
        $provide.service("licenseService", LicenseServiceMock);
        $provide.service("loadingOverlayService", LoadingOverlayServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("settings", SettingsServiceMock);
    }));

    beforeEach(inject((selectionManager: ISelectionManager) => {
        componentTest = new ComponentTest<BPAttachmentsPanelController>(template, "bp-attachments-panel");
        vm = componentTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
        onChangesObj = {
            context: {
                currentValue: {
                    panelType: PanelType.Files
                },
                previousValue: undefined,
                isFirstChange: () => { return true; }
            }
        };
    }));

    afterEach( () => {
        vm = undefined;
        onChangesObj = undefined;
    });

    it("should be visible by default", () => {
        //Assert
        expect(componentTest.element.find(".empty-state").length).toBe(1);
    });

    it("should load data and display it for a selected artifact",
        inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService,
            artifactAttachments: IArtifactAttachmentsService) => {

            //Arrange
            const services = new StatefulArtifactServices($q, null, null, null, null, null, null, artifactAttachments, null, null, null, null, null, null);
            const artifact = new StatefulArtifact({id: 22, name: "Artifact", prefix: "PRO"}, services);
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();

            //Assert
            expect(vm.attachmentsList).toBeDefined();
            expect(vm.attachmentsList.length).toBe(7);
            expect(vm.docRefList.length).toBe(3);
            expect(componentTest.element.find("bp-attachment-item").length).toBe(7);
            expect(componentTest.element.find("bp-document-item").length).toBe(3);
            expect(componentTest.element.find(".empty-state").length).toBe(0);
        }));

    it("addDocRef should add new document reference to the list",
        inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService,
            artifactAttachments: IArtifactAttachmentsService,
            dialogService: DialogServiceMock,
            loadingOverlayService: LoadingOverlayServiceMock,
            artifactService: ArtifactServiceMock,
            messageService: MessageServiceMock) => {

            //Arrange
            const services = new StatefulArtifactServices($q, null, null, messageService, null, null, artifactService, artifactAttachments,
                null, null, loadingOverlayService, null, null, null);
            const artifact = new StatefulArtifact({id: 22, name: "Artifact", version: 0}, services);
            let deferred = $q.defer();

            const documentName = "Document1";
            spyOn(dialogService, "open").and.callFake((dialogSettings: any): ng.IPromise<Models.IArtifact[]> => {
                deferred.resolve([{
                    name: documentName,
                    id: 3
                }]);
                return deferred.promise;
            });
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            vm.addDocRef();
            $timeout.flush();

            //Assert
            expect(vm.docRefList
                .filter((doc: IArtifactDocRef) => { return doc.artifactName === documentName; }).length)
                .toBeGreaterThan(0);
        }));

    it("the list should be empty when service throwing exception",
        inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService,
            artifactAttachments: IArtifactAttachmentsService) => {

            //Arrange
            const services = new StatefulArtifactServices($q, null, null, null, null, null, null, artifactAttachments, null, null, null, null, null, null);
            const artifact = new StatefulArtifact({id: 22, name: "Artifact", prefix: "PRO"}, services);
            let deferred = $q.defer();
            spyOn(artifactAttachments, "getArtifactAttachments").and.callFake((): ng.IPromise<IArtifactAttachmentsResultSet> => {
                deferred.reject({
                    statusCode: HttpStatusCode.NotFound,
                    errorCode: 2000
                });
                return deferred.promise;
            });
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            $timeout.flush();

            //Assert
            expect(vm.attachmentsList.length).toBe(0);
        }));

        it("Delete Attachment should remove attachment from list",
        inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService,
            artifactAttachments: IArtifactAttachmentsService,
            dialogService: DialogServiceMock,
            loadingOverlayService: LoadingOverlayServiceMock,
            artifactService: ArtifactServiceMock,
            messageService: MessageServiceMock) => {

            //Arrange
            const services = new StatefulArtifactServices($q, null, null, messageService, null, null, artifactService, artifactAttachments,
                null, null, loadingOverlayService, null, null, null);
            const artifact = new StatefulArtifact({id: 22, name: "Artifact", version: 0}, services);
            let deferred = $q.defer();

            spyOn(dialogService, "open").and.callFake((dialogSettings: any): ng.IPromise<boolean> => {
                const deferred = $q.defer<any>();
                deferred.resolve(true);
                return deferred.promise;
            });
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            const attachmentCount = vm.attachmentsList.length;
            vm.deleteAttachment(vm.attachmentsList[0]);
            $timeout.flush();

            //Assert
            expect(vm.attachmentsList.length).toBe(attachmentCount - 1);
        }));

        it("Delete DocRef should remove document from list",
        inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService,
            artifactAttachments: IArtifactAttachmentsService,
            dialogService: DialogServiceMock,
            loadingOverlayService: LoadingOverlayServiceMock,
            artifactService: ArtifactServiceMock,
            messageService: MessageServiceMock) => {

            //Arrange
            const services = new StatefulArtifactServices($q, null, null, messageService, null, null, artifactService, artifactAttachments,
                null, null, loadingOverlayService, null, null, null);
            const artifact = new StatefulArtifact({id: 22, name: "Artifact", version: 0}, services);
            let deferred = $q.defer();

            spyOn(dialogService, "open").and.callFake((dialogSettings: any): ng.IPromise<boolean> => {
                const deferred = $q.defer<any>();
                deferred.resolve(true);
                return deferred.promise;
            });
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            const docRefCount = vm.docRefList.length;
            vm.deleteDocRef(vm.docRefList[0]);
            $timeout.flush();

            //Assert
            expect(vm.docRefList.length).toBe(docRefCount - 1);
        }));

        it("should add new attachment to the list",
        inject(($rootScope: ng.IRootScopeService,
            $q: ng.IQService,
            $timeout: ng.ITimeoutService,
            artifactAttachments: IArtifactAttachmentsService,
            dialogService: DialogServiceMock,
            loadingOverlayService: LoadingOverlayServiceMock,
            artifactService: ArtifactServiceMock,
            messageService: MessageServiceMock) => {

            //Arrange
            const files = [<File>{name: "testName1"}];
            const services = new StatefulArtifactServices($q, null, null, messageService, null, null, artifactService, artifactAttachments,
                null, null, loadingOverlayService, null, null, null);
            const artifact = new StatefulArtifact({id: 22, name: "Artifact", version: 0}, services);
            let deferred = $q.defer();

            const documentName = "Document1";
            spyOn(dialogService, "open").and.callFake((dialogSettings: any): ng.IPromise<File[]> => {
                deferred.resolve([files[0]]);
                return deferred.promise;
            });
            onChangesObj.context.currentValue.artifact = artifact;

            //Act
            vm.$onChanges(onChangesObj);
            $rootScope.$digest();
            vm.onFileSelect(files);
            $timeout.flush();

            //Assert
            expect(vm.attachmentsList
                .filter((attachment: IArtifactAttachment) => { return attachment.fileName === files[0].name; }).length)
                .toBeGreaterThan(0);
        }));
});
