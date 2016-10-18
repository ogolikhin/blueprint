import {
    IProcessGraph,
    IDiagramNode,
    ICondition,
    IDecision
} from "../../diagram/presentation/graph/models/process-graph-interfaces";
import {UserTask, SystemTask} from "../../diagram/presentation/graph/shapes/";
import {IProcessShape, NodeType} from "../../diagram/presentation/graph/models/";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";

export class DecisionEditorModel implements IModalDialogModel {
    public clonedUserTask: UserTask;
    public originalUserTask: UserTask;

    // new conditions to be added to Graph upon OK click
    public conditions: ICondition[];

    public graph: IProcessGraph;
    public isReadonly: boolean;
    public isHistoricalVersion: boolean;

    public label: string;
    public originalDecision: IDecision;

    public subArtifactId: number;
    public nextNode: IProcessShape;
    public propertiesMw: any; //TODO correct interface required! 
    public tabClick: Function;
    public systemNodeVisible: boolean;
}
