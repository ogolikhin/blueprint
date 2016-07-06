import { ILocalizationService } from "../../core";
import * as Models from "../models/models";
import {tinymceMentionsData} from "../../util/tinymce-mentions.mock.ts";
export interface IArtifactService {

    getArtifactPropertyFileds(artifact: Models.IArtifact, project: Models.IProject): Models.IArtifactDetailFields}


interface IFieldType {
    name: string;
    type: Models.IPrimitiveType;
}
interface IFiledData {
    name: string;
    group: string;
    minValue?: any;
    maxValue?: any;

}


export class ArtifactService implements IArtifactService {

    static $inject: [string] = ["localization"];

    static systemProperties: [IFieldType] = [
        { name: "name", type: Models.IPrimitiveType.Text },
        { name: "type", type: Models.IPrimitiveType.Choice },
        { name: "createdBy", type: Models.IPrimitiveType.Text },
        { name: "createdOn", type: Models.IPrimitiveType.Date },
        { name: "lastEditBy", type: Models.IPrimitiveType.Text },
        { name: "lastEditOn", type: Models.IPrimitiveType.Date }
    ]
    static noteProperties: [IFieldType] = [
        { name: "description", type: Models.IPrimitiveType.Text },
    ]
    
    constructor(
        private localization: ILocalizationService) {
    }

    private addFieldValidation(field: AngularFormly.IFieldConfigurationObject): AngularFormly.IFieldConfigurationObject {

        return field;
    }

    private createField(modelName: string, type: Models.IPropertyType): AngularFormly.IFieldConfigurationObject {
        if (!modelName) {
            throw new Error(this.localization.get("Artifact_Details_FieldNameError"));
        }
        if (!type) {
            throw new Error(this.localization.get("ArtifactType_NotFound"));
        }
        let field: AngularFormly.IFieldConfigurationObject = {
            key: modelName,
            templateOptions: {
                label: type.name,
                required: type.isRequired,
                disabled: type.disabled
            },
            data: type,
            expressionProperties: {},
        };


        switch (type.primitiveType) {
            case Models.IPrimitiveType.Text:
                field.type = type.isRichText ? "tinymceInline" : (type.isMultipleAllowed ? "textarea" : "input");
                field.defaultValue = type.stringDefaultValue;
                //field.templateOptions.minlength;
                //field.templateOptions.maxlength;
                break;
            case Models.IPrimitiveType.Date:
                field.type = "input";
                field.templateOptions.type = "date"
                field.defaultValue = type.dateDefaultValue || new Date();
                //field.templateOptions.min = type.minDate;
                //field.templateOptions.max = type.maxDate;
                break;
            case Models.IPrimitiveType.Number:
                field.type = "input";
                field.templateOptions.type = "number"
                field.defaultValue = type.decimalDefaultValue || 0;
                field.templateOptions.min = type.minNumber;
                field.templateOptions.max = type.maxNumber;
                break;
            case Models.IPrimitiveType.Choice:
                field.type = "select";
                field.defaultValue = (type.defaultValidValueIndex || 0).toString();
                if (type.validValues) {
                    field.templateOptions.options = type.validValues.map(function (it, index) {
                        return <AngularFormly.ISelectOption>{ value: index.toString(), name: it };
                    });
                }
                break;
            default:
                return undefined;
        }
        return field;
    }

    private createSystemPropertyFileds(artifactType: Models.IItemType, metaData: Models.IProjectMeta): AngularFormly.IFieldConfigurationObject[] {
        let fields: AngularFormly.IFieldConfigurationObject[] = [];
        let field: AngularFormly.IFieldConfigurationObject;

        fields.push(this.createField("name", <Models.IPropertyType>{
            id: -1,
            name: "Name",
            primitiveType: Models.IPrimitiveType.Text,
            isRequired: true
        }));
        fields.push(field = this.createField("type", <Models.IPropertyType>{
            id: -1,
            name: "Type",
            primitiveType: Models.IPrimitiveType.Choice,
            isRequired: true
        }));
        
        field.templateOptions.options = metaData.artifactTypes.filter((it: Models.IItemType) => {
            return (artifactType && artifactType.baseType === it.baseType);
        }).map(function (it) {
            return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.name };
            });
        field.expressionProperties = {
            "templateOptions.disabled": "to.options.length < 2",
        };

