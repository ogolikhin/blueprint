// import {ProcessServiceMock} from "./services/process.svc.mock";
// import * as TestModels from "./models/test-model-factory";
// import {SelectionManager} from "../../main/services";
// import {CommunicationManager} from "./";
// import {StateManagerMock} from "../../core/services/state-manager.mock";
// import {WindowResize} from "../../core/services/window-resize";
// import {MessageServiceMock} from "../../core/messages/message.mock";
// import {LocalizationServiceMock} from "../../core/localization/localization.mock";
// import {WindowManager} from "../../main";
// import {ProjectManagerMock} from "../../main/services/project-manager.mock";
// import {BpProcessEditorController} from "./bp-process-editor";
// import { ComponentTest } from "../../util/component.test";
// import { DialogServiceMock } from "../../shared/widgets/bp-dialog/bp-dialog";
// import { ArtifactServiceMock } from "../../main/services/artifact.svc.mock";

// describe("BpProcessEditorController Tests", () => {
//     beforeEach(angular.mock.module("bp.editors.process"));

//     beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//         $provide.service("artifactService", ArtifactServiceMock);
//         $provide.service("dialogService", DialogServiceMock);
//         $provide.service("processModelService", ProcessServiceMock);
//         $provide.service("messageService", MessageServiceMock);
//         $provide.service("selectionManager", SelectionManager);
//         $provide.service("localization", LocalizationServiceMock);
//         $provide.service("stateManager", StateManagerMock);
//         $provide.service("windowManager", WindowManager);
//         $provide.service("projectManager", ProjectManagerMock);
//         $provide.service("windowResize", WindowResize);
//         $provide.service("communicationManager", CommunicationManager);
//     }));

//     let componentTest: ComponentTest<BpProcessEditorController>;
//     let template = `<bp-process-editor context="context"></bp-process-editor>`;
//     let vm: BpProcessEditorController;
//     let bindings = {
//         context: {
//             artifact: {
//                 id: 1
//             },
//             type: {
//             }
//         }
//     };

//     let httpBackend;
//     beforeEach(
//         inject(
//             ($httpBackend: ng.IHttpBackendService) => {
//                 httpBackend = $httpBackend;
//             }
//         )
//     );

//     afterEach(() => {
//         httpBackend.verifyNoOutstandingExpectation();
//         httpBackend.verifyNoOutstandingRequest();
//     });

//     it("Initilize bp-process-editor successfully", () => {
//         // arrange
//         let model = TestModels.createDefaultProcessModel();
//         httpBackend.expectGET("/svc/components/storyteller/processes/1")
//             .respond(model);

//         // act
//         componentTest = new ComponentTest<BpProcessEditorController>(template, "bp-process-editor");
//         vm = componentTest.createComponent(bindings);
//         httpBackend.flush();
//         // assert
//         expect(vm.subArtifactEditorModalOpener).not.toBeNull();
//         expect(vm.processDiagram).not.toBeNull();
//     });
// });