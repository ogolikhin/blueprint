import "angular";
import "angular-mocks";
import {IStatefulArtifact} from "../../managers/artifact-manager/artifact";
import {StatefulArtifactFactoryMock} from "../../managers/artifact-manager/artifact/artifact.factory.mock";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IPropertyValue} from "../../main/models/models";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {IPropertyDescriptor} from "../configuration/property-descriptor-builder";
import {PropertyEditor} from "./bp-property-editor";


describe("Property Editor ->", () => {

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
            const result = editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);

            //Assert
            expect(result).toBeTruthy();
            expect(spy).toHaveBeenCalledTimes(12);
        });
        it("verify property fields", () => {
            editor.create(artifact, mockData.properties as IPropertyDescriptor[], true);
            const fields = editor.getFields();
            //Assert
            expect(fields).toEqual(jasmine.any(Array));
            expect(fields.length).toEqual(12);
        });
    });
});
