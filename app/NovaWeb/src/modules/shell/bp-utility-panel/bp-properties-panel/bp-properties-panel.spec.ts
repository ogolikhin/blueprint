// import "angular";
// import "angular-mocks";
// import "angular-sanitize";
// import "rx/dist/rx.lite";
// import "../../";
// import { ComponentTest } from "../../../util/component.test";
// import { BPPropertiesController } from "./bp-properties-panel";
// import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
// import { ArtifactHistoryMock } from "../bp-history-panel/artifact-history.mock";
// import { ArtifactRelationshipsMock } from "../bp-relationships-panel/artifact-relationships.mock";
// import { ArtifactAttachmentsMock } from "../bp-attachments-panel/artifact-attachments.mock";
// //import { Models } from "../../../main/services/project-manager";
// import { Enums, Models} from "../../../main";
// import { SelectionManager, SelectionSource } from "../../../main/services/selection-manager";
// import { MessageServiceMock } from "../../../core/messages/message.mock";
// import { ArtifactServiceMock } from "../../../main/services/artifact.svc.mock";
// import { StateManagerMock } from "../../../core/services/state-manager.mock";
// import { DialogService } from "../../../shared/widgets/bp-dialog";
// import { WindowManager } from "../../../main";
// import { ProjectManagerMock } from "../../../main/services/project-manager.mock";
// import {PropertyContext} from "../../../editors/bp-artifact/bp-property-context";

// describe("Component BPPropertiesPanel", () => {

//     let componentTest: ComponentTest<BPPropertiesController>;
//     let template = `<bp-properties-panel></bp-properties-panel>`;
//     let ctrl: BPPropertiesController;
//     //let bindings: any;    
//     let bpAccordionPanelController = {
//         isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()    
//     };

//     beforeEach(angular.mock.module("app.shell"));
//     //beforeEach(angular.mock.module("app.main"));

//     beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//         //$provide.service("artifactHistory", ArtifactHistoryMock);
//         //$provide.service("artifactRelationships", ArtifactRelationshipsMock);
//         //$provide.service("artifactAttachments", ArtifactAttachmentsMock);        
//         $provide.service("messageService", MessageServiceMock);        
//         $provide.service("artifactService", ArtifactServiceMock);
//         $provide.service("localization", LocalizationServiceMock);
//         $provide.service("stateManager", StateManagerMock);             
//         $provide.service("selectionManager", SelectionManager);              
//         $provide.service("windowManager", WindowManager);
//         $provide.service("projectManager", ProjectManagerMock);       
//     }));

//     beforeEach(inject(() => {        
//         componentTest = new ComponentTest<BPPropertiesController>(template, "bp-properties-panel");   
//         ctrl = componentTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);  
//     }));
    
//     afterEach( () => {
//         ctrl = null;
//     });
    
//     xit("should load data for a selected artifact",
//         inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager, artifactService: ArtifactServiceMock) => {
//             //Arrange                        
//             const artifact = { id: 22, name: "Artifact", prefix: "My" } as Models.IArtifact;
//             let observerSpy1 = spyOn(artifactService, "getArtifact");            

//             // Act
//             selectionManager.selection = { artifact: artifact, source: SelectionSource.Explorer };            
//             $rootScope.$digest();            
            
//             // Assert
//             expect(observerSpy1).toHaveBeenCalled();
//         }));

//     it("should load data for a selected sub-artifact",
//         inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager, artifactService: ArtifactServiceMock) => {
//             //Arrange                        
//             const artifact = { id: 22, name: "Artifact", prefix: "My" } as Models.IArtifact;
//             const subArtifact = { id: 32, name: "SubArtifact", prefix: "SA" } as Models.ISubArtifact;

//             let observerSpy1 = spyOn(artifactService, "getSubArtifact");            

//             // Act
//             selectionManager.selection = { artifact: artifact, subArtifact: subArtifact, source: SelectionSource.Explorer };
//             $rootScope.$digest();            

