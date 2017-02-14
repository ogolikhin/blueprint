import "angular-mocks";
import "angular-sanitize";
import "rx/dist/rx.lite";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {createDefaultProcessModel} from "../../../editorsModule/bp-process/models/test-model-factory";
import {INovaProcess} from "../../../editorsModule/bp-process/process-artifact";
import {IPropertyDescriptor, PropertyDescriptorBuilderMock} from "../../../editorsModule/services";
import {UnpublishedArtifactsServiceMock} from "../../../editorsModule/unpublished/unpublished.service.mock";
import {Enums, Models} from "../../../main";
import {MessageServiceMock} from "../../../main/components/messages/message.mock";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {IStatefulArtifactFactory, MetaDataService, StatefulArtifactFactory} from "../../../managers/artifact-manager";
import {ArtifactServiceMock} from "../../../managers/artifact-manager/artifact/artifact.svc.mock";
import {ArtifactAttachmentsMock} from "../../../managers/artifact-manager/attachments/attachments.svc.mock";
import {ArtifactRelationshipsMock} from "../../../managers/artifact-manager/relationships/relationships.svc.mock";
import {ValidationServiceMock} from "../../../managers/artifact-manager/validation/validation.mock";
import {ISelectionManager, SelectionManager} from "../../../managers/selection-manager/selection-manager";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {ComponentTest} from "../../../util/component.test";
import {LicenseServiceMock} from "../../license/license.svc.mock";
import {SessionSvcMock} from "../../login/session.svc.mock";
import {IOnPanelChangesObject, PanelType} from "../utility-panel.svc";
import {BPPropertiesController} from "./bp-properties-panel";
import * as angular from "angular";

