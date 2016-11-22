import {IProcessShape} from "../../diagram/presentation/graph/models/";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";
import {DiagramNodeElement, UserTask, SystemTask} from "../../diagram/presentation/graph/shapes/";
import {IArtifactReference} from "../../../models/process-models";


export interface IPersonaOption {
    value: IArtifactReference;
    label: string;
}

export abstract class SubArtifactTaskDialogModel implements IModalDialogModel {
    artifactId: number;
    subArtifactId: number;
    isHistoricalVersion: boolean;
    isReadonly: boolean;
}

export class UserTaskDialogModel extends SubArtifactTaskDialogModel {
    originalItem: UserTask;
    action: string;
    associatedArtifact: IArtifactReference;
    personaReference: IArtifactReference;
    label: string;
    objective: string;
    userTaskPersonaReferenceOptions: IPersonaOption[];
}

export class SystemTaskDialogModel extends SubArtifactTaskDialogModel {
    originalItem: SystemTask;
    action: string;
    associatedArtifact: IArtifactReference;
    personaReference: IArtifactReference;
    label: string;
    imageId: string;
    associatedImageUrl: string;
    systemTaskPersonaReferenceOptions: IPersonaOption[];
}
