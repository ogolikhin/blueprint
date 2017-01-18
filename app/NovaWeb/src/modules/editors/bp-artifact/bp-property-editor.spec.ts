import "angular";
import "angular-mocks";
import {IStatefulArtifact} from "../../managers/artifact-manager/artifact";
import {StatefulArtifactFactoryMock} from "../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService} from "../../core/localization/localization.service";
import {IPropertyValue} from "../../main/models/models";
import {LocalizationServiceMock} from "../../core/localization/localization.service.mock";
import {IPropertyDescriptor} from "../configuration/property-descriptor-builder";
import {PropertyEditor} from "./bp-property-editor";


describe("Property Editor ->", () => {
    let _$q: ng.IQService;
    let descriptor: IPropertyDescriptor;
    let propertyValue: IPropertyValue;
    const factory = new StatefulArtifactFactoryMock();
    const mockData = JSON.parse(require("./bp-property-editor.mock.json")) ;
    let artifact: IStatefulArtifact;
    let editor: PropertyEditor;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
    }));
    beforeEach(inject(($q: ng.IQService, $rootScope: ng.IRootScopeService, localization: ILocalizationService) => {
        _$q = $q;
        editor = new PropertyEditor(localization);
    }));
    describe("Create", () => {


        beforeEach(inject((localization: ILocalizationService) => {
            artifact = factory.createStatefulArtifact(mockData.artifacts[0]);
            spyOn(artifact, "getServices").and.returnValue({
                localizationService: localization
            });
            artifact.customProperties.initialize(mockData.artifacts[0].customPropertyValues);
        }));
        it("should create property fields", () => {
            const spy = spyOn(editor, "createPropertyField").and.callThrough();
            const result = editor.create(artifact, mockData.properties as IPropertyDescriptor[], false);

            //Assert
            expect(result).toBeTruthy();
            expect(spy).toHaveBeenCalledTimes(12);
        });
        it("verify property fields", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], false);
            const fields = editor.getFields();
            //Assert
            expect(fields).toEqual(jasmine.any(Array));
            expect(fields.length).toEqual(12);
        });
        it("recreate property fields on field name change", () => {
            //Arrange
            const propertyDescriptors = mockData.properties as IPropertyDescriptor[];
            const firstResult = editor.create(artifact, propertyDescriptors, false);
            const fieldsBefore = editor.getFields();


            //Act
            const secondResult = editor.create(artifact, propertyDescriptors, true);
            const fieldsAfter = editor.getFields();

            //Assert
            expect(secondResult).toBeTruthy();
            expect(fieldsBefore).toEqual(jasmine.any(Array));
            expect(fieldsBefore.length).toEqual(12);
            expect(fieldsAfter).toEqual(jasmine.any(Array));
            expect(fieldsAfter.length).toEqual(12);

        });
    });
    describe("Convert values", () => {
        beforeEach(inject((localization: ILocalizationService) => {
            artifact = factory.createStatefulArtifact(mockData.artifacts[0]);
            spyOn(artifact, "getServices").and.returnValue({
                localizationService: localization
            });
            spyOn(artifact, "lock").and.callFake(() => { return _$q.resolve(); });

            artifact.customProperties.initialize(mockData.artifacts[0].customPropertyValues);

        }));
        afterEach(() => {
            artifact = null;
        });
        it("convert - no field", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const newValue = editor.convertToModelValue(null);
            //Asserts
            expect(newValue).toBeNull();
        });
        it("convert - no descriptor", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const newValue = editor.convertToModelValue({});
            //Asserts
            expect(newValue).toBeNull();
        });
        it("convert - invalid value", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const descriptor = _.clone(_.find(mockData.properties, {modelPropertyName: "name"}) as IPropertyDescriptor);
            descriptor.fieldPropertyName = "TTT";
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).toBeNull();
        });
        it("text", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: "name"}) as IPropertyDescriptor;
            //act
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);
            model.name = "NewValue";
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual("NewValue");
        });
        it("richtext", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: "description"}) as IPropertyDescriptor;
            //act
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);
            model.description = "<div>TEST</div>";
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual("<div>TEST</div>");
        });
        it("richtext (no inner text)", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: "description"}) as IPropertyDescriptor;
            //act
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);
            model.description = "<div></div>";
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).toBeFalsy();
        });
        it("date - valid", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 15629}) as IPropertyDescriptor;
            //act
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);
            model[descriptor.fieldPropertyName] = new Date(2016, 11, 26);
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual(new Date(2016, 11, 26));
        });
        it("date(text) - valid", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();

            const descriptor = _.find(mockData.properties, {modelPropertyName: 15629}) as IPropertyDescriptor;
            //act
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);
            const property = artifact.customProperties.get(15629);
            model[descriptor.fieldPropertyName] = "12/20/2016";
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual(new Date(2016, 11, 20));
        });
        it("date - invalid", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();

            const descriptor = _.find(mockData.properties, {modelPropertyName: 15629}) as IPropertyDescriptor;
            //act
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);
            model[descriptor.fieldPropertyName] = "2016/12/20T00:00:00";
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual(null);
        });
        it("number - valid", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();

            const descriptor = _.find(mockData.properties, {modelPropertyName: 11114}) as IPropertyDescriptor;
            //act
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);
            model[descriptor.fieldPropertyName] = 2000;
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual(2000);
        });
        it("number(text) - valid", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();

            const descriptor = _.find(mockData.properties, {modelPropertyName: 11114}) as IPropertyDescriptor;
            //act
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);
            model[descriptor.fieldPropertyName] = "5000";
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual(5000);
        });
        it("number(text) - invalid", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();

            const descriptor = _.find(mockData.properties, {modelPropertyName: 11114}) as IPropertyDescriptor;
            //act
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);
            model[descriptor.fieldPropertyName] = "5000a";
            const newValue = editor.convertToModelValue({data: descriptor});
            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual(null);
        });
        it("choice (single number) - valid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 9641}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = 100;
            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual({validValues: [{id: 100}]});
        });
        it("choice (single text) - valid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 9641}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = "200";

            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual({validValues: [{id: 200}]});
        });
        it("choice - invalid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 9641}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = "aaa";
            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual({validValues: [{id: null}]});
        });
        it("choice(multiple) - valid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 9641}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = [1, 2, 3];

            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual({validValues: [{id: 1}, {id: 2}, {id: 3}]});
        });
        it("choice(multiple text) - valid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 9641}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = [100, "200", 300];

            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual({validValues: [{id: 100}, {id: 200}, {id: 300}]});
        });
        it("choice(custom) - valid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 9641}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = {customValue: "test"};

            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual({customValue: "test"});
        });
        it("choice(custom) - invalid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 9641}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = {custom: "test"};

            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual({customValue: undefined});
        });
        it("user - valid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 15648}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = [{id: 1}];

            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual({usersGroups: [{id: 1}]});
        });
        it("user (gruups)- valid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 15648}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = [{id: 1, isImported: true}, {id: 2}];

            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual({usersGroups: [{id: 2}]});
        });
        it("user - invalid", () => {
            //Arrange
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const model = editor.getModel();
            const descriptor = _.find(mockData.properties, {modelPropertyName: 15648}) as IPropertyDescriptor;
            const oldValue = editor.getModelValue(descriptor.fieldPropertyName);

            //Act
            model[descriptor.fieldPropertyName] = {id: 1};

            const newValue = editor.convertToModelValue({data: descriptor});

            //Asserts
            expect(newValue).not.toEqual(oldValue);
            expect(newValue).toEqual(null);
        });
    });

});
