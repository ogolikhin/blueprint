import { ILocalizationService } from "../../core";
import {IMessageService, Message, MessageType} from "../../shell";
import * as Models from "../models/models";
import {tinymceMentionsData} from "../../util/tinymce-mentions.mock.ts";
export interface IArtifactService {

    getArtifactPropertyFileds(artifact: Models.IArtifact, project: Models.IProject): Models.IArtifactDetailFields}



export class ArtifactService implements IArtifactService {

    static $inject: [string] = ["localization"];
    constructor(
        private localization: ILocalizationService) {
    }

    private addFieldValidation(field: AngularFormly.IFieldConfigurationObject): AngularFormly.IFieldConfigurationObject {

        return field;
    }

    private getSystemPropertyFileds(itemType: Models.IItemType, metaData: Models.IProjectMeta): AngularFormly.IFieldConfigurationObject[] {
        let fields: AngularFormly.IFieldConfigurationObject[] = [];


        fields.push({
            key: "name",
            type: "input",
            templateOptions: {
                label: "Name",
                required: true
            }
        });

        if (itemType) {
            fields.push({
                key: "type",
                type: "select",
                defaultValue: itemType.id.toString(),
                templateOptions: {
                    label: "Type",
                    required: true,
                    options: metaData.artifactTypes.filter((it: Models.IItemType) => {
                        return (itemType && itemType.baseType === it.baseType);
                    }).map(function (it) {
                        return <AngularFormly.ISelectOption>{ value: it.id.toString(), name: it.name };
                    })
                },
                expressionProperties: {
                    "templateOptions.disabled": "to.options.length < 2",
                }
            });
        }
        fields.push({
            key: "createdBy",
            type: "input",
            templateOptions: {
                label: "Created by",
                disabled: true
            }
        });
        fields.push({
            key: "createdOn",
            type: "input",
            templateOptions: {
                type: "date",
                label: "Created on",
                disabled: true,
            }
        });
        fields.push({
            key: "lastEditBy",
            type: "input",
            templateOptions: {
                label: "Last edited by",
                disabled: true
            }
        });
        fields.push({
            key: "lastEditOn",
            type: "input",
            templateOptions: {
                type: "date",
                label: "Last edited on",
                disabled: true
            }
        });

        return fields;

    }

    private getFieldTemplateOptionType(type: Models.IPrimitiveType): string {
        switch (type) {
            case Models.IPrimitiveType.Date:
                return "date";
            case Models.IPrimitiveType.Number:
                return "number";
            default:
                return undefined;
        }
    }

    private createField(type: Models.IPropertyType): AngularFormly.IFieldConfigurationObject {
        let field: AngularFormly.IFieldConfigurationObject = {
            key: type.id.toString(),
            templateOptions: {
                label: type.name
            },
            data: type,
            expressionProperties: {},
        };
        switch (type.primitiveType) {
            case Models.IPrimitiveType.Text:
                field.type = type.isRichText ? "tinymceInline" : (type.isMultipleAllowed ? "textarea" : "input");
                field.defaultValue = type.stringDefaultValue;
                field.templateOptions.maxlength
                break;
            case Models.IPrimitiveType.Date:
                field.type = "input";
                field.templateOptions.type = "date"
                field.defaultValue = type.dateDefaultValue || new Date();
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
                field.defaultValue = (type.defaultValidValueIndex || 0).toString(),
                field.templateOptions.options = type.validValues.map(function (it, index) {
                    return <AngularFormly.ISelectOption>{ value: index.toString(), name: it};
                })
                break;
            default:
                return undefined;
        }
        return field;
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


            fields.systemFields = this.getSystemPropertyFileds(artifactType, project.meta);

            if (artifactType) {
                project.meta.propertyTypes.map((it: Models.IPropertyType) => {
                    if (artifactType.customPropertyTypeIds.indexOf(it.id) >= 0) {
                        let field = this.createField(it);
                        if (field) {
                            fields.customFields.push(field);
                        }
                    }
                });

            }
            fields.noteFields.push({
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
            fields.noteFields.push({
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

}