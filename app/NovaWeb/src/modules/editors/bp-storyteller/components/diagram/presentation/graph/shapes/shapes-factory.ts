﻿import {ItemTypePredefined, PropertyTypePredefined} from "../../../../../../../main/models/enums";
import {ProcessShapeType, ProcessType} from "../../../../../models/enums";
import {ProcessShapeModel, UserTaskShapeModel} from "../../../../../models/processModels";
import {SystemTaskShapeModel, IHashMapOfPropertyValues} from "../../../../../models/processModels";
import {IArtifactReference, IArtifactReferenceLink} from "../../../../../models/processModels";
import {IPropertyValueInformation, IProcessShape} from "../../../../../models/processModels";
import {ISystemTaskShape, IUserTaskShape} from "../../../../../models/processModels";
import {IdGenerator} from "./id-generator";

export interface IPropertyNameConstantsInformation {
    key: string;
    name: string;
}

class ShapesFactorySettings {
    private _userTaskPersona = null;
    private _systemTaskPersona = null;

    public getUserTaskPersona(): string {
        return this._userTaskPersona;
    }
    public setUserTaskPersona(value): void {
        this._userTaskPersona = value;
    }

    public getSystemTaskPersona(): string {
        return this._systemTaskPersona;
    }
    public setSystemTaskPersona(value): void {
        this._systemTaskPersona = value;
    }
    public destroy() {
        this._userTaskPersona = null;
        this._systemTaskPersona = null;
    }
}

export class ShapesFactory {

    private _idGenerator = new IdGenerator();

    public NEW_USER_TASK_LABEL: string;
    public NEW_USER_TASK_PERSONA: string;
    public NEW_SYSTEM_TASK_LABEL: string;
    public NEW_SYSTEM_TASK_PERSONA: string;
    public NEW_USER_DECISION_LABEL: string;
    public NEW_SYSTEM_DECISION_LABEL: string;
    public NEW_MERGE_NODE_NAME: string;

    public Persona: IPropertyNameConstantsInformation = { key: "persona", name: "Persona" };
    public ClientType: IPropertyNameConstantsInformation = { key: "clientType", name: "ClientType" };
    public X: IPropertyNameConstantsInformation = { key: "x", name: "X" };
    public Y: IPropertyNameConstantsInformation = { key: "y", name: "Y" };
    public Height: IPropertyNameConstantsInformation = { key: "height", name: "Height" };
    public Width: IPropertyNameConstantsInformation = { key: "width", name: "Width" };

    public Description: IPropertyNameConstantsInformation = { key: "description", name: "Description" };
    public Label: IPropertyNameConstantsInformation = { key: "label", name: "Label" };
    public Objective: IPropertyNameConstantsInformation = { key: "itemLabel", name: "ItemLabel" };
    public AssociatedImageUrl: IPropertyNameConstantsInformation = { key: "associatedImageUrl", name: "AssociatedImageUrl" };
    public ImageId: IPropertyNameConstantsInformation = { key: "imageId", name: "ImageId" };
    public Include: IPropertyNameConstantsInformation = { key: "include", name: "Include" };
    public StoryLinks: IPropertyNameConstantsInformation = { key: "storyLinks", name: "StoryLinks" };

    public static $inject = ["$rootScope"];

    private settings = new ShapesFactorySettings();

    public setUserTaskPersona(value) {
        this.settings.setUserTaskPersona(value);
    }

    public setSystemTaskPersona(value) {
        this.settings.setSystemTaskPersona(value);
    }

