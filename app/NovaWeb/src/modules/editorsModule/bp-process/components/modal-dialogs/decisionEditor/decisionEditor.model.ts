import {ICondition} from "../../diagram/presentation/graph/shapes/condition";
import {
    IProcessGraph,
    IDecision
} from "../../diagram/presentation/graph/models/process-graph-interfaces";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";

export class DecisionEditorModel implements IModalDialogModel {
    public artifactId: number;
    public subArtifactId: number;
    public label: string;
    public conditions: ICondition[];
    public graph: IProcessGraph;
    public originalDecision: IDecision;
    public isReadonly: boolean;
    public isHistoricalVersion: boolean;
    public conditionHeader: string;
    public defaultDestinationId: number;
}
