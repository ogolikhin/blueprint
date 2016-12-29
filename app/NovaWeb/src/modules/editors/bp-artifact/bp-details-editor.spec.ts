﻿import "angular";
import "angular-mocks";
import "../";
import {ComponentTest} from "../../util/component.test";
import {BpArtifactDetailsEditorController} from "./bp-details-editor";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {ArtifactManagerMock} from "./../../managers/artifact-manager/artifact-manager.mock";
import {WindowManagerMock} from "./../../main/services/window-manager.mock";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {PropertyDescriptorBuilderMock} from "./../configuration/property-descriptor-builder.mock";
import {IPropertyDescriptorBuilder, IPropertyDescriptor} from "./../configuration/property-descriptor-builder";
import {IArtifactManager} from "./../../managers/artifact-manager";
import {ISelectionManager} from "./../../managers/selection-manager/selection-manager";
import {IStatefulArtifact} from "./../../managers/artifact-manager/artifact/artifact";
import {ValidationServiceMock} from "./../../managers/artifact-manager/validation/validation.mock";
import {Enums, Models} from "./../../main";

describe("Component BpArtifactDetailsEditor", () => {
    let componentTest: ComponentTest<BpArtifactDetailsEditorController>;
    let template = `<bp-artifact-details-editor context="artifact"></bp-artifact-details-editor>`;
    let ctrl: BpArtifactDetailsEditorController;

    let _artifactManager: IArtifactManager;
    let _propertyDescriptorBuilder: IPropertyDescriptorBuilder;
    let _$q: ng.IQService;
    let descriptor: IPropertyDescriptor;
    let propertyValue: Models.IPropertyValue;

    beforeEach(angular.mock.module("bp.editors.details"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("messageService", MessageServiceMock);
        $provide.service("artifactManager", ArtifactManagerMock);
        $provide.service("windowManager", WindowManagerMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("propertyDescriptorBuilder", PropertyDescriptorBuilderMock);
        $provide.service("validationService", ValidationServiceMock);
    }));

    beforeEach(inject((artifactManager: IArtifactManager, propertyDescriptorBuilder: IPropertyDescriptorBuilder, $q: ng.IQService) => {
        _artifactManager = artifactManager;
        _propertyDescriptorBuilder = propertyDescriptorBuilder;
        _$q = $q;

        propertyValue = {
            propertyTypeId: 1,
            propertyTypeVersionId: 1,
            propertyTypePredefined: Models.PropertyTypePredefined.Description,
            name: "Desc",
            isReuseReadOnly: false,
            isRichText: true,
            primitiveType: Models.PrimitiveType.Text,
            isMultipleAllowed: false,
            value: "My text"
        };

        const specialPropertyValue = {
            propertyTypeId: 1,
            propertyTypeVersionId: 1,
            propertyTypePredefined: Models.PropertyTypePredefined.DocumentFile,
            name: "File",
            isReuseReadOnly: false,
            isRichText: true,
            primitiveType: Models.PrimitiveType.DocumentFile,
            isMultipleAllowed: false,
            value: "My text"
        };

        const itemtypeId = 1;
        descriptor = {
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

        //artifactManager.selection.getArtifact(); -> Needs to return an artifact that has getObservable()
        artifactManager.selection = {
            getArtifact: () => {
                return {
                    getObservable: () => new Rx.BehaviorSubject<IStatefulArtifact>(this).asObservable(),
                    artifactState: {readonly: false} as any,
                    customProperties: {
                        get: (() => propertyValue)
                    },
                    specialProperties: {
                        get: (() => specialPropertyValue)
                    }
                } as any;
            }
        } as ISelectionManager;
    }));

    beforeEach(() => {
        componentTest = new ComponentTest<BpArtifactDetailsEditorController>(template, "bp-artifact-details-editor");
    });

    afterEach(() => {
        ctrl = null;
    });

     it("should be visible by default", () => {
        // Arrange
        spyOn(_propertyDescriptorBuilder, "createArtifactPropertyDescriptors").and.callFake(() => {
            return _$q.resolve<IPropertyDescriptor[]>([descriptor]);
        });

        ctrl = componentTest.createComponent({});

        //Assert
        expect(componentTest.element.find(".artifact-overview").length).toBe(1);
        expect(componentTest.element.find(".readonly-indicator").length).toBe(0);
        expect(componentTest.element.find(".lock-indicator").length).toBe(0);
        expect(componentTest.element.find(".dirty-indicator").length).toBe(0);
     });

     it("should have a rich text field for description", () => {
        // Arrange
        spyOn(_propertyDescriptorBuilder, "createArtifactPropertyDescriptors").and.callFake(() => {
            return _$q.resolve<IPropertyDescriptor[]>([descriptor]);
        });

        ctrl = componentTest.createComponent({});

        //Assert
        expect(ctrl.richTextFields.length).toBe(1);
     });

     it("should validate a required rich text field", () => {
        // Arrange
        descriptor.isRequired = true;
        spyOn(_propertyDescriptorBuilder, "createArtifactPropertyDescriptors").and.callFake(() => {
            return _$q.resolve<IPropertyDescriptor[]>([descriptor]);
        });

        ctrl = componentTest.createComponent({});

        //Assert
        expect(ctrl.richTextFields.length).toBe(1);
        expect(ctrl.isRtfFieldValid(ctrl.richTextFields[0])).toBe(true);
     });

     it("should have a system field for name", () => {
        // Arrange
        descriptor.propertyTypePredefined = Enums.PropertyTypePredefined.Name;
        descriptor.primitiveType = Models.PrimitiveType.Text;
        descriptor.lookup = Enums.PropertyLookupEnum.System;
        descriptor.isRichText = false;
        spyOn(_propertyDescriptorBuilder, "createArtifactPropertyDescriptors").and.callFake(() => {
            return _$q.resolve<IPropertyDescriptor[]>([descriptor]);
        });

        ctrl = componentTest.createComponent({});

        //Assert
        expect(ctrl.systemFields.length).toBe(1);
        expect(ctrl.isSystemPropertyAvailable).toBe(true);
     });

     it("should have a custom field if provided", () => {
        // Arrange
        descriptor.propertyTypePredefined = Enums.PropertyTypePredefined.None;
        descriptor.primitiveType = Models.PrimitiveType.Text;
        descriptor.lookup = Enums.PropertyLookupEnum.Custom;
        descriptor.isRichText = false;
        spyOn(_propertyDescriptorBuilder, "createArtifactPropertyDescriptors").and.callFake(() => {
            return _$q.resolve<IPropertyDescriptor[]>([descriptor]);
        });

        ctrl = componentTest.createComponent({});

        //Assert
        expect(ctrl.customFields.length).toBe(1);
        expect(ctrl.isCustomPropertyAvailable).toBe(true);
     });

     it("should have a special field if provided", () => {
        // Arrange
        descriptor.propertyTypePredefined = Enums.PropertyTypePredefined.None;
        descriptor.primitiveType = Models.PrimitiveType.DocumentFile;
        descriptor.lookup = Enums.PropertyLookupEnum.Special;
        descriptor.isRichText = false;
        spyOn(_propertyDescriptorBuilder, "createArtifactPropertyDescriptors").and.callFake(() => {
            return _$q.resolve<IPropertyDescriptor[]>([descriptor]);
        });

        ctrl = componentTest.createComponent({});

        //Assert
        expect(ctrl.specificFields.length).toBe(1);
     });

     it("should have no fields if none are provided", () => {
        // Arrange
        spyOn(_propertyDescriptorBuilder, "createArtifactPropertyDescriptors").and.callFake(() => {
            return _$q.resolve<IPropertyDescriptor[]>([]);
        });

        ctrl = componentTest.createComponent({});

        //Assert
        expect(ctrl.systemFields.length).toBe(0);
        expect(ctrl.richTextFields.length).toBe(0);
        expect(ctrl.fields.length).toBe(0);
        expect(ctrl.editor.getFields().length).toBe(0);
     });
});