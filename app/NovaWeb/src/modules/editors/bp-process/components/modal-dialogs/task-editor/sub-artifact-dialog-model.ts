import {IProcessShape} from "../../diagram/presentation/graph/models/";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";
import {DiagramNodeElement, UserTask, SystemTask} from "../../diagram/presentation/graph/shapes/";
import {IArtifactReference} from "../../../models/process-models";

export abstract class SubArtifactTaskDialogModel implements IModalDialogModel {
    subArtifactId: number;
    isHistoricalVersion: boolean;
    isReadonly: boolean;
}

export class UserTaskDialogModel extends SubArtifactTaskDialogModel {
    originalItem: UserTask;
    persona: string;
    action: string;
    associatedArtifact: IArtifactReference;
    label: string;
    objective: string;
}

export class SystemTaskDialogModel extends SubArtifactTaskDialogModel {
    originalItem: SystemTask;
    persona: string;
    action: string;
    associatedArtifact: IArtifactReference;
    label: string;
    imageId: string;
    associatedImageUrl: string;
}
