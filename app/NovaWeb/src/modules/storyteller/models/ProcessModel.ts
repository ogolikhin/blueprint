module Storyteller {

    export interface IProcessLinkModel extends IProcessLink {
        parentId: number;
        sourceNode: any;
        destinationNode: any;
    }

    export class ProcessModel implements IProcess {
        public status: IItemStatus;
        constructor(
            public id: number = 0,
            public name: string = "",
            public typePrefix: string = "",
            public projectId: number = 0,
            public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.Process,
            public shapes: IProcessShape[] = [],
            public links: IProcessLinkModel[] = [],
            public propertyValues: IHashMapOfPropertyValues = {},
            public decisionBranchDestinationLinks: IProcessLink[] = [],
            public itemTypeId: number = 0,
            status?: IItemStatus,
            public requestedVersionInfo: IVersionInfo = null) {
            this.status = status || <IItemStatus>{};
        }
    }

    export class ProcessShapeModel implements IProcessShape {
        constructor(
            public id: number = 0,
            public name: string = "",
            public projectId: number = 0,
            public typePrefix: string = "",
            public parentId: number = 0,
            public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.PROShape,
            public associatedArtifact: IArtifactReference = null,
            public propertyValues: IHashMapOfPropertyValues = {},
            public branchDestinationId: number = undefined,
            public flags: ITaskFlags = <ITaskFlags>{},
            public decisionSourceIds: number[] = []) {
        }
    }

    export class ProcessFlowModel implements IProcessFlow {
        constructor(
            public parentFlow: IProcessFlow = null,
            public orderIndex: number = 0,
            public startShapeId: number = null,
            public endShapeId: number = null,
            public shapes: Shell.IHashMap<IProcessShape> = {}
        ) {
        }
    }

    export class TaskShapeModel extends ProcessShapeModel implements ITaskShape {
        constructor(
            public id: number = 0,
            public name: string = "",
            public projectId: number = 0,
            public typePrefix: string = "",
            public parentId: number = 0,
            public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.PROShape,
            public associatedArtifact: IArtifactReference = null,
            public propertyValues: IHashMapOfPropertyValues = {}) {
            super(id, name, projectId, typePrefix, parentId, baseItemTypePredefined, associatedArtifact, propertyValues);
        }
    }

    export class UserTaskShapeModel extends TaskShapeModel implements IUserTaskShape {
        constructor(
            public id: number = 0,
            public name: string = "",
            public projectId: number = 0,
            public typePrefix: string = "",
            public parentId: number = 0,
            public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.PROShape,
            public associatedArtifact: IArtifactReference = null,
            public propertyValues: IHashMapOfPropertyValues = {}) {
            super(id, name, projectId, typePrefix, parentId, baseItemTypePredefined, associatedArtifact, propertyValues);
        }
    }

    export class SystemTaskShapeModel extends TaskShapeModel implements ISystemTaskShape {
        constructor(
            public id: number = 0,
            public name: string = "",
            public projectId: number = 0,
            public typePrefix: string = "",
            public parentId: number = 0,
            public baseItemTypePredefined: ItemTypePredefined = ItemTypePredefined.PROShape,
            public associatedArtifact: IArtifactReference = null,
            public propertyValues: IHashMapOfPropertyValues = {}) {
            super(id, name, projectId, typePrefix, parentId, baseItemTypePredefined, associatedArtifact, propertyValues);
        }
    }

    export class ProcessLinkModel implements IProcessLinkModel {
        constructor(
            public parentId: number = 0,
            public sourceId: number = 0,
            public destinationId: number = 0,
            public orderindex: number = 0,
            public label: string = "",
            public sourceNode: any = null,
            public destinationNode: any = null) {
        }
    }
    
    export class Condition implements ICondition {
        constructor(
            public sourceId: number,
            public destinationId: number,
            public orderindex: number,
            public label: string,
            public mergeNode: IDiagramNode,
            public validMergeNodes: IDiagramNode[]) {
        }

        public static create(link: IProcessLink, mergeNode: IDiagramNode, validMergeNodes: IDiagramNode[]): ICondition {
            return new Condition(link.sourceId, link.destinationId, link.orderindex, link.label, mergeNode, validMergeNodes);

        }
    }

    export class ArtifactReferenceModel implements IArtifactReference {
        constructor(
            public id: number = 0,
            public projectId: number = 0,
            public name: string = "",
            public typePrefix: string = "",
            public baseItemTypePredefined: BluePrintSys.RC.CrossCutting.ItemTypePredefined = ItemTypePredefined.None,
            public link: string = "",
            public projectName: string = "") {
        }
    }

    export class ArtifactReferenceLinkModel implements IArtifactReferenceLink {
        constructor(
            public sourceId: number = 0,
            public destinationId: number = 0,
            public orderindex: number = 0,
            public associatedReferenceArtifactId: number = 0) {
        }
    }

    
}
