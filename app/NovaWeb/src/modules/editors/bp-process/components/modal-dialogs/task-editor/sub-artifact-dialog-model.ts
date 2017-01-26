import {IProcessShape} from "../../diagram/presentation/graph/models/";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";
import {DiagramNodeElement, UserTask, SystemTask} from "../../diagram/presentation/graph/shapes/";
import {IArtifactReference} from "../../../models/process-models";


export interface IPersonaOption {
    value: IArtifactReference;
    label: string;
}

export abstract class TaskDialogModel implements IModalDialogModel {
    artifactId: number;
    subArtifactId: number;
    isHistoricalVersion: boolean;
    isReadonly: boolean;
    itemTypeId: number;
    action: string;
    associatedArtifact: IArtifactReference;
    personaReference: IArtifactReference;
    label: string;
}

export class UserTaskDialogModel extends TaskDialogModel {
    originalItem: UserTask;
    objective: string;
    userTaskPersonaReferenceOptions: IPersonaOption[];
}

export class SystemTaskDialogModel extends TaskDialogModel {
    originalItem: SystemTask;
    imageId: string;
    associatedImageUrl: string;
    systemTaskPersonaReferenceOptions: IPersonaOption[];
}
