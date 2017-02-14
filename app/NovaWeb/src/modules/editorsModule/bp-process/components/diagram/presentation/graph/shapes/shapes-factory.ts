import {PropertyTypePredefined} from "../../../../../../../main/models/enums";
import {ItemTypePredefined} from "../../../../../../../main/models/itemTypePredefined.enum";
import {IStatefulArtifact, IStatefulArtifactFactory} from "../../../../../../../managers/artifact-manager/";
import {ProcessShapeType, ProcessType} from "../../../../../models/enums";
import {
    IArtifactReference,
    IArtifactReferenceLink,
    IHashMapOfPropertyValues,
    IProcessShape,
    IPropertyValueInformation,
    ISystemTaskShape,
    IUserTaskShape,
    ProcessShapeModel,
    SystemTaskShapeModel,
    UserTaskShapeModel
} from "../../../../../models/process-models";
import {StatefulProcessSubArtifact} from "../../../../../process-subartifact";
import {IdGenerator} from "./id-generator";

export interface IPropertyNameConstantsInformation {
    key: string;
    name: string;
}

export class ShapesFactorySettings {
    private _userTaskPersona: IArtifactReference;
    private _systemTaskPersona: IArtifactReference;

    public get userTaskPersona(): IArtifactReference {
        return this._userTaskPersona;
    }

    public set userTaskPersona(value: IArtifactReference) {
        this._userTaskPersona = value;
    }

    public get systemTaskPersona(): IArtifactReference {
        return this._systemTaskPersona;
    }

    public set systemTaskPersona(value: IArtifactReference) {
        this._systemTaskPersona = value;
    }

    public destroy() {
        delete this._userTaskPersona;
        delete this._systemTaskPersona;
    }
}

export class ShapesFactory {

    private _idGenerator = new IdGenerator();

    public readonly NEW_USER_TASK_LABEL: string;
    public readonly NEW_USER_TASK_PERSONAREFERENCE: IArtifactReference;
    public readonly NEW_SYSTEM_TASK_LABEL: string;
    public readonly NEW_SYSTEM_TASK_PERSONAREFERENCE: IArtifactReference;
    public readonly NEW_USER_DECISION_LABEL: string;
    public readonly NEW_SYSTEM_DECISION_LABEL: string;
    public readonly NEW_MERGE_NODE_NAME: string;

    public ClientType: IPropertyNameConstantsInformation = {key: "clientType", name: "ClientType"};
    public X: IPropertyNameConstantsInformation = {key: "x", name: "X"};
    public Y: IPropertyNameConstantsInformation = {key: "y", name: "Y"};
    public Height: IPropertyNameConstantsInformation = {key: "height", name: "Height"};
    public Width: IPropertyNameConstantsInformation = {key: "width", name: "Width"};

    public Description: IPropertyNameConstantsInformation = {key: "description", name: "Description"};
    public Label: IPropertyNameConstantsInformation = {key: "label", name: "Label"};
    public Objective: IPropertyNameConstantsInformation = {key: "itemLabel", name: "ItemLabel"};
    public AssociatedImageUrl: IPropertyNameConstantsInformation = {
        key: "associatedImageUrl",
        name: "AssociatedImageUrl"
    };
    public ImageId: IPropertyNameConstantsInformation = {key: "imageId", name: "ImageId"};
    public StoryLinks: IPropertyNameConstantsInformation = {key: "storyLinks", name: "StoryLinks"};

    private settings = new ShapesFactorySettings();

    public static $inject = ["$rootScope", "statefulArtifactFactory"];