    constructor(private $rootScope: ng.IRootScopeService) {

        var definedSconfig = false;
        if ((<any>this.$rootScope) !== undefined
            && (<any>this.$rootScope).config !== undefined
            && (<any>this.$rootScope).config.labels !== undefined) {
            definedSconfig = true;
        }

        if (this.NEW_USER_TASK_LABEL == null) {
            if (definedSconfig) {
                this.NEW_USER_TASK_LABEL = (<any>this.$rootScope).config.labels["ST_New_User_Task_Label"]; //"New User Task";
            } else {
                this.NEW_USER_TASK_LABEL = "";
            }
        }

        if (this.NEW_USER_TASK_PERSONA == null) {
            if (definedSconfig) {
                this.NEW_USER_TASK_PERSONA = (<any>this.$rootScope).config.labels["ST_New_User_Task_Persona"]; //"User";
            } else {
                this.NEW_USER_TASK_PERSONA = "";
            }
        }

        if (this.NEW_SYSTEM_TASK_LABEL == null) {
            if (definedSconfig) {
                this.NEW_SYSTEM_TASK_LABEL = (<any>this.$rootScope).config.labels["ST_New_System_Task_Label"]; //"New System Task";
            } else {
                this.NEW_SYSTEM_TASK_LABEL = "";
            }
        }

        if (this.NEW_SYSTEM_TASK_PERSONA == null) {
            if (definedSconfig) {
                this.NEW_SYSTEM_TASK_PERSONA = (<any>this.$rootScope).config.labels["ST_New_System_Task_Persona"]; //"System";
            } else {
                this.NEW_SYSTEM_TASK_PERSONA = "";
            }
        }

        if (this.NEW_USER_DECISION_LABEL == null) {
            if (definedSconfig) {
                this.NEW_USER_DECISION_LABEL = (<any>this.$rootScope).config.labels["ST_New_User_Decision_Label"]; //"User Decision";
            } else {
                this.NEW_USER_DECISION_LABEL = "";
            }
        }

        if (this.NEW_SYSTEM_DECISION_LABEL == null) {
            if (definedSconfig) {
                this.NEW_SYSTEM_DECISION_LABEL = (<any>this.$rootScope).config.labels["ST_New_System_Decision_Label"]; //"System Decision";
            } else {
                this.NEW_SYSTEM_DECISION_LABEL = "";
            }
        }

        if (this.NEW_MERGE_NODE_NAME == null) {
            if (definedSconfig) {
                this.NEW_MERGE_NODE_NAME = (<any>this.$rootScope).config.labels["ST_NEW_MERGE_NODE_NAME"];
            } else {
                this.NEW_MERGE_NODE_NAME = "";
            }
        }
    }

    public createModelMergeNodeShape(parentId: number, projectId: number, id: number, x: number, y: number) {
        var nameCounter = this._idGenerator.getId(ProcessShapeType.None);

        var obj = new ProcessShapeModel(id, this.NEW_MERGE_NODE_NAME + nameCounter, projectId, "", parentId, ItemTypePredefined.None);
        obj.propertyValues = this.createPropertyValuesFormergePointShape(obj.name, "", x, y);

        return obj;
    }

    public createModelUserTaskShape(parentId: number, projectId: number, id: number, x: number, y: number): IUserTaskShape {
        var nameCounter = this._idGenerator.getId(ProcessShapeType.UserTask);

        // hard coded strings, if change, please search above chars and replace the other place on server side
        // replace "Storyteller_DefaultUserTask_Name" in StringTokens.resx
        // see https://trello.com/c/k6UpxuGi

        var tempUserTaskName = this.NEW_USER_TASK_LABEL + nameCounter;
        var obj = new UserTaskShapeModel(id, tempUserTaskName, projectId, "PROS", parentId,
            ItemTypePredefined.PROShape);

        let persona = this.NEW_USER_TASK_PERSONA;
        if (!!this.settings.getUserTaskPersona()) {
            persona = this.settings.getUserTaskPersona();
        }

        obj.propertyValues = this.createPropertyValuesForUserTaskShape(persona, [], "",
            "", x, y, -1, -1, "");

        return obj;
    }

