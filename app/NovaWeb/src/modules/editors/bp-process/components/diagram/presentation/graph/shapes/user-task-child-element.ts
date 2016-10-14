import {IProcessShape} from "../../../../../models/process-models";
import {IProcessGraph} from "../models/";
import {IUserTaskChildElement, IUserTask} from "../models/";
import {NodeType} from "../models/";
import {DiagramNode} from "./diagram-node";

export class UserTaskChildElement<T extends IProcessShape> extends DiagramNode<T> implements IUserTaskChildElement {

    public getUserTask(graph: IProcessGraph): IUserTask {
        const sources = this.getSources(graph.getMxGraphModel());
        if (sources) {
            const firstSource = sources[0];
            if (firstSource != null && firstSource.getNodeType() === NodeType.UserTask) {
                return <IUserTask>firstSource;
            }
            const uTChildElement = <IUserTaskChildElement>firstSource;
            if (uTChildElement && uTChildElement.getUserTask) {
                return uTChildElement.getUserTask(graph);
            }
        }
        return null;
    }
}
