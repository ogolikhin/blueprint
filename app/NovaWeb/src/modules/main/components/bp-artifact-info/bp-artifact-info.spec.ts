// import "../../";
// import "../../../shell";
// import "angular";
// import "angular-mocks";
// import "angular-sanitize";

// import { StateManager } from "../../../core/services/state-manager";
// import { Models, Enums} from "../../../main/models";
// import { SessionSvcMock } from "../../../shell/login/mocks.spec";

// import { ComponentTest } from "../../../util/component.test";
// import { BpArtifactInfoController} from "./bp-artifact-info";



// describe("Component BpArtifactInfo", () => {

//     let componentTest: ComponentTest<BpArtifactInfoController>;
//     let template = `<bp-artifact-info context="context"></bp-artifact-info>`;
//     let vm: BpArtifactInfoController;
//     let subscriber;
//     beforeEach(angular.mock.module("app.shell"));
//     beforeEach(angular.mock.module("app.main"));

//     beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//         $provide.service("session", SessionSvcMock);
//         $provide.service("stateManager", StateManager);

//     }));

//     beforeEach(() => {
//         componentTest = new ComponentTest<BpArtifactInfoController>(template, "bp-artifact-info");
//         vm = componentTest.createComponent({});
        
//     });

//     afterEach(() => {
//         vm = null;
//         if (subscriber) {
//             subscriber.dispose();
//         }
//     });

//     it("initial state", () => {
        
//         //Assert
//         expect(componentTest.element.find(".icon").length).toBe(1);
//         expect(componentTest.element.find(".type-id").length).toBe(1);
//         expect(componentTest.element.find(".readonly-indicator").length).toBe(0);
//         expect(componentTest.element.find(".lock-indicator").length).toBe(0);
//         expect(componentTest.element.find(".dirty-indicator").length).toBe(0);
//         expect(vm.artifactType).toBeNull();
//         expect(vm.artifactName).toBeNull();
//         expect(vm.artifactClass).toBeNull();
//         expect(vm.artifactTypeDescription).toBeNull();
//         expect(vm.isChanged).toBeFalsy();
//         expect(vm.isReadonly).toBeFalsy();
//         expect(vm.isLocked).toBeFalsy();
//         expect(vm.isLegacy).toBeFalsy();
//     });

//     it("Artifact with no type description", inject((stateManager: StateManager) => {

//         const artifact = {
//             id: 1,
//             name: "",
//             projectId: 1,
//             predefinedType: Models.ItemTypePredefined.TextualRequirement
//         } as Models.IArtifact;
//         //Act
//         stateManager.addItem(artifact);
//         vm = componentTest.createComponent({});
        
//         //Assert
//         expect(vm.artifactType).toBe("TextualRequirement");
//         expect(vm.artifactClass).toBe("icon-textual-requirement");
//         expect(vm.artifactTypeDescription).toBe("TextualRequirement - 1");

//     }));
//     it("Legacy Artifact", inject((stateManager: StateManager) => {

//         const artifact = {
//             id: 1,
//             name: "",
//             projectId: 1,
//             prefix: "SB_",
//             predefinedType: Models.ItemTypePredefined.Storyboard
//         } as Models.IArtifact;
//         //Act
//         stateManager.addItem(artifact);
//         vm = componentTest.createComponent({});
        
//         //Assert
//         expect(vm.isLegacy).toBeTruthy();
//         expect(vm.artifactType).toBe("Storyboard");
//         expect(vm.artifactClass).toBe("icon-storyboard");
//         expect(vm.artifactTypeDescription).toBe("Storyboard - SB_1");


//     }));


//     it("Artifact with no type description, with prefix", inject((stateManager: StateManager) => {

//         const artifact = {
//             id: 1,
//             name: "",
//             projectId: 1,
//             prefix: "TR_",
//             predefinedType: Models.ItemTypePredefined.TextualRequirement
//         } as Models.IArtifact;
//         //Act
//         stateManager.addItem(artifact);
//         vm = componentTest.createComponent({});
        
//         //Assert
//         expect(vm.artifactType).toBe("TextualRequirement");
//         expect(vm.artifactClass).toBe("icon-textual-requirement");
//         expect(vm.artifactTypeDescription).toBe("TextualRequirement - TR_1");

//     }));


//     it("Artifact with  type description, with prefix", inject((stateManager: StateManager) => {
//         const artifact = {
//             id: 1,
//             name: "Simple",
//             projectId: 1,
//             predefinedType: Models.ItemTypePredefined.TextualRequirement,
//             prefix: "TR_"
//         } as Models.IArtifact;
//         const type = {
//             id: 4444,
//             name: "Textual Requirement",
//             predefinedType: Models.ItemTypePredefined.TextualRequirement,
//         } as Models.IItemType;
//         //Act
//         stateManager.addItem(artifact, type);
//         vm = componentTest.createComponent({});
        
//         //Assert
//         expect(vm.artifactType).toBe("Textual Requirement");
//         expect(vm.artifactClass).toBe("icon-textual-requirement");
//         expect(vm.artifactTypeDescription).toBe("Textual Requirement - TR_1");
        
//     }));

//     it("Artifact state: locked by current user ", inject((stateManager: StateManager) => {
//         const artifact = {
//             id: 1,
//             name: "Simple",
//             projectId: 1,
//             permissions: 8159,
//             lockedByUser: { id: 1 },
//             predefinedType: Models.ItemTypePredefined.TextualRequirement,
//             prefix: "TR_"
//         } as Models.IArtifact;
        
//         //Act
//         stateManager.addItem(artifact);
//         vm = componentTest.createComponent({});
        
//         //Assert
//         expect(vm.isChanged).toBeFalsy();
//         expect(vm.isReadonly).toBeFalsy();
//         expect(vm.selfLocked).toBeTruthy();

//     }));

//     it("Artifact state:locked by other user ", inject((stateManager: StateManager) => {
//         const artifact = {
//             id: 1,
//             name: "Simple",
//             projectId: 1,
//             permissions: 8159,
//             lockedByUser: {id: 2},
//             predefinedType: Models.ItemTypePredefined.TextualRequirement,
//             prefix: "TR_"
//         } as Models.IArtifact;
        
//         //Act
//         stateManager.addItem(artifact);
//         vm = componentTest.createComponent({});
        
//         //Assert
//         expect(vm.isChanged).toBeFalsy();
//         expect(vm.isReadonly).toBeTruthy();
//         expect(vm.lockMessage).toBeDefined();
//         expect(vm.selfLocked).toBeFalsy();

//     }));

//     it("Artifact state: changed ", inject((stateManager: StateManager) => {
//         const artifact = {
//             id: 1,
//             name: "Simple",
//             projectId: 1,
//             permissions: 8159,
//             lockedByUser: { id: 1 },
//             predefinedType: Models.ItemTypePredefined.TextualRequirement,
//             prefix: "TR_"
//         } as Models.IArtifact;
        
//         //Act
//         stateManager.addItem(artifact);
//         stateManager.addChange(artifact, {
//             id: "name",
//             lookup: Enums.PropertyLookupEnum.System,
//             value : "updated"
//         });
//         vm = componentTest.createComponent({});
        
//         //Assert
//         expect(vm.isChanged).toBeTruthy();
//         expect(vm.isReadonly).toBeFalsy();
//         expect(vm.selfLocked).toBeTruthy();


//     }));



// });