    public createModelSystemTaskShape(parentId: number, projectId: number, id: number, x: number, y: number): ISystemTaskShape {
        var nameCounter = this._idGenerator.getId(ProcessShapeType.SystemTask);

        // hard coded strings, if change, please search above chars and replace the other place on server side
        // replace "Storyteller_DefaultSystemTask_Name" in StringTokens.resx
        // see https://trello.com/c/k6UpxuGi
        var tempSystemTaskName = this.NEW_SYSTEM_TASK_LABEL + nameCounter;
        var obj = new SystemTaskShapeModel(id, tempSystemTaskName, projectId, "PROS", parentId, ItemTypePredefined.PROShape);

        let persona = this.NEW_SYSTEM_TASK_PERSONA;
        if (!!this.settings.getSystemTaskPersona()) {
            persona = this.settings.getSystemTaskPersona();
        }

        obj.propertyValues = this.createPropertyValuesForSystemTaskShape(persona, [], -1,
            null, "", "", x, y, -1, -1, "", null);

        return obj;
    }

    public createModelUserDecisionShape(parentId: number, projectId: number, id: number, x: number, y: number): IProcessShape {
        var nameCounter = this._idGenerator.getId(ProcessShapeType.UserDecision);
        var obj = new ProcessShapeModel(id, this.NEW_USER_DECISION_LABEL + nameCounter, projectId, "PROS", parentId,
            ItemTypePredefined.PROShape);

        obj.propertyValues = this.createPropertyValuesForUserDecisionShape(this.NEW_USER_DECISION_LABEL + nameCounter, "", x, y, -1, -1, "");

        return obj;
    }

    public createModelSystemDecisionShape(parentId: number, projectId: number, id: number, x: number, y: number): IProcessShape {
        const nameCounter = this._idGenerator.getId(ProcessShapeType.SystemDecision);
        const obj = new ProcessShapeModel(id, this.NEW_SYSTEM_DECISION_LABEL + nameCounter, projectId, "PROS", parentId,
            ItemTypePredefined.PROShape);

        obj.propertyValues = this.createPropertyValuesForSystemDecisionShape(this.NEW_SYSTEM_DECISION_LABEL + nameCounter, "", x, y, -1, -1, "");

        return obj;
    }


    public createSystemDecisionShapeModel(id: number, parentId: number, projectId: number, x: number, y: number): IProcessShape {
        var nameCounter = this._idGenerator.getId(ProcessShapeType.SystemDecision);
        var model = new ProcessShapeModel(id, this.NEW_SYSTEM_DECISION_LABEL + nameCounter, projectId, "PROS", parentId, ItemTypePredefined.PROShape);

        model.propertyValues = this.createPropertyValuesForSystemDecisionShape(this.NEW_SYSTEM_DECISION_LABEL + nameCounter, "", x, y, -1, -1, "");

        return model;
    }

    public createPropertyValuesForUserTaskShape(persona: string = this.NEW_USER_TASK_PERSONA,
        inputParameters: string[] = [],
        label: string = this.NEW_USER_TASK_LABEL,
        description: string = "",
        x: number = 0,
        y: number = 0,
        width: number = -1,
        height: number = -1,
        objective: string = "",
        include: IArtifactReference = null): IHashMapOfPropertyValues {
        var propertyValues: IHashMapOfPropertyValues = {};

        propertyValues[this.Persona.key] = this.createPersonaValue(persona);
        propertyValues[this.Label.key] = this.createLabelValue(label);
        propertyValues[this.Description.key] = this.createDescriptionValue(description);
        propertyValues[this.X.key] = this.createXValue(x);
        propertyValues[this.Y.key] = this.createYValue(y);
        propertyValues[this.Width.key] = this.createWidhtValue(width);
        propertyValues[this.Height.key] = this.createHeightValue(height);
        propertyValues[this.ClientType.key] = this.createClientTypeValue(ProcessShapeType.UserTask);
        propertyValues[this.Objective.key] = this.createObjectiveValue(objective);
        propertyValues[this.Include.key] = this.createIncludeValue(include);

        return propertyValues;
    }

