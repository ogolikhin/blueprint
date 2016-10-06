import * as angular from "angular";
import "angular-mocks";
import "angular-sanitize";
import "rx/dist/rx.lite";
import "../../";
import { ComponentTest } from "../../../util/component.test";
import { BPPropertiesController } from "./bp-properties-panel";
import { LocalizationServiceMock } from "../../../core/localization/localization.mock";
import { ArtifactRelationshipsMock } from "./../../../managers/artifact-manager/relationships/relationships.svc.mock";
import { ArtifactAttachmentsMock } from "./../../../managers/artifact-manager/attachments/attachments.svc.mock";
import { ArtifactServiceMock } from "./../../../managers/artifact-manager/artifact/artifact.svc.mock";
import { DialogServiceMock } from "../../../shared/widgets/bp-dialog/bp-dialog";
import { ProcessServiceMock } from "../../../editors/bp-process/services/process.svc.mock";
//import { Models } from "../../../main/services/project-manager";
import { Enums, Models} from "../../../main";
import { SelectionManager } from "./../../../managers/selection-manager/selection-manager";
import { MessageServiceMock } from "../../../core/messages/message.mock";
import { DialogService } from "../../../shared/widgets/bp-dialog";
import {PropertyContext} from "../../../editors/bp-artifact/bp-property-context";import {
    IArtifactManager,
    ArtifactManager,
    IStatefulArtifactFactory,
    StatefulArtifactFactory,
    MetaDataService
} from "../../../managers/artifact-manager";

