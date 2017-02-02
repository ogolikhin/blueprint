import {SystemTask} from "../../diagram/presentation/graph/shapes/system-task";
import {IPersonaOption, TaskDialogModel} from "./taskDialogModel";

export class SystemTaskDialogModel extends TaskDialogModel {
    originalItem: SystemTask;
    imageId: string;
    associatedImageUrl: string;
    systemTaskPersonaReferenceOptions: IPersonaOption[];
}