//             // Assert
//             expect(observerSpy1).toHaveBeenCalled();            
//         }));


//     it("not load properties for undefined artifact",
//         inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager, projectManager: ProjectManagerMock) => {
//             //Arrange                                    
//             const subArtifact = { id: 32, name: "SubArtifact", prefix: "SA" } as Models.ISubArtifact;
//             let observerSpy1 = spyOn(projectManager, "getArtifactPropertyTypes");            

//             // Act
//             ctrl.onUpdate(undefined, subArtifact);
            
//             // Assert            
//             expect(observerSpy1).not.toHaveBeenCalled();
//         }));

//     it("load properties for artifact",
//         inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager, projectManager: ProjectManagerMock) => {
//             //Arrange                  
//             const artifact = { id: 22, name: "Artifact", prefix: "My" } as Models.IArtifact;
//             const subArtifact = { id: 32, name: "SubArtifact", prefix: "SA" } as Models.ISubArtifact;            
//             ctrl.customFields = [];
//             expect(ctrl.editor.propertyContexts).toBeFalsy();

//             // Act
//             ctrl.onUpdate(artifact, subArtifact);

//             // Assert                        
//             expect(ctrl.editor.propertyContexts).toBeTruthy();
//         }));

//     it("on field update for plain text",
//         inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager, projectManager: ProjectManagerMock) => {
//             //Arrange           
//             let pt: Models.IPropertyType = {
//                 id: 1,
//                 versionId: 1,
//                 name: "",
//                 primitiveType: undefined,
//                 instancePropertyTypeId: 0,
//                 isRichText: false,
//                 decimalDefaultValue: 0,
//                 dateDefaultValue: "",
//                 userGroupDefaultValue: undefined,
//                 stringDefaultValue: "",
//                 decimalPlaces: 0,
//                 maxNumber: 0,
//                 minNumber: 0,
//                 maxDate: "",
//                 minDate: "",
//                 isMultipleAllowed: false,
//                 isRequired: false,
//                 isValidated: false,
//                 validValues: undefined,
//                 defaultValidValueId: 0,                   
//                 propertyTypePredefined: Enums.PropertyTypePredefined.Description,
//                 disabled: false
//             };
//             let pc: PropertyContext = new PropertyContext(pt);
//             let field: AngularFormly.IFieldConfigurationObject = { data: pc };
//             ctrl.systemFields = [];

//             // Act
//             ctrl.onFieldUpdate(field);

//             // Assert                        
//             expect(ctrl.systemFields[0]).toBeTruthy();
//         }));

//     it("on field update for rich text",
//         inject(($rootScope: ng.IRootScopeService, selectionManager: SelectionManager, projectManager: ProjectManagerMock) => {
//             //Arrange           
//             let pt: Models.IPropertyType = {
//                 id: 1,
//                 versionId: 1,
//                 name: "",
//                 primitiveType: undefined,
//                 instancePropertyTypeId: 0,
//                 isRichText: true,
//                 decimalDefaultValue: 0,
//                 dateDefaultValue: "",
//                 userGroupDefaultValue: undefined,
//                 stringDefaultValue: "",
//                 decimalPlaces: 0,
//                 maxNumber: 0,
//                 minNumber: 0,
//                 maxDate: "",
//                 minDate: "",
//                 isMultipleAllowed: false,
//                 isRequired: false,
//                 isValidated: false,
//                 validValues: undefined,
//                 defaultValidValueId: 0,
//                 propertyTypePredefined: Enums.PropertyTypePredefined.Description,
//                 disabled: false
//             };
//             let pc: PropertyContext = new PropertyContext(pt);
//             let field: AngularFormly.IFieldConfigurationObject = { data: pc };
//             ctrl.richTextFields = [];

//             // Act
//             ctrl.onFieldUpdate(field);

//             // Assert                        
//             expect(ctrl.richTextFields[0]).toBeTruthy();
//         }));
// });