    public createPropertyValuesForSystemTaskShape(persona: string = this.NEW_SYSTEM_TASK_PERSONA,
        outputParameters: string[] = [],
        userTaskId: number = -1,
        associatedImageUrl: string = "",
        label: string = this.NEW_SYSTEM_TASK_LABEL,
        description: string = "",
        x: number = 0,
        y: number = 0,
        width: number = -1,
        height: number = -1,
        objective: string = "",
        include: IArtifactReference = null): IHashMapOfPropertyValues {
        var propertyValues: IHashMapOfPropertyValues = {};

        propertyValues[this.Persona.key] = this.createPersonaValue(persona);
        propertyValues[this.AssociatedImageUrl.key] = this.createAssociatedImageUrlValue();
        propertyValues[this.ImageId.key] = this.createImageIdValue();
        propertyValues[this.Label.key] = this.createLabelValue(label);
        propertyValues[this.Description.key] = this.createDescriptionValue(description);
        propertyValues[this.X.key] = this.createXValue(x);
        propertyValues[this.Y.key] = this.createYValue(y);
        propertyValues[this.Width.key] = this.createWidhtValue(width);
        propertyValues[this.Height.key] = this.createHeightValue(height);
        propertyValues[this.ClientType.key] = this.createClientTypeValue(ProcessShapeType.SystemTask);
        propertyValues[this.Objective.key] = this.createObjectiveValue(objective);
        propertyValues[this.Include.key] = this.createIncludeValue(include);

        return propertyValues;
    }

    public createPropertyValuesForUserDecisionShape(label: string = this.NEW_USER_DECISION_LABEL,
        description: string = "",
        x: number = 0,
        y: number = 0,
        width: number = -1,
        height: number = -1,
        objective: string = ""): IHashMapOfPropertyValues {
        var propertyValues: IHashMapOfPropertyValues = {};

        propertyValues[this.Label.key] = this.createLabelValue(label);
        propertyValues[this.Description.key] = this.createDescriptionValue(description);
        propertyValues[this.X.key] = this.createXValue(x);
        propertyValues[this.Y.key] = this.createYValue(y);
        propertyValues[this.Width.key] = this.createWidhtValue(width);
        propertyValues[this.Height.key] = this.createHeightValue(height);
        propertyValues[this.ClientType.key] = this.createClientTypeValue(ProcessShapeType.UserDecision);
        propertyValues[this.Objective.key] = this.createObjectiveValue(objective);

        return propertyValues;
    }

    public createPropertyValuesForSystemDecisionShape(label: string = this.NEW_SYSTEM_DECISION_LABEL,
        description: string = "",
        x: number = 0,
        y: number = 0,
        width: number = -1,
        height: number = -1,
        objective: string = ""): IHashMapOfPropertyValues {
        var propertyValues: IHashMapOfPropertyValues = {};

        propertyValues[this.Label.key] = this.createLabelValue(label);
        propertyValues[this.Description.key] = this.createDescriptionValue(description);
        propertyValues[this.X.key] = this.createXValue(x);
        propertyValues[this.Y.key] = this.createYValue(y);
        propertyValues[this.Width.key] = this.createWidhtValue(width);
        propertyValues[this.Height.key] = this.createHeightValue(height);
        propertyValues[this.ClientType.key] = this.createClientTypeValue(ProcessShapeType.SystemDecision);
        propertyValues[this.Objective.key] = this.createObjectiveValue(objective);

        return propertyValues;
    }

