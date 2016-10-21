import {IProcessShape} from "../../diagram/presentation/graph/models/";
import {IModalDialogModel} from "./modal-dialog-model-interface";
import {DiagramNodeElement, UserTask, SystemTask} from "../../diagram/presentation/graph/shapes/";
import {IArtifactReference} from "../../../models/process-models";

export abstract class SubArtifactTaskDialogModel implements IModalDialogModel {
    
    isUserTask: boolean;
    isSystemTask: boolean;
    
    
    subArtifactId: number;
    isHistoricalVersion: boolean;
    isReadonly: boolean;
}

export class TaskModalModel {
    persona: string;
    action: string;
    associatedArtifact: IArtifactReference;
    label: string;
}

export class UserTaskModalModel extends TaskModalModel {
    objective: string;
}

export class SystemTaskModalModel extends TaskModalModel {
    imageId: string;
    associatedImageUrl: string;
}

export class SubArtifactUserTaskDialogModel extends SubArtifactTaskDialogModel {
    clonedItem: UserTaskModalModel;
    originalItem: UserTask;
}

export class SubArtifactSystemTaskDialogModel extends SubArtifactTaskDialogModel {
    clonedItem: SystemTaskModalModel;
    originalItem: SystemTask;
}