        fields.push(this.createField("createBy", <Models.IPropertyType>{
            id: -1,
            name: "Created by",
            primitiveType: Models.IPrimitiveType.Text,
            disabled: true
        }));
        fields.push(this.createField("createdOn", <Models.IPropertyType>{
            id: -1,
            name: "Created on",
            primitiveType: Models.IPrimitiveType.Date,
            disabled: true
        }));
        fields.push(this.createField("lastEditBy", <Models.IPropertyType>{
            id: -1,
            name: "Last edited by",
            primitiveType: Models.IPrimitiveType.Text,
            disabled: true
        }));
        fields.push(this.createField("lastEditOn", <Models.IPropertyType>{
            id: -1,
            name: "Last edited on",
            primitiveType: Models.IPrimitiveType.Date,
            disabled: true
        }));

        return fields;

    }

    private createCustomPropertyFileds(model: any, artifactType: Models.IItemType, metaData: Models.IProjectMeta): AngularFormly.IFieldConfigurationObject[] {
        let fields: AngularFormly.IFieldConfigurationObject[] = [];
        let field: AngularFormly.IFieldConfigurationObject;

        if (artifactType) {
            metaData.propertyTypes.map((it: Models.IPropertyType) => {
                if (artifactType.customPropertyTypeIds.indexOf(it.id) >= 0) {
                    field = this.createField(`property_${it.id}`, it);
                    if (field) {
                        fields.push(field);
                    }
                }
            });
        }
        return fields;
    }

    private createNotePropertyFileds(artifactType: Models.IItemType, metaData: Models.IProjectMeta): AngularFormly.IFieldConfigurationObject[] {
        let fields: AngularFormly.IFieldConfigurationObject[] = [];
        let field: AngularFormly.IFieldConfigurationObject;
        fields.push({
            key: "tinymceControl",
            type: "tinymce",
            data: { // using data property
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standard tinymce option
                    plugins: "advlist autolink link image paste lists charmap print noneditable mention",
                    mentions: {
                        source: tinymceMentionsData,
                        delay: 100,
                        items: 5,
                        queryBy: "fullname",
                        insert: function (item) {
                            return `<a class="mceNonEditable" href="mailto:` + item.emailaddress + `" title="ID# ` + item.id + `">` + item.fullname + `</a>`;
                        }
                    }
                }
            },
            templateOptions: {
                label: "TinyMCE control"
            }
        });
        fields.push({
            key: "tinymceInlineControl",
            type: "tinymceInline",
            data: { // using data property
                tinymceOption: { // this will goes to ui-tinymce directive
                    // standard tinymce option
                    inline: true,
                    plugins: "advlist autolink link image paste lists charmap print noneditable mention",
                    mentions: {
                        source: tinymceMentionsData,
                        delay: 100,
                        items: 5,
                        queryBy: "fullname",
                        insert: function (item) {
                            return `<a class="mceNonEditable" href="mailto:` + item.emailaddress + `" title="ID# ` + item.id + `">` + item.fullname + `</a>`;
                        }
                    },
                    fixed_toolbar_container: ".form-tinymce-toolbar"
                }
            },
            templateOptions: {
                label: "TinyMCE Inline control"
            }
        });
        
        return fields;
    }

    public getArtifactPropertyFileds(artifact: Models.IArtifact, project: Models.IProject): Models.IArtifactDetailFields {
        let fields: Models.IArtifactDetailFields = <Models.IArtifactDetailFields>{
            systemFields: [],
            customFields: [],
            noteFields: []
        };
        if (!artifact) {
            throw new Error(this.localization.get("Artifact_NotFound"));
        }
        if (!project || !project.meta) {
            throw new Error(this.localization.get("Project_NotFound"));
        }
        let artifactType = project.meta.artifactTypes.filter((it: Models.IItemType) => {
            return it.id === artifact.typeId;
        })[0];

        
        fields.systemFields = this.createSystemPropertyFileds(artifactType, project.meta);
        fields.customFields = this.createCustomPropertyFileds(artifact, artifactType, project.meta);
        fields.noteFields = this.createNotePropertyFileds(artifactType, project.meta);

        return fields;

    }

}