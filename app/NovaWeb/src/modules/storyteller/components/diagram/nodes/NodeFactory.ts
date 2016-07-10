module Storyteller {

    export class NodeFactory {

        public static createNode(model: IProcessShape, rootScope: any, shapesFactoryService: ShapesFactoryService, nodeFactorySettings: NodeFactorySettings = null): IDiagramNode {
            var type = <ProcessShapeType>model.propertyValues["clientType"].value;
            switch (type) {
                case ProcessShapeType.Start:
                    return new ProcessStart(model, nodeFactorySettings);

                case ProcessShapeType.End:
                    return new ProcessEnd(model, nodeFactorySettings); 

                case ProcessShapeType.UserTask:
                    return new UserTask(<IUserTaskShape>model, rootScope, nodeFactorySettings, shapesFactoryService);

                case ProcessShapeType.PreconditionSystemTask:
                    return new SystemTask(<ISystemTaskShape>model, rootScope, shapesFactoryService.NEW_SYSTEM_TASK_LABEL, nodeFactorySettings, shapesFactoryService);

                case ProcessShapeType.SystemTask:
                    return new SystemTask(<ISystemTaskShape>model, rootScope, shapesFactoryService.NEW_SYSTEM_TASK_LABEL, nodeFactorySettings, shapesFactoryService);

                case ProcessShapeType.UserDecision:
                    return new UserDecision(model, rootScope, nodeFactorySettings);

                case ProcessShapeType.SystemDecision:
                    return new SystemDecision(model, rootScope, nodeFactorySettings);

                default: 
                    return null;
            }
        }
    }

    export class NodeFactorySettings {
        public isCommentsButtonEnabled: boolean;
        public isRelationshipButtonEnabled: boolean;
        public isLinkButtonEnabled: boolean;
        public isPreviewButtonEnabled: boolean;
        public isMockupButtonEnabled: boolean;
        public isDetailsButtonEnabled: boolean;
    }
}
