import "angular-mocks";
import "angular-sanitize";
import "lodash";
import {LocalizationServiceMock} from "../../commonModule/localization/localization.service.mock";
import {PrimitiveType, PropertyLookupEnum, PropertyTypePredefined} from "../../main/models/enums";
import {ItemTypePredefined} from "../../main/models/item-type-predefined";
import {IPropertyValue} from "../../main/models/models";
import {IStatefulArtifact, IStatefulSubArtifact} from "../../managers/artifact-manager";
import {IPropertyDescriptor, IPropertyDescriptorBuilder, PropertyDescriptor} from "./propertyDescriptorBuilder.service";
import * as angular from "angular";

describe("property-descriptor-builder->", () => {

    beforeEach(angular.mock.module("editor.services"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));

    describe("Property Descriptor->", () => {
        it("create property descriptor from rich text property value", () => {
            //arrange
            const propertyValue: IPropertyValue = <any>{
                propertyTypeId: 1,
                propertyTypePredefined: PropertyTypePredefined.CustomGroup,
                isRichText: true,
                primitiveType: PrimitiveType.Text,
                isMultipleAllowed: true
            };
            //act
            const propertyDescriptor = PropertyDescriptor.createFromPropertyValue(propertyValue);
            //assert
            expect(propertyDescriptor.isRichText).toEqual(true);
            expect(propertyDescriptor.isMultipleAllowed).toEqual(true);
            expect(propertyDescriptor.primitiveType).toEqual(PrimitiveType.Text);
            expect(propertyDescriptor.propertyTypePredefined).toEqual(PropertyTypePredefined.CustomGroup);
        });

        it("create property descriptor from plain text property value", () => {
            //arrange
            const propertyValue: IPropertyValue = <any>{
                propertyTypeId: 1,
                propertyTypePredefined: PropertyTypePredefined.CustomGroup,
                primitiveType: PrimitiveType.Text,
                isMultipleAllowed: false
            };
            //act
            const propertyDescriptor = PropertyDescriptor.createFromPropertyValue(propertyValue);
            //assert
            expect(propertyDescriptor.isRichText).toBeFalsy();
            expect(propertyDescriptor.isMultipleAllowed).toEqual(false);
            expect(propertyDescriptor.primitiveType).toEqual(PrimitiveType.Text);
            expect(propertyDescriptor.propertyTypePredefined).toEqual(PropertyTypePredefined.CustomGroup);
        });

        it("create property descriptor from choice property value", () => {
            //arrange
            const validValues = [
                {id: 1, value: "value 1"},
                {id: 2, value: "value 2"}
            ];
            const propertyValue: IPropertyValue = <any>{
                propertyTypeId: 1,
                propertyTypePredefined: PropertyTypePredefined.CustomGroup,
                primitiveType: PrimitiveType.Choice,
                isMultipleAllowed: true,
                value: {
                    validValues: validValues
                }
            };
            //act
            const propertyDescriptor = PropertyDescriptor.createFromPropertyValue(propertyValue);
            //assert
            expect(propertyDescriptor.isRichText).toBeFalsy();
            expect(propertyDescriptor.isMultipleAllowed).toEqual(true);
            expect(propertyDescriptor.primitiveType).toEqual(PrimitiveType.Choice);
            expect(propertyDescriptor.propertyTypePredefined).toEqual(PropertyTypePredefined.CustomGroup);
            expect(propertyDescriptor.validValues).toEqual(validValues);
        });

        it("create property descriptor from date property value", () => {
            //arrange
            const propertyValue: IPropertyValue = <any>{
                propertyTypeId: 1,
                propertyTypePredefined: PropertyTypePredefined.CustomGroup,
                primitiveType: PrimitiveType.Date
            };
            //act
            const propertyDescriptor = PropertyDescriptor.createFromPropertyValue(propertyValue);
            //assert
            expect(propertyDescriptor.isRichText).toBeFalsy();
            expect(propertyDescriptor.isMultipleAllowed).toBeFalsy();
            expect(propertyDescriptor.primitiveType).toEqual(PrimitiveType.Date);
            expect(propertyDescriptor.propertyTypePredefined).toEqual(PropertyTypePredefined.CustomGroup);
        });

        it("create property descriptor from number property value", () => {
            //arrange
            const propertyValue: IPropertyValue = <any>{
                propertyTypeId: 1,
                propertyTypePredefined: PropertyTypePredefined.CustomGroup,
                primitiveType: PrimitiveType.Number
            };
            //act
            const propertyDescriptor = PropertyDescriptor.createFromPropertyValue(propertyValue);
            //assert
            expect(propertyDescriptor.isRichText).toBeFalsy();
            expect(propertyDescriptor.isMultipleAllowed).toBeFalsy();
            expect(propertyDescriptor.primitiveType).toEqual(PrimitiveType.Number);
            expect(propertyDescriptor.propertyTypePredefined).toEqual(PropertyTypePredefined.CustomGroup);
        });
    });

    describe("historical Artifact->", () => {

        let artifact: IStatefulArtifact;

        beforeEach(() => {
            artifact = <any>{
                itemTypeId: 1,
                subArtifactCollection: undefined,
                artifactState: {
                    historical: true
                },
                customProperties: {
                    list: () => {
                        return [];
                    }
                }
            };
        });

        it("creates system property descriptors for Requirement", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            artifact.predefinedType = ItemTypePredefined.TextualRequirement;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createArtifactPropertyDescriptors(artifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            expect(propertyDescriptors.length).toEqual(7);
            const name = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Name}) as IPropertyDescriptor;
            expect(name.lookup).toEqual(PropertyLookupEnum.System);
            const typeId = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.ItemTypeId}) as IPropertyDescriptor;
            expect(typeId.lookup).toEqual(PropertyLookupEnum.System);
            const createdBy = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.CreatedBy}) as IPropertyDescriptor;
            expect(createdBy.lookup).toEqual(PropertyLookupEnum.System);
            const createdOn = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.CreatedOn}) as IPropertyDescriptor;
            expect(createdOn.lookup).toEqual(PropertyLookupEnum.System);
            const lastEditedBy = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.LastEditedBy}) as IPropertyDescriptor;
            expect(lastEditedBy.lookup).toEqual(PropertyLookupEnum.System);
            const lastEditedOn = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.LastEditedOn}) as IPropertyDescriptor;
            expect(lastEditedOn.lookup).toEqual(PropertyLookupEnum.System);
            const description = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Description}) as IPropertyDescriptor;
            expect(description.lookup).toEqual(PropertyLookupEnum.System);
        }));

        it("creates 7 system and 2 specific property descriptors for Actor", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            artifact.predefinedType = ItemTypePredefined.Actor;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createArtifactPropertyDescriptors(artifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            //assert
            expect(propertyDescriptors.length).toEqual(9);
            const image = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Image}) as IPropertyDescriptor;
            expect(image.lookup).toEqual(PropertyLookupEnum.Special);
            const actorInheritance = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.ActorInheritance}) as IPropertyDescriptor;
            expect(actorInheritance.lookup).toEqual(PropertyLookupEnum.Special);
        }));

        it("creates 7 system and 1 specific property descriptors for Document", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            artifact.predefinedType = ItemTypePredefined.Document;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createArtifactPropertyDescriptors(artifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            //assert
            expect(propertyDescriptors.length).toEqual(8);
            const documentFile = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.DocumentFile}) as IPropertyDescriptor;
            expect(documentFile.lookup).toEqual(PropertyLookupEnum.Special);
        }));

        it("creates custom property descriptors for Requirement", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            artifact.predefinedType = ItemTypePredefined.TextualRequirement;
            spyOn(artifact.customProperties, "list").and.returnValue([{
                propertyTypeId: 1,
                propertyTypePredefined: PropertyTypePredefined.CustomGroup
            }]);
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createArtifactPropertyDescriptors(artifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            //assert
            const customProperty = _.find(propertyDescriptors, {modelPropertyName: 1}) as IPropertyDescriptor;
            expect(customProperty.lookup).toEqual(PropertyLookupEnum.Custom);
        }));

    });

    describe("historical Sub-Artifact->", () => {

        let subArtifact: IStatefulSubArtifact;

        beforeEach(() => {
            subArtifact = <any>{
                predefinedType: undefined,
                artifactState: {
                    historical: true
                },
                customProperties: {
                    list: () => {
                        return [];
                    }
                }
            };
        });

        function verifyShapeProperties(propertyDescriptors: IPropertyDescriptor[]) {
            expect(propertyDescriptors.length).toEqual(7);
            const name = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Name}) as IPropertyDescriptor;
            expect(name.lookup).toEqual(PropertyLookupEnum.System);
            const description = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Description}) as IPropertyDescriptor;
            expect(description.lookup).toEqual(PropertyLookupEnum.System);
            const label = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Label}) as IPropertyDescriptor;
            expect(label.lookup).toEqual(PropertyLookupEnum.Special);
            const x = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.X}) as IPropertyDescriptor;
            expect(x.lookup).toEqual(PropertyLookupEnum.Special);
            const y = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Y}) as IPropertyDescriptor;
            expect(y.lookup).toEqual(PropertyLookupEnum.Special);
            const width = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Width}) as IPropertyDescriptor;
            expect(width.lookup).toEqual(PropertyLookupEnum.Special);
            const heigth = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Height}) as IPropertyDescriptor;
            expect(heigth.lookup).toEqual(PropertyLookupEnum.Special);
        }

        function verifyConnectorProperties(propertyDescriptors: IPropertyDescriptor[]) {
            expect(propertyDescriptors.length).toEqual(3);
            const name = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Name}) as IPropertyDescriptor;
            expect(name.lookup).toEqual(PropertyLookupEnum.System);
            const description = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Description}) as IPropertyDescriptor;
            expect(description.lookup).toEqual(PropertyLookupEnum.System);
            const label = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Label}) as IPropertyDescriptor;
            expect(label.lookup).toEqual(PropertyLookupEnum.Special);
        }

        it("creates property descriptors for Use Case step", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.Step;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            expect(propertyDescriptors.length).toEqual(3);
            const name = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Name}) as IPropertyDescriptor;
            expect(name.lookup).toEqual(PropertyLookupEnum.System);
            const description = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.Description}) as IPropertyDescriptor;
            expect(description.lookup).toEqual(PropertyLookupEnum.System);
            const stepOf = _.find(propertyDescriptors, {propertyTypePredefined: PropertyTypePredefined.StepOf}) as IPropertyDescriptor;
            expect(stepOf.lookup).toEqual(PropertyLookupEnum.Special);
        }));

        it("creates property descriptors for Generic Diagram Shape", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.GDShape;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyShapeProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Domain Diagram Shape", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.DDShape;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyShapeProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Story board Shape", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.SBShape;
            const shape = subArtifact;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(shape).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyShapeProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Story board Shape", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.SBShape;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyShapeProperties(propertyDescriptors);
        }));

        it("creates property descriptors for UI Mockup Shape", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.UIShape;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyShapeProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Use Case Diagram Shape", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.UCDShape;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyShapeProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Business Process Shape", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.BPShape;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyShapeProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Process Shape", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.PROShape;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyShapeProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Generic Diagram Connector", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.GDConnector;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyConnectorProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Domain Diagram Connector", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.DDConnector;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyConnectorProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Storyboard Connector", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.SBConnector;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyConnectorProperties(propertyDescriptors);
        }));

        it("creates property descriptors for UI Mockup Connector", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.UIConnector;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyConnectorProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Business Process Connector", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.BPConnector;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyConnectorProperties(propertyDescriptors);
        }));

        it("creates property descriptors for Use Case Diagram Connector", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.UCDConnector;
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            // assert
            verifyConnectorProperties(propertyDescriptors);
        }));

        it("creates custom property descriptors for Requirement", inject((
            propertyDescriptorBuilder: IPropertyDescriptorBuilder,
            $rootScope: ng.IRootScopeService) => {
            // arrange
            subArtifact.predefinedType = ItemTypePredefined.TextualRequirement;
            spyOn(subArtifact.customProperties, "list").and.returnValue([{
                propertyTypeId: 1,
                propertyTypePredefined: PropertyTypePredefined.CustomGroup
            }]);
            // act
            let propertyDescriptors;
            propertyDescriptorBuilder.createSubArtifactPropertyDescriptors(subArtifact).then((result) => {
                propertyDescriptors = result;
            });
            $rootScope.$digest();
            //assert
            const customProperty = _.find(propertyDescriptors, {modelPropertyName: 1}) as IPropertyDescriptor;
            expect(customProperty.lookup).toEqual(PropertyLookupEnum.Custom);
        }));

    });
});
