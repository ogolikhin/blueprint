
module Storyteller {
    export interface IUserTaskChildElement  extends IDiagramNode {
        getUserTask(graph: ProcessGraph): IUserTask;
    }

    export class UserTaskChildElement<T extends IProcessShape> extends DiagramNode<T> implements IUserTaskChildElement {

        public getUserTask(graph: ProcessGraph): IUserTask {
            var sources = this.getSources(graph);
            if (sources) {
                var firstSource = sources[0];
                if (firstSource != null && firstSource.getNodeType() === NodeType.UserTask) {
                    return <IUserTask>firstSource;
                }
                var uTChildElement = <IUserTaskChildElement>firstSource;
                if (uTChildElement && uTChildElement.getUserTask) {
                    return uTChildElement.getUserTask(graph);
                }
            }
            return null;
        }
    }
};