    constructor(private $rootScope: ng.IRootScopeService, private statefulArtifactFactory: IStatefulArtifactFactory) {

        let definedSconfig = false;
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

        if (this.NEW_USER_TASK_PERSONAREFERENCE == null) {
            this.NEW_USER_TASK_PERSONAREFERENCE = {
                id: this._idGenerator.getUserPeronaId(),
                projectId: null,
                name: "",
                typePrefix: null,
                baseItemTypePredefined: ItemTypePredefined.Actor,
                projectName: null,
                link: null,
                version: null
            };
            if (definedSconfig) {
                this.NEW_USER_TASK_PERSONAREFERENCE.name = (<any>this.$rootScope).config.labels["ST_New_User_Task_Persona"]; //"User";
            }
        }

        if (this.NEW_SYSTEM_TASK_LABEL == null) {
            if (definedSconfig) {
                this.NEW_SYSTEM_TASK_LABEL = (<any>this.$rootScope).config.labels["ST_New_System_Task_Label"]; //"New System Task";
            } else {
                this.NEW_SYSTEM_TASK_LABEL = "";
            }
        }

        if (this.NEW_SYSTEM_TASK_PERSONAREFERENCE == null) {
            this.NEW_SYSTEM_TASK_PERSONAREFERENCE = {
                id: this._idGenerator.getUserPeronaId(),
                projectId: null,
                name: "",
                typePrefix: null,
                baseItemTypePredefined: ItemTypePredefined.Actor,
                projectName: null,
                link: null,
                version: null
            };

            if (definedSconfig) {
                this.NEW_SYSTEM_TASK_PERSONAREFERENCE.name = (<any>this.$rootScope).config.labels["ST_New_System_Task_Persona"]; //"User";
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

    public setUserTaskPersona(value: IArtifactReference): void {
        this.settings.userTaskPersona = value;
    }

    public setSystemTaskPersona(value: IArtifactReference): void {
        this.settings.systemTaskPersona = value;
    }

    public createStatefulSubArtifact(artifact: IStatefulArtifact, subartifact: IProcessShape): StatefulProcessSubArtifact {
        const statefulArtifact = this.statefulArtifactFactory.createStatefulProcessSubArtifact(artifact, subartifact);
        return statefulArtifact;
    }

    public createModelMergeNodeShape(parentId: number, projectId: number, id: number, x: number, y: number) {
        const nameCounter = this._idGenerator.getId(ProcessShapeType.None);

        const obj = new ProcessShapeModel(id, this.NEW_MERGE_NODE_NAME + nameCounter, projectId, "", parentId, ItemTypePredefined.None);
        obj.propertyValues = this.createPropertyValuesFormergePointShape(obj.name, "", x, y);

        return obj;
    }

    public createModelUserTaskShape(parentId: number, projectId: number, id: number, x: number, y: number): IUserTaskShape {
        // hard coded strings, if change, please search above chars and replace the other place on server side
        // replace "Process_DefaultUserTask_Name" in StringTokens.resx
        // see https://trello.com/c/k6UpxuGi

        const nameCounter = this._idGenerator.getId(ProcessShapeType.UserTask);

        const tempUserTaskName = this.NEW_USER_TASK_LABEL + " " + nameCounter;

        let defaultUserPersonaReference = this.NEW_USER_TASK_PERSONAREFERENCE;

        if (!!this.settings.userTaskPersona) {
            defaultUserPersonaReference = this.settings.userTaskPersona;
        }

        const shapeModel = new UserTaskShapeModel(
            id, tempUserTaskName, projectId, "PROS", parentId, ItemTypePredefined.PROShape, null, defaultUserPersonaReference
        );
        shapeModel.propertyValues = this.createPropertyValuesForUserTaskShape([], "", "", x, y, -1, -1, "");

        return shapeModel;
    }

    public createModelSystemTaskShape(parentId: number, projectId: number, id: number, x: number, y: number): ISystemTaskShape {
        // hard coded strings, if change, please search above chars and replace the other place on server side
        // replace "Process_DefaultSystemTask_Name" in StringTokens.resx
        // see https://trello.com/c/k6UpxuGi

        const nameCounter = this._idGenerator.getId(ProcessShapeType.SystemTask);
        const tempSystemTaskName = this.NEW_SYSTEM_TASK_LABEL + " " + nameCounter;

        let defaultSystemPersonaReference = this.NEW_SYSTEM_TASK_PERSONAREFERENCE;

        if (!!this.settings.systemTaskPersona) {
            defaultSystemPersonaReference = this.settings.systemTaskPersona;
        }

        const shapeModel = new SystemTaskShapeModel(
            id, tempSystemTaskName, projectId, "PROS", parentId, ItemTypePredefined.PROShape, null, defaultSystemPersonaReference
        );

        shapeModel.propertyValues = this.createPropertyValuesForSystemTaskShape([], -1, null, "", "", x, y, -1, -1, "", null);

        return shapeModel;
    }

    public createModelUserDecisionShape(parentId: number, projectId: number, id: number, x: number, y: number): IProcessShape {
        const nameCounter = this._idGenerator.getId(ProcessShapeType.UserDecision);
        const obj = new ProcessShapeModel(id, this.NEW_USER_DECISION_LABEL + nameCounter, projectId, "PROS", parentId,
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
        const nameCounter = this._idGenerator.getId(ProcessShapeType.SystemDecision);
        const model = new ProcessShapeModel(id, this.NEW_SYSTEM_DECISION_LABEL + nameCounter, projectId, "PROS", parentId, ItemTypePredefined.PROShape);

        model.propertyValues = this.createPropertyValuesForSystemDecisionShape(this.NEW_SYSTEM_DECISION_LABEL + nameCounter, "", x, y, -1, -1, "");

        return model;
    }

    public createPropertyValuesForUserTaskShape(inputParameters: string[] = [],
                                                label: string = this.NEW_USER_TASK_LABEL,
                                                description: string = "",
                                                x: number = 0,
                                                y: number = 0,
                                                width: number = -1,
                                                height: number = -1,
                                                objective: string = "",
                                                include: IArtifactReference = null): IHashMapOfPropertyValues {
        const propertyValues: IHashMapOfPropertyValues = {};

        propertyValues[this.Label.key] = this.createLabelValue(label);
        propertyValues[this.Description.key] = this.createDescriptionValue(description);
        propertyValues[this.X.key] = this.createXValue(x);
        propertyValues[this.Y.key] = this.createYValue(y);
        propertyValues[this.Width.key] = this.createWidhtValue(width);
        propertyValues[this.Height.key] = this.createHeightValue(height);
        propertyValues[this.ClientType.key] = this.createClientTypeValue(ProcessShapeType.UserTask);
        propertyValues[this.Objective.key] = this.createObjectiveValue(objective);
        propertyValues[this.StoryLinks.key] = this.createStoryLinksValue(null);

        return propertyValues;
    }

    public createPropertyValuesForSystemTaskShape(outputParameters: string[] = [],
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
        const propertyValues: IHashMapOfPropertyValues = {};

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

        return propertyValues;
    }

    public createPropertyValuesForUserDecisionShape(label: string = this.NEW_USER_DECISION_LABEL,
                                                    description: string = "",
                                                    x: number = 0,
                                                    y: number = 0,
                                                    width: number = -1,
                                                    height: number = -1,
                                                    objective: string = ""): IHashMapOfPropertyValues {
        const propertyValues: IHashMapOfPropertyValues = {};

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
        const propertyValues: IHashMapOfPropertyValues = {};

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
        const propertyValues: IHashMapOfPropertyValues = {};

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
            typePredefined: PropertyTypePredefined.ImageId,
            typeId: -1,
            value: imageId
        };
    }

    public createStoryLinksValue(storyLinks: IArtifactReferenceLink): IPropertyValueInformation {
        return {
            propertyName: this.StoryLinks.name,
            typePredefined: PropertyTypePredefined.StoryLink,
            typeId: -1,
            value: storyLinks
        };
    }

    public reset() {
        if (this._idGenerator) {
            this._idGenerator.reset();
        }

        if (this.settings) {
            this.settings.destroy();
        }
    }

    public destroy() {
        if (this.settings) {
            this.settings.destroy();
        }
    }
}

export class ShapesFactoryMock {
}
