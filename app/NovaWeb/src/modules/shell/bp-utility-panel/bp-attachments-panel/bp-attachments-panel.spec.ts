import "../../";
import "angular";
import "angular-mocks";
import "angular-sanitize";
import { ComponentTest } from "../../../util/component.test";
import { BPAttachmentsPanelController } from "./bp-attachments-panel";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ArtifactAttachmentsMock } from "./artifact-attachments.mock";
import { IArtifactAttachmentsResultSet, IArtifactDocRef } from "./artifact-attachments.svc";
import { Models } from "../../../main/services/project-manager";
import { SelectionManager, SelectionSource } from "../../../main/services/selection-manager";
import { DialogService } from "../../../shared/widgets/bp-dialog";
import { SessionSvcMock } from "../../login/mocks.spec";

describe("Component BP Attachments Panel", () => {

    let componentTest: ComponentTest<BPAttachmentsPanelController>;
    let template = `<bp-attachments-panel></bp-attachments-panel>`;
    let vm: BPAttachmentsPanelController;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("dialogService", DialogService);
        $provide.service("session", SessionSvcMock);
    }));

    beforeEach(inject(() => {
        componentTest = new ComponentTest<BPAttachmentsPanelController>(template, "bp-attachments-panel");
        vm = componentTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
    }));
    
    afterEach( () => {
        vm = null;
    });

    it("should be visible by default", () => {
        //Assert
        expect(componentTest.element.find(".empty-state").length).toBe(1);
    });

    it("should load data and display it for a selected artifact", 
        inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager) => {
            
            //Arrange
            const artifact = { id: 22, name: "Artifact", prefix: "PRO" } as Models.IArtifact;
            
            //Act
            selectionManager.selection = { artifact: artifact, source:  SelectionSource.Explorer };
            $rootScope.$digest();
            const selectedArtifact = selectionManager.selection.artifact;

            //Assert
            expect(selectedArtifact).toBeDefined();
            expect(vm.artifactAttachmentsList).toBeDefined();
            expect(vm.artifactAttachmentsList.attachments.length).toBe(7);
            expect(vm.artifactAttachmentsList.documentReferences.length).toBe(3);
            expect(componentTest.element.find("bp-artifact-attachment-item").length).toBe(7);
            expect(componentTest.element.find("bp-artifact-document-item").length).toBe(3);
            expect(componentTest.element.find(".empty-state").length).toBe(0);
        }));

    it("should not load data for artifact without Prefix",
        inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager) => {

            //Arrange
            const artifact = { id: 22, name: "Artifact" } as Models.IArtifact;

            //Act
            selectionManager.selection = { artifact: artifact, source: SelectionSource.Explorer };
            $rootScope.$digest();
            const selectedArtifact = selectionManager.selection.artifact;

            //Assert
            expect(selectedArtifact).toBeDefined();
            expect(vm.artifactAttachmentsList).toBe(null);
        }));

    it("addDocRef should add new document reference to the list",
        inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager, $q: ng.IQService, $timeout: ng.ITimeoutService) => {
            //Arrange
            const artifact = { id: 22, name: "TestDocument", prefix: "PRO" } as Models.IArtifact;
            let deferred = $q.defer();
            DialogService.prototype.open = jasmine.createSpy("open() spy").and.callFake(
                (): ng.IPromise<Models.IArtifact> => {
                    deferred.resolve(artifact);
                    return deferred.promise;
                }
            );

            //Act
            selectionManager.selection = { artifact: artifact, source: SelectionSource.Explorer };
            $rootScope.$digest();
            vm.addDocRef();
            $timeout.flush();

            //Assert
            expect(vm.artifactAttachmentsList.documentReferences
                .filter((doc: IArtifactDocRef) => { return doc.artifactName === artifact.name; }).length)
                .toBeGreaterThan(0);
        }));

    it("the list should be empty when service throwing exception",
        inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager, $q: ng.IQService, $timeout: ng.ITimeoutService) => {

            //Arrange
            const artifact = { id: 22, name: "Artifact", prefix: "PRO" } as Models.IArtifact;
            let deferred = $q.defer();
            ArtifactAttachmentsMock.prototype.getArtifactAttachments = jasmine.createSpy("getArtifactAttachments() spy").and.callFake(
                (): ng.IPromise<IArtifactAttachmentsResultSet> => {
                    deferred.reject({
                        statusCode: 404,
                        errorCode: 2000
                    });
                    return deferred.promise;
                }
            );

            //Act
            selectionManager.selection = { artifact: artifact, source: SelectionSource.Explorer };
            $rootScope.$digest();
            $timeout.flush();

            //Assert
            expect(vm.artifactAttachmentsList).toBe(null);
            expect(vm.canAddNewFile()).toBe(false);
        }));
});
