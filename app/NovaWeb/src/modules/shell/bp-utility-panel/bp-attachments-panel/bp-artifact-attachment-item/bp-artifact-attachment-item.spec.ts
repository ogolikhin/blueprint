// import "../../../";
// import "angular";
// import "angular-mocks";
// import { ComponentTest } from "../../../../util/component.test";
// import { BPArtifactAttachmentItemController} from "./bp-artifact-attachment-item";
// import { LocalizationServiceMock } from "../../../../core/localization/localization.mock";
// import { Models } from "../../../../main/services/project-manager";
// import { SelectionManager, SelectionSource } from "../../../../main/services/selection-manager";

// describe("Component BP Artifact Attachment Item", () => {


//     beforeEach(angular.mock.module("app.shell"));

//     beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//         $provide.service("localization", LocalizationServiceMock);
//         $provide.service("selectionManager", SelectionManager);
//     }));

//     let componentTest: ComponentTest<BPArtifactAttachmentItemController>;
//     let template = `
//         <bp-artifact-attachment-item 
//             attachment-info="attachment">
//         </bp-artifact-attachment-item>
//     `;
//     let vm: BPArtifactAttachmentItemController;

//     beforeEach(inject(() => {
//         let bindings: any = { 
//             attachment: {
//                 userId: 1,
//                 userName: "admin",
//                 fileName: "test.png",
//                 attachmentId: 1093,
//                 uploadedDate: "2016-06-23T14:54:27.273Z"
//             }
//         };
//         componentTest = new ComponentTest<BPArtifactAttachmentItemController>(template, "bp-artifact-attachment-item");
//         vm = componentTest.createComponent(bindings);
//     }));

//     it("should be visible by default", () => {
//         //Assert
//         expect(componentTest.element.find(".author").length).toBe(1);
//         expect(componentTest.element.find(".button-bar").length).toBe(1);
//         expect(componentTest.element.find("h6").length).toBe(1);
//         expect(componentTest.element.find(".ext-image").length).toBe(1);
//     });

//     it("should try to download an attachment", 
//         inject(($rootScope: ng.IRootScopeService, $window: ng.IWindowService, selectionManager: SelectionManager) => {

//             //Arrange
//             const artifact = { id: 22, name: "Artifact" } as Models.IArtifact;

//             //Act
//             selectionManager.selection = { artifact: artifact, source:  SelectionSource.Explorer };
//             $rootScope.$digest();

//             spyOn($window, "open").and.callFake(() => true);

//             // Act
//             vm.downloadItem();

//             //Assert
//             expect($window.open).toHaveBeenCalled();
//             expect($window.open).toHaveBeenCalledWith("/svc/components/RapidReview/artifacts/22/files/1093?includeDraft=true", "_blank");
//     }));

//     it("should try to delete an attachment", 
//         inject(($window: ng.IWindowService) => {

//         // Arrange
//         spyOn($window, "alert").and.callFake(() => true);

//         // Act
//         vm.deleteItem();

//         //Assert
//         expect($window.alert).toHaveBeenCalled();
//     }));
// });