describe("Component BPPropertiesPanel", () => {

    let componentTest: ComponentTest<BPPropertiesController>;
    let template = `<bp-properties-panel></bp-properties-panel>`;
    let ctrl: BPPropertiesController;
    //let bindings: any;    
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()    
    };

    beforeEach(angular.mock.module("app.shell"));
    //beforeEach(angular.mock.module("app.main"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("processService", ProcessServiceMock);
    }));

    beforeEach(inject(() => {        
        componentTest = new ComponentTest<BPPropertiesController>(template, "bp-properties-panel");   
        ctrl = componentTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);  
    }));
    
    afterEach( () => {
        ctrl = null;
    });
    
    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService, 
                statefulArtifactFactory: IStatefulArtifactFactory, 
                artifactManager: IArtifactManager, 
                artifactService: ArtifactServiceMock, 
                metadataService: MetaDataService) => {
            //Arrange                        
            const artifactModel = { id: 22, name: "Artifact", prefix: "My", predefinedType: Models.ItemTypePredefined.Actor } as Models.IArtifact;
            let observerSpy1 = spyOn(artifactService, "getArtifact").and.callThrough();            
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);
            // Act
            artifactManager.selection.setArtifact(artifact);       
            $rootScope.$digest();            
            
            // Assert
            expect(observerSpy1).toHaveBeenCalled();
        }));

    it("should load data for a selected sub-artifact",
        inject(($rootScope: ng.IRootScopeService, 
                statefulArtifactFactory: IStatefulArtifactFactory, 
                artifactManager: IArtifactManager, 
                artifactService: ArtifactServiceMock) => {
            //Arrange                        
            const artifactModel = { id: 22, name: "Artifact", prefix: "My", predefinedType: Models.ItemTypePredefined.Process } as Models.IArtifact;
            const subArtifactModel = { id: 32, name: "SubArtifact", prefix: "SA", predefinedType: Models.ItemTypePredefined.PROShape } as Models.ISubArtifact;

            let observerSpy1 = spyOn(artifactService, "getSubArtifact").and.callThrough();      

            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);    
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);  

            // Act
            artifactManager.selection.setSubArtifact(subArtifact);       
            $rootScope.$digest();            

            // Assert
            expect(observerSpy1).toHaveBeenCalled();            
    }));


    it("not load properties for undefined artifact",
        inject((
                $rootScope: ng.IRootScopeService, 
                artifactManager: IArtifactManager, 
                metadataService: MetaDataService
                ) => {
            //Arrange                                    
            const subArtifact = { id: 32, name: "SubArtifact", prefix: "SA", predefinedType: Models.ItemTypePredefined.PROShape  } as Models.ISubArtifact;
            let observerSpy1 = spyOn(metadataService, "getArtifactPropertyTypes").and.callThrough();            

            // Act
            artifactManager.selection.setArtifact(undefined);
            
            // Assert            
            expect(observerSpy1).not.toHaveBeenCalled();
        }));

    it("load properties for artifact",
        inject((
            $rootScope: ng.IRootScopeService, 
            statefulArtifactFactory: IStatefulArtifactFactory, 
            artifactManager: IArtifactManager
            ) => {
            //Arrange                  
            const artifactModel = { id: 22, name: "Artifact", prefix: "My", predefinedType: Models.ItemTypePredefined.Process } as Models.IArtifact;      
            ctrl.customFields = [];
            expect(ctrl.editor.propertyContexts).toBeFalsy();
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);    

            // Act
            artifactManager.selection.setArtifact(artifact);       
            $rootScope.$digest();            

            // Assert                        
            expect(ctrl.editor.propertyContexts).toBeTruthy();
        }));

    it("on field update for plain text",
        inject(($rootScope: ng.IRootScopeService) => {
            //Arrange           
            let pt: Models.IPropertyType = {
                id: 1,
                versionId: 1,
                name: "",
                primitiveType: undefined,
                instancePropertyTypeId: 0,
                isRichText: false,
                decimalDefaultValue: 0,
                dateDefaultValue: "",
                userGroupDefaultValue: undefined,
                stringDefaultValue: "",
                decimalPlaces: 0,
                maxNumber: 0,
                minNumber: 0,
                maxDate: "",
                minDate: "",
                isMultipleAllowed: false,
                isRequired: false,
                isValidated: false,
                validValues: undefined,
                defaultValidValueId: 0,                   
                propertyTypePredefined: Enums.PropertyTypePredefined.Description,
                disabled: false
            };
            let pc: PropertyContext = new PropertyContext(pt);
            let field: AngularFormly.IFieldConfigurationObject = { data: pc };
            ctrl.systemFields = [];

            // Act
            ctrl.onFieldUpdate(field);

            // Assert                        
            expect(ctrl.systemFields[0]).toBeTruthy();
        }));

    it("on field update for rich text",
        inject(($rootScope: ng.IRootScopeService) => {
            //Arrange           
            let pt: Models.IPropertyType = {
                id: 1,
                versionId: 1,
                name: "",
                primitiveType: undefined,
                instancePropertyTypeId: 0,
                isRichText: true,
                decimalDefaultValue: 0,
                dateDefaultValue: "",
                userGroupDefaultValue: undefined,
                stringDefaultValue: "",
                decimalPlaces: 0,
                maxNumber: 0,
                minNumber: 0,
                maxDate: "",
                minDate: "",
                isMultipleAllowed: false,
                isRequired: false,
                isValidated: false,
                validValues: undefined,
                defaultValidValueId: 0,
                propertyTypePredefined: Enums.PropertyTypePredefined.Description,
                disabled: false
            };
            let pc: PropertyContext = new PropertyContext(pt);
            let field: AngularFormly.IFieldConfigurationObject = { data: pc };
            ctrl.richTextFields = [];

            // Act
            ctrl.onFieldUpdate(field);

            // Assert                        
            expect(ctrl.richTextFields[0]).toBeTruthy();
        }));

    it("should return correct property types for a selected sub-artifact",
        inject(($q: ng.IQService,
                $rootScope: ng.IRootScopeService, 
                statefulArtifactFactory: IStatefulArtifactFactory, 
                artifactManager: IArtifactManager, 
                artifactService: ArtifactServiceMock,
                metadataService: MetaDataService) => {
                 //Arrange                        
            const artifactModel = { id: 22, name: "Artifact", prefix: "My", predefinedType: Models.ItemTypePredefined.BusinessProcess } as Models.IArtifact;
            const subArtifactModel = { id: 32, name: "SubArtifact", prefix: "SA", predefinedType: Models.ItemTypePredefined.BPShape } as Models.ISubArtifact;

          
            spyOn(metadataService, "getSubArtifactItemType").and.returnValue(
                {   predefinedType: Models.ItemTypePredefined.BPShape,
                    customProperties: []                        
                }
             );   

            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);    
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);  

            // Act
            artifactManager.selection.setArtifact(artifact);            
            $rootScope.$digest();            
            artifactManager.selection.setSubArtifact(subArtifact);             
            $rootScope.$digest();               

            // Assert
                
            let propertyContexts: PropertyContext[] = ctrl.specificFields.map(a=>a.data as PropertyContext);
            let model: any = ctrl.model;
            expect(propertyContexts.filter(a=>a.name === "Label_X").length).toBe(1);
            expect(propertyContexts.filter(a=>a.name === "Label_Y").length).toBe(1);
            expect(propertyContexts.filter(a=>a.name === "Label_Width").length).toBe(1);
            expect(propertyContexts.filter(a=>a.name === "Label_Height").length).toBe(1);
    }));
    it("should contain populated model data for a selected sub-artifact",
        inject(($q: ng.IQService,
                $rootScope: ng.IRootScopeService, 
                statefulArtifactFactory: IStatefulArtifactFactory, 
                artifactManager: IArtifactManager, 
                artifactService: ArtifactServiceMock,
                metadataService: MetaDataService) => {
            //Arrange                        
            const artifactModel = { id: 22, name: "Artifact", prefix: "My", predefinedType: Models.ItemTypePredefined.BusinessProcess } as Models.IArtifact;
            const subArtifactModel = { id: 32, name: "SubArtifact", prefix: "SA", predefinedType: Models.ItemTypePredefined.BPShape } as Models.ISubArtifact;

            let x = 1;
            let y = 2;
            let width = 100;
            let height = 200;

            let xPropertyValue:Models.IPropertyValue = ArtifactServiceMock.createSpecificPropertyValues(1, x, Models.PropertyTypePredefined.X);
            let yPropertyValue:Models.IPropertyValue = ArtifactServiceMock.createSpecificPropertyValues(1, y, Models.PropertyTypePredefined.Y);
            let widthPropertyValue:Models.IPropertyValue = ArtifactServiceMock.createSpecificPropertyValues(1, width, Models.PropertyTypePredefined.Width);
            let heightPropertyValue:Models.IPropertyValue = ArtifactServiceMock.createSpecificPropertyValues(1, height, Models.PropertyTypePredefined.Height);
            
            spyOn(metadataService, "getSubArtifactItemType").and.returnValue(
                {   predefinedType: Models.ItemTypePredefined.BPShape,
                    customProperties: []                        
                }
             );   
             
             spyOn(artifactService, "getSubArtifact").and.callFake( (artifactId: number, subArtifactId: number)=>{
                    let model = {   predefinedType: Models.ItemTypePredefined.BPShape,
                        specificPropertyValues: [xPropertyValue, yPropertyValue, widthPropertyValue, heightPropertyValue]                  
                    };
                    var deferred = $q.defer<any>();
                    deferred.resolve(model);
                    return deferred.promise;
                    
                }
             );  

            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);    
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);  

            // Act
            artifactManager.selection.setArtifact(artifact);            
            $rootScope.$digest();            
            artifactManager.selection.setSubArtifact(subArtifact);             
            $rootScope.$digest();               

            // Assert                
            let model: any = ctrl.model;
            expect(model.x).toBeDefined();
            expect(model.y).toBeDefined();
            expect(model.width).toBeDefined();
            expect(model.height).toBeDefined();
            expect(model.x).toBe(x);
            expect(model.y).toBe(y);
            expect(model.width).toBe(width);
            expect(model.height).toBe(height);
        }));
    it("should not display properties for a selected process shape ",
        inject(($q: ng.IQService,
                $rootScope: ng.IRootScopeService, 
                statefulArtifactFactory: IStatefulArtifactFactory, 
                artifactManager: IArtifactManager, 
                artifactService: ArtifactServiceMock,
                metadataService: MetaDataService) => {
            //Arrange                        
            const artifactModel = { id: 22, name: "Artifact", prefix: "My", predefinedType: Models.ItemTypePredefined.Process } as Models.IArtifact;
            const subArtifactModel = { id: 32, name: "SubArtifact", prefix: "SA", predefinedType: Models.ItemTypePredefined.PROShape } as Models.ISubArtifact;

            let x = 1;
            let y = 2;
            let width = 100;
            let height = 200;
            let xPropertyValue:Models.IPropertyValue = ArtifactServiceMock.createSpecificPropertyValues(1, x, Models.PropertyTypePredefined.X);
            let yPropertyValue:Models.IPropertyValue = ArtifactServiceMock.createSpecificPropertyValues(1, y, Models.PropertyTypePredefined.Y);
            let widthPropertyValue:Models.IPropertyValue = ArtifactServiceMock.createSpecificPropertyValues(1, width, Models.PropertyTypePredefined.Width);
            let heightPropertyValue:Models.IPropertyValue = ArtifactServiceMock.createSpecificPropertyValues(1, height, Models.PropertyTypePredefined.Height);
            
            spyOn(metadataService, "getSubArtifactItemType").and.returnValue(
                {   predefinedType: Models.ItemTypePredefined.PROShape,    
                    customProperties: []                          
                }
             );     
  
             spyOn(artifactService, "getSubArtifact").and.callFake( (artifactId: number, subArtifactId: number)=>{
                    let model = {   predefinedType: Models.ItemTypePredefined.PROShape,
                        specificPropertyValues: [xPropertyValue, yPropertyValue, widthPropertyValue, heightPropertyValue]                  
                    };
                    var deferred = $q.defer<any>();
                    deferred.resolve(model);
                    return deferred.promise;
                    
                }
             );  
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);    
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);  

            // Act
            artifactManager.selection.setArtifact(artifact);            
            $rootScope.$digest();            
            artifactManager.selection.setSubArtifact(subArtifact);       
            $rootScope.$digest();            

            // Assert                
            let propertyContexts: PropertyContext[] = ctrl.specificFields.map(a=>a.data as PropertyContext);
            
            expect(propertyContexts.filter(a=>a.name === "Label_X").length).toBe(0);
            expect(propertyContexts.filter(a=>a.name === "Label_Y").length).toBe(0);
            expect(propertyContexts.filter(a=>a.name === "Label_Width").length).toBe(0);
            expect(propertyContexts.filter(a=>a.name === "Label_Height").length).toBe(0);
        }));
});
