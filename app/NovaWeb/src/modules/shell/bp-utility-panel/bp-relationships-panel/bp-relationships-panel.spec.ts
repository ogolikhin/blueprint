// import "angular";
// import "angular-mocks";
// import "angular-sanitize";
// import "Rx";
// import "../../";
// import { ComponentTest } from "../../../util/component.test";
// import { BPRelationshipsPanelController } from "./bp-relationships-panel";
// import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
// import { ArtifactRelationshipsMock } from "./../../../managers/artifact-manager/relationships/relationships.svc.mock";
// import { MessageServiceMock } from "../../../core/messages/message.mock";
// import { SelectionManager } from "./../../../managers/selection-manager/selection-manager";
// import { DialogService } from "../../../shared/widgets/bp-dialog";
// import {
//     IArtifactManager,
//     ArtifactManager,
//     IStatefulArtifactFactory,
//     StatefulArtifactFactory,
//     MetaDataService,
//     ArtifactService,
//     ArtifactAttachmentsService 
// }
// from "../../../managers/artifact-manager";

// describe("Component BPRelationshipsPanel", () => {

//     let directiveTest: ComponentTest<BPRelationshipsPanelController>;
//     let template = `<bp-relationships-panel></bp-relationships-panel>`;
//     let vm: BPRelationshipsPanelController;
//     let bpAccordionPanelController = {
//         isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
//     };

//     beforeEach(angular.mock.module("app.shell"));

//     beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//         $provide.service("artifactRelationships", ArtifactRelationshipsMock);
//         $provide.service("localization", LocalizationServiceMock);
//         $provide.service("dialogService", DialogService);
//         $provide.service("selectionManager", SelectionManager);
//         $provide.service("messageService", MessageServiceMock);
//         $provide.service("artifactService", ArtifactService);
//         $provide.service("artifactManager", ArtifactManager);
//         $provide.service("artifactAttachments", ArtifactAttachmentsService);
//         $provide.service("metadataService", MetaDataService);
//         $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
//     }));

//     beforeEach(inject(() => {
//         directiveTest = new ComponentTest<BPRelationshipsPanelController>(template, "bp-relationships-panel");
//         vm = directiveTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
//     }));

//     afterEach(() => {
//         vm = null;
//     });

//     it("should be visible by default", () => {
//         //Assert
//         expect(directiveTest.element.find(".filter-bar").length).toBe(0);
//         expect(directiveTest.element.find(".empty-state").length).toBe(1);
//     });

//     it("should load data for a selected artifact",
//         inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory) => {

//             //Arrange
//             const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "AC"});

//             //Act
//             artifactManager.selection.setArtifact(artifact);
//             $rootScope.$digest();
//             const selectedArtifact = artifactManager.selection.getArtifact();

//             //Assert
//             expect(selectedArtifact).toBeDefined();
//             expect(vm.manualTraces.length).toBe(2);
//             expect(vm.otherTraces.length).toBe(3);
//         }));

//     it("should not load data for artifact without Prefix",
//         inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager, statefulArtifactFactory: IStatefulArtifactFactory) => {

//             //Arrange
//             const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact"});

//             //Act
//             artifactManager.selection.setArtifact(artifact);
//             $rootScope.$digest();
//             const selectedArtifact = artifactManager.selection.getArtifact();

//             //Assert
//             expect(selectedArtifact).toBeDefined();
//             expect(vm.manualTraces).toBe(null);
//             expect(vm.otherTraces).toBe(null);
//         }));
// });
