import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "../../../";
import {ComponentTest} from "../../../../util/component.test";
import {BPDocumentItemController} from "./bp-document-item";
import {LocalizationServiceMock} from "../../../../core/localization/localization.mock";
import {IArtifactAttachmentsService} from "../../../../managers/artifact-manager";
import {ArtifactAttachmentsMock} from "../../../../managers/artifact-manager/attachments/attachments.svc.mock";
import {IMessageService} from "../../../../core/messages/message.svc";

describe("Component BP Artifact Document Item", () => {
    const template = `<bp-document-item doc-ref-info="document" delete-item="delete()"></bp-document-item>`;
    let component: ComponentTest<BPDocumentItemController>;
    let vm: BPDocumentItemController;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
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
            $window: ng.IWindowService) => {

            // Arrange
            spyOn($window, "open").and.callFake(function () {
                return true;
            });

            // Act
            vm.downloadItem();
            $timeout.flush();

            //Assert
            expect($window.open).toHaveBeenCalled();
            expect($window.open).toHaveBeenCalledWith("/svc/bpartifactstore/artifacts/306/attachments/1093", "_blank");
        }));

    it("should try to download a document which has an historical attachment",
        inject((
            $timeout: ng.ITimeoutService,
            artifactAttachments: IArtifactAttachmentsService,
            $window: ng.IWindowService) => {

            // Arrange
            spyOn($window, "open").and.callFake(function () {
                return true;
            });
            vm.docRefInfo.versionId = 4;

            // Act
            vm.downloadItem();
            $timeout.flush();

            //Assert
            expect($window.open).toHaveBeenCalled();
            expect($window.open).toHaveBeenCalledWith("/svc/bpartifactstore/artifacts/306/attachments/1093?versionId=4", "_blank");
        }));

    it("should try to download a document which has no attachment",
        inject((
            $timeout: ng.ITimeoutService,
            $window: ng.IWindowService,
            messageService: IMessageService,
            artifactAttachments: IArtifactAttachmentsService,
            $q: ng.IQService) => {

            // Arrange
            spyOn($window, "open").and.callFake(() => true);
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
            expect($window.open).not.toHaveBeenCalled();
            expect(messageService.addError).toHaveBeenCalled();
        }));

    it("should try to delete an item",
        inject(($rootScope: ng.IRootScopeService, $window: ng.IWindowService) => {
            // Arrange
            spyOn($window, "alert").and.callFake(() => true);

            // Act
            vm.deleteItem();

            //Assert
            expect($window.alert).toHaveBeenCalled();
        }));

    it("should navigate to document",
        inject(($rootScope: ng.IRootScopeService, $state: ng.ui.IStateService, $timeout: ng.ITimeoutService) => {

        //Arrange
        const routerSpy = spyOn($state, "go");

        //Act
        // vm.navigateToDocumentReference(5);
        component.element.find("a").click();
        $timeout.flush();

        // Assert
        expect(routerSpy).toHaveBeenCalled();
        expect(routerSpy).toHaveBeenCalledWith("main.item", {id: 357}, jasmine.any(Object));
    }));
});
