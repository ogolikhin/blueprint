import {ProcessShapeType} from "../../../../../models/enums";
import {IProcessShape} from "../../../../../models/process-models";
import {IUserTaskShape, ISystemTaskShape} from "../../../../../models/process-models";
import {IDiagramNode} from "../models/";
import {ShapesFactory} from "./shapes-factory";
import {ProcessStart} from "./process-start";
import {ProcessEnd} from "./process-end";
import {UserTask} from "./user-task";
import {SystemTask} from "./system-task";
import {UserDecision} from "./user-decision";
import {SystemDecision} from "./system-decision";
import {NodeFactorySettings} from "./node-factory-settings";
import {BpAccordionPanelService} from "../../../../../../../main/components/bp-accordion/bp-accordion";

export class NodeFactory {

    public static createNode(model: IProcessShape, rootScope: any,
                             shapesFactoryService: ShapesFactory,
                             nodeFactorySettings: NodeFactorySettings = null,
                             bpAccordionPanelService?: BpAccordionPanelService): IDiagramNode {

        const type = <ProcessShapeType>model.propertyValues["clientType"].value;
        switch (type) {
            case ProcessShapeType.Start:
                return new ProcessStart(model, nodeFactorySettings);

            case ProcessShapeType.End:
                return new ProcessEnd(model, nodeFactorySettings);

            case ProcessShapeType.UserTask:
                return new UserTask(<IUserTaskShape>model, rootScope, nodeFactorySettings, shapesFactoryService, bpAccordionPanelService);

            case ProcessShapeType.PreconditionSystemTask:
                return new SystemTask(
                    <ISystemTaskShape>model, rootScope, shapesFactoryService.NEW_SYSTEM_TASK_PERSONAREFERENCE, nodeFactorySettings, shapesFactoryService
                );

            case ProcessShapeType.SystemTask:
                return new SystemTask(
                    <ISystemTaskShape>model, rootScope, shapesFactoryService.NEW_SYSTEM_TASK_PERSONAREFERENCE, nodeFactorySettings, shapesFactoryService
                );

            case ProcessShapeType.UserDecision:
                return new UserDecision(model, rootScope, nodeFactorySettings);

            case ProcessShapeType.SystemDecision:
                return new SystemDecision(model, rootScope, nodeFactorySettings);

            default:
                return null;
        }
    }
}