describe("Component BPPropertiesPanel", () => {

    let componentTest: ComponentTest<BPPropertiesController>;
    let template = `<bp-properties-panel></bp-properties-panel>`;
    let ctrl: BPPropertiesController;
    //let bindings: any;
    let bpAccordionPanelController = {
        isActiveObservable: new Rx.BehaviorSubject<boolean>(true).asObservable()
    };
    let onChangesObj: IOnPanelChangesObject;

    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("artifactRelationships", ArtifactRelationshipsMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactService", ArtifactServiceMock);
        $provide.service("artifactAttachments", ArtifactAttachmentsMock);
        $provide.service("metadataService", MetaDataService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("publishService", UnpublishedArtifactsServiceMock);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
        $provide.service("validationService", ValidationServiceMock);
        $provide.service("licenseService", LicenseServiceMock);
        $provide.service("session", SessionSvcMock);
    }));

    beforeEach(inject(() => {
        componentTest = new ComponentTest<BPPropertiesController>(template, "bp-properties-panel");
        ctrl = componentTest.createComponentWithMockParent({}, "bpAccordionPanel", bpAccordionPanelController);
        onChangesObj = {
            context: {
                currentValue: {
                    panelType: PanelType.Properties
                },
                previousValue: undefined,
                isFirstChange: () => { return true; }
            }
        };
    }));

    let metaDataServiceGetMethodSpy;

    beforeEach(inject(($q: ng.IQService, metadataService: MetaDataService) => {
        const deferred = $q.defer<any>();
        deferred.resolve({
            data: {
                artifactTypes: [],
                subArtifactTypes: [],
                propertyTypes: []
            }
        });
        metaDataServiceGetMethodSpy = spyOn(metadataService, "get");
        metaDataServiceGetMethodSpy.and.returnValue(deferred.promise);
    }));

    afterEach(() => {
        ctrl = undefined;
        onChangesObj = undefined;
    });

    it("should load data for a selected artifact",
        inject(($rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                artifactService: ArtifactServiceMock,
                metadataService: MetaDataService) => {
            //Arrange
            const artifactModel = {
                id: 22,
                name: "Artifact",
                prefix: "My",
                predefinedType: ItemTypePredefined.Actor
            } as Models.IArtifact;
            const observerSpy1 = spyOn(artifactService, "getArtifact").and.callThrough();
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);
            onChangesObj.context.currentValue.artifact = artifact;
            // Act
            ctrl.$onChanges(onChangesObj);
            $rootScope.$digest();

            // Assert
            expect(observerSpy1).toHaveBeenCalled();
        }));

    it("should load data for a selected sub-artifact",
        inject(($rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                artifactService: ArtifactServiceMock) => {
            //Arrange
            const artifactModel = {
                id: 22,
                name: "Artifact",
                prefix: "My",
                predefinedType: ItemTypePredefined.Process
            } as Models.IArtifact;
            const subArtifactModel = {
                id: 32,
                name: "SubArtifact",
                prefix: "SA",
                predefinedType: ItemTypePredefined.PROShape
            } as Models.ISubArtifact;

            const observerSpy1 = spyOn(artifactService, "getSubArtifact").and.callThrough();

            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);
            onChangesObj.context.currentValue.artifact = artifact;
            onChangesObj.context.currentValue.subArtifact = subArtifact;

            // Act
            ctrl.$onChanges(onChangesObj);
            $rootScope.$digest();

            // Assert
            expect(observerSpy1).toHaveBeenCalled();
        }));


    it("not load properties for undefined artifact",
        inject(($rootScope: ng.IRootScopeService,
                metadataService: MetaDataService,
                statefulArtifactFactory: IStatefulArtifactFactory) => {
            //Arrange
            const artifactModel = {
                id: 22,
                name: "Artifact",
                prefix: "My",
                predefinedType: ItemTypePredefined.Process
            } as Models.IArtifact;
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);
            const subArtifact: Models.ISubArtifact = <any>{
                id: 32,
                name: "SubArtifact",
                prefix: "SA",
                predefinedType: ItemTypePredefined.PROShape
            };
            const statefulSubArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifact);
            const observerSpy1 = spyOn(metadataService, "getArtifactPropertyTypes").and.callThrough();
            onChangesObj.context.currentValue.subArtifact = statefulSubArtifact;

            // Act
            ctrl.$onChanges(onChangesObj);
            $rootScope.$digest();

            // Assert
            expect(observerSpy1).not.toHaveBeenCalled();
        }));

    it("load properties for artifact",
        inject(($rootScope: ng.IRootScopeService,
                $q: ng.IQService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                artifactService: ArtifactServiceMock) => {
            //Arrange
            const artifactModel = {
                id: 22,
                name: "Artifact",
                prefix: "My",
                predefinedType: ItemTypePredefined.Process
            } as Models.IArtifact;
            ctrl.customFields = [];
            expect(ctrl.editor.propertyContexts).toBeFalsy();
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);
            onChangesObj.context.currentValue.artifact = artifact;

            const processArtifactReturn: INovaProcess = ArtifactServiceMock.createArtifact(1);
            processArtifactReturn.process = createDefaultProcessModel();
            const getArtifactModelSpy = spyOn(artifactService, "getArtifactModel").
                                    and.returnValue($q.when(processArtifactReturn));
            // Act
            ctrl.$onChanges(onChangesObj);
            $rootScope.$digest();

            // Assert
            expect(ctrl.editor.propertyContexts).toBeTruthy();
        }));

    it("on field update for plain text",
        inject(($rootScope: ng.IRootScopeService) => {
            //Arrange
            const itemtypeId = 1;
            const descriptor: IPropertyDescriptor = {
                id: itemtypeId,
                versionId: 1,
                name: "",
                primitiveType: undefined,
                instancePropertyTypeId: 0,
                isRichText: false,
                decimalDefaultValue: 0,
                dateDefaultValue: undefined,
                userGroupDefaultValue: undefined,
                stringDefaultValue: "",
                decimalPlaces: 0,
                maxNumber: 0,
                minNumber: 0,
                maxDate: undefined,
                minDate: undefined,
                isMultipleAllowed: false,
                isRequired: false,
                isValidated: false,
                validValues: undefined,
                defaultValidValueId: 0,
                propertyTypePredefined: Enums.PropertyTypePredefined.Description,
                disabled: false,
                lookup: Enums.PropertyLookupEnum.System,
                fieldPropertyName: `${Enums.PropertyLookupEnum[Enums.PropertyLookupEnum.System]}_${itemtypeId.toString()}`,
                modelPropertyName: itemtypeId
            };
            const field: AngularFormly.IFieldConfigurationObject = {data: descriptor};
            ctrl.systemFields = [];

            // Act
            ctrl.onFieldUpdate(field);

            // Assert
            expect(ctrl.systemFields[0]).toBeTruthy();
        }));

    it("on field update for rich text",
        inject(($rootScope: ng.IRootScopeService) => {
            //Arrange
            const itemtypeId = 1;
            const descriptor: IPropertyDescriptor = {
                id: itemtypeId,
                versionId: 1,
                name: "",
                primitiveType: undefined,
                instancePropertyTypeId: 0,
                isRichText: true,
                decimalDefaultValue: 0,
                dateDefaultValue: undefined,
                userGroupDefaultValue: undefined,
                stringDefaultValue: "",
                decimalPlaces: 0,
                maxNumber: 0,
                minNumber: 0,
                maxDate: undefined,
                minDate: undefined,
                isMultipleAllowed: false,
                isRequired: false,
                isValidated: false,
                validValues: undefined,
                defaultValidValueId: 0,
                propertyTypePredefined: Enums.PropertyTypePredefined.Description,
                disabled: false,
                lookup: Enums.PropertyLookupEnum.Custom,
                fieldPropertyName: `${Enums.PropertyLookupEnum[Enums.PropertyLookupEnum.Custom]}_${itemtypeId.toString()}`,
                modelPropertyName: itemtypeId
            };

            const field: AngularFormly.IFieldConfigurationObject = {data: descriptor};
            ctrl.richTextFields = [];

            // Act
            ctrl.onFieldUpdate(field);

            // Assert
            expect(ctrl.richTextFields[0]).toBeTruthy();
        }));

    xit("should return correct property types for a selected sub-artifact",
        inject(($q: ng.IQService,
                $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                selectionManager: ISelectionManager,
                artifactService: ArtifactServiceMock,
                metadataService: MetaDataService) => {
            //Arrange
            const artifactModel = {
                id: 22,
                name: "Business Process",
                prefix: "My",
                predefinedType: ItemTypePredefined.BusinessProcess
            } as Models.IArtifact;
            const subArtifactModel = {
                id: 32,
                name: "Business Process Shape",
                prefix: "SA",
                predefinedType: ItemTypePredefined.BPShape
            } as Models.ISubArtifact;

            const deferred = $q.defer<any>();
            deferred.resolve({
                data: {
                    artifactTypes: [],
                    propertyTypes: [],
                    subArtifactTypes: [{
                            predefinedType: ItemTypePredefined.BPShape,
                            customProperties: []
                        }
                    ]
                }
            });

            metaDataServiceGetMethodSpy.and.returnValue(deferred.promise);

            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);

            // Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            selectionManager.setSubArtifact(subArtifact);
            $rootScope.$digest();

            // Assert
            const propertyContexts = ctrl.specificFields.map(a => a.data as IPropertyDescriptor);

            expect(propertyContexts.filter(a => a.name === "Label_X").length).toBe(1);
            expect(propertyContexts.filter(a => a.name === "Label_Y").length).toBe(1);
            expect(propertyContexts.filter(a => a.name === "Label_Width").length).toBe(1);
            expect(propertyContexts.filter(a => a.name === "Label_Height").length).toBe(1);
        }));
    xit("should contain populated model data for a selected sub-artifact",
        inject(($q: ng.IQService,
                $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                selectionManager: ISelectionManager,
                artifactService: ArtifactServiceMock,
                metadataService: MetaDataService) => {
            //Arrange
            const artifactModel = {
                id: 22,
                name: "Artifact",
                prefix: "My",
                predefinedType: ItemTypePredefined.BusinessProcess
            } as Models.IArtifact;
            const subArtifactModel = {
                id: 32,
                name: "SubArtifact",
                prefix: "SA",
                predefinedType: ItemTypePredefined.BPShape
            } as Models.ISubArtifact;

            const x = 1;
            const y = 2;
            const width = 100;
            const height = 200;
            const xPropertyValue = ArtifactServiceMock.createSpecificPropertyValue(1, x, Models.PropertyTypePredefined.X);
            const yPropertyValue = ArtifactServiceMock.createSpecificPropertyValue(1, y, Models.PropertyTypePredefined.Y);
            const widthPropertyValue = ArtifactServiceMock.createSpecificPropertyValue(1, width, Models.PropertyTypePredefined.Width);
            const heightPropertyValue = ArtifactServiceMock.createSpecificPropertyValue(1, height, Models.PropertyTypePredefined.Height);

            const deferred = $q.defer<any>();
            deferred.resolve({
                data: {
                    artifactTypes: [],
                    propertyTypes: [],
                    subArtifactTypes: [{
                            predefinedType: ItemTypePredefined.BPShape,
                            customProperties: []
                        }
                    ]
                }
            });

            metaDataServiceGetMethodSpy.and.returnValue(deferred.promise);

            spyOn(artifactService, "getSubArtifact").and.callFake((artifactId: number, subArtifactId: number) => {
                    const model = {
                        predefinedType: ItemTypePredefined.BPShape,
                        specificPropertyValues: [xPropertyValue, yPropertyValue, widthPropertyValue, heightPropertyValue]
                    };
                    const deferred = $q.defer<any>();
                    deferred.resolve(model);
                    return deferred.promise;

                }
            );

            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);

            // Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            selectionManager.setSubArtifact(subArtifact);
            $rootScope.$digest();

            // Assert
            const model: any = ctrl.model;
            expect(model.x).toBeDefined();
            expect(model.y).toBeDefined();
            expect(model.width).toBeDefined();
            expect(model.height).toBeDefined();
            expect(model.x).toBe(x);
            expect(model.y).toBe(y);
            expect(model.width).toBe(width);
            expect(model.height).toBe(height);
        }));

    xit("should not display properties for a selected process shape ",
        inject(($q: ng.IQService,
                $rootScope: ng.IRootScopeService,
                statefulArtifactFactory: IStatefulArtifactFactory,
                selectionManager: ISelectionManager,
                artifactService: ArtifactServiceMock,
                metadataService: MetaDataService) => {
            //Arrange
            const artifactModel = {
                id: 22,
                name: "Artifact",
                prefix: "My",
                predefinedType: ItemTypePredefined.Process
            } as Models.IArtifact;
            const subArtifactModel = {
                id: 32,
                name: "SubArtifact",
                prefix: "SA",
                predefinedType: ItemTypePredefined.PROShape
            } as Models.ISubArtifact;

            const x = 1;
            const y = 2;
            const width = 100;
            const height = 200;
            const xPropertyValue = ArtifactServiceMock.createSpecificPropertyValue(1, x, Models.PropertyTypePredefined.X);
            const yPropertyValue = ArtifactServiceMock.createSpecificPropertyValue(1, y, Models.PropertyTypePredefined.Y);
            const widthPropertyValue = ArtifactServiceMock.createSpecificPropertyValue(1, width, Models.PropertyTypePredefined.Width);
            const heightPropertyValue = ArtifactServiceMock.createSpecificPropertyValue(1, height, Models.PropertyTypePredefined.Height);

            const deferred = $q.defer<any>();
            deferred.resolve({
                data: {
                    artifactTypes: [],
                    propertyTypes: [],
                    subArtifactTypes: [{
                            predefinedType: ItemTypePredefined.PROShape,
                            customProperties: []
                        }
                    ]
                }
            });

            metaDataServiceGetMethodSpy.and.returnValue(deferred.promise);

            spyOn(artifactService, "getSubArtifact").and.callFake((artifactId: number, subArtifactId: number) => {
                    const model = {
                        predefinedType: ItemTypePredefined.PROShape,
                        specificPropertyValues: [xPropertyValue, yPropertyValue, widthPropertyValue, heightPropertyValue]
                    };
                    const deferred = $q.defer<any>();
                    deferred.resolve(model);
                    return deferred.promise;

                }
            );
            const artifact = statefulArtifactFactory.createStatefulArtifact(artifactModel);
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);

            // Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            selectionManager.setSubArtifact(subArtifact);
            $rootScope.$digest();

            // Assert
            const propertyContexts: IPropertyDescriptor[] = ctrl.specificFields.map(a => a.data as IPropertyDescriptor);

            expect(propertyContexts.filter(a => a.name === "Label_X").length).toBe(0);
            expect(propertyContexts.filter(a => a.name === "Label_Y").length).toBe(0);
            expect(propertyContexts.filter(a => a.name === "Label_Width").length).toBe(0);
            expect(propertyContexts.filter(a => a.name === "Label_Height").length).toBe(0);
        }));
});
