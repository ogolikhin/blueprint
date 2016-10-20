import {IProcessShape} from "../../diagram/presentation/graph/models/";
import {IModalDialogModel} from "./modal-dialog-model-interface";
import {DiagramNodeElement, UserTask, SystemTask} from "../../diagram/presentation/graph/shapes/";

export abstract class SubArtifactTaskDialogModel implements IModalDialogModel {
    
    isUserTask: boolean;
    isSystemTask: boolean;
    
    
    subArtifactId: number;
    isHistoricalVersion: boolean;
    isReadonly: boolean;
}

export class SubArtifactUserTaskDialogModel extends SubArtifactTaskDialogModel {
    clonedItem: UserTask;
    originalItem: UserTask;
}

export class SubArtifactSystemTaskDialogModel extends SubArtifactTaskDialogModel {
    clonedItem: SystemTask;
    originalItem: SystemTask;
}