    public createPropertyValuesFormergePointShape(label: string = this.NEW_MERGE_NODE_NAME,
        description: string = "",
        x: number = 0,
        y: number = 0,
        width: number = -1,
        height: number = -1): IHashMapOfPropertyValues {
        var propertyValues: IHashMapOfPropertyValues = {};

        propertyValues[this.Label.key] = this.createLabelValue(label);
        propertyValues[this.Description.key] = this.createDescriptionValue(description);
        propertyValues[this.X.key] = this.createXValue(x);
        propertyValues[this.Y.key] = this.createYValue(y);
        propertyValues[this.Width.key] = this.createWidhtValue(width);
        propertyValues[this.Height.key] = this.createHeightValue(height);
        propertyValues[this.ClientType.key] = this.createClientTypeValue(ProcessShapeType.UserDecision);

        return propertyValues;
    }

    public createLabelValue(label: string): IPropertyValueInformation {
        return {
            propertyName: this.Label.name,
            typePredefined: PropertyTypePredefined.Label,
            typeId: -1,
            value: label
        };
    }

    public createDescriptionValue(description: string): IPropertyValueInformation {
        return {
            propertyName: this.Description.name,
            typePredefined: PropertyTypePredefined.Description,
            typeId: -1,
            value: description
        };
    }

    public createXValue(x: number): IPropertyValueInformation {
        return {
            propertyName: this.X.name,
            typePredefined: PropertyTypePredefined.X,
            typeId: -1,
            value: x
        };
    }

    public createYValue(y: number): IPropertyValueInformation {
        return {
            propertyName: this.Y.name,
            typePredefined: PropertyTypePredefined.Y,
            typeId: -1,
            value: y
        };
    }

    public createWidhtValue(width: number): IPropertyValueInformation {
        return {
            propertyName: this.Width.name,
            typePredefined: PropertyTypePredefined.Width,
            typeId: -1,
            value: width
        };
    }

    public createHeightValue(height: number): IPropertyValueInformation {
        return {
            propertyName: this.Height.name,
            typePredefined: PropertyTypePredefined.Height,
            typeId: -1,
            value: height
        };
    }

    public createClientTypeValue(clientType: ProcessShapeType): IPropertyValueInformation {
        return {
            propertyName: this.ClientType.name,
            typePredefined: PropertyTypePredefined.ClientType,
            typeId: -1,
            value: clientType
        };
    }

    public createClientTypeValueForProcess(clientType: ProcessType) {
        return {
            propertyName: this.ClientType.name,
            typePredefined: PropertyTypePredefined.ClientType,
            typeId: -1,
            value: clientType
        };
    }

    public createObjectiveValue(objective: string): IPropertyValueInformation {
        return {
            propertyName: this.Objective.name,
            typePredefined: PropertyTypePredefined.ItemLabel,
            typeId: -1,
            value: objective
        };
    }

    public createAssociatedImageUrlValue(url: string = ""): IPropertyValueInformation {
        return {
            propertyName: this.AssociatedImageUrl.name,
            typePredefined: PropertyTypePredefined.None,
            typeId: -1,
            value: url
        };
    }
    public createImageIdValue(imageId: string = ""): IPropertyValueInformation {
        return {
            propertyName: this.ImageId.name,
            typePredefined: PropertyTypePredefined.None,
            typeId: -1,
            value: imageId
        };
    }

    public createPersonaValue(persona: string): IPropertyValueInformation {
        return {
            propertyName: this.Persona.name,
            typePredefined: PropertyTypePredefined.None,
            typeId: -1,
            value: persona

        };
    }

    public createIncludeValue(includeValue: IArtifactReference): IPropertyValueInformation {
        return {
            propertyName: this.Include.name,
            typePredefined: PropertyTypePredefined.None,
            typeId: -1,
            value: includeValue
        };
    }

    public createStoryLinksValue(storyLinks: IArtifactReferenceLink): IPropertyValueInformation {
        return {
            propertyName: this.StoryLinks.name,
            typePredefined: PropertyTypePredefined.None,
            typeId: -1,
            value: storyLinks
        };
    }

    public destroy() {
        if (this.settings) {
            this.settings.destroy();
        }
    }

}
