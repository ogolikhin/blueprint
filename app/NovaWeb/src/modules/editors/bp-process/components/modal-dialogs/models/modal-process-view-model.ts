import {IProcessShape, 
        NodeType, 
        IUserTaskShape, 
        ISystemTaskShape, 
        IArtifactReference, 
        IProcess, 
        ProcessLinkModel} from "../../diagram/presentation/graph/models/";
import * as Enums from "../../../../../main/models/enums";
import {IProcessViewModel, ProcessViewModel} from "../../diagram/viewmodel/process-viewmodel";
import {ProcessShapeType, ProcessType} from "../../../models/enums";

export interface IModalProcessViewModel {
    isChanged: boolean;
    isUnpublished: boolean;
    //processModel: IProcess;
    processViewModel: IProcessViewModel;
    licenseType: Enums.LicenseTypeEnum;

    //load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<IProcess>;
    //save(): ng.IPromise<IProcess>;

    dispose();

    getNextNode(node: ISystemTaskShape): IProcessShape;
    setNextNode(node: ISystemTaskShape, value: IProcessShape);
    getNextNodes(node: IUserTaskShape): IProcessShape[];

    // Returns all processes in specified project

    // TODO look at this later
    //getProcesses(projectId: number): ng.IPromise<IArtifactReference[]>;

    isUserToSystemProcess(): boolean;
    updateProcessType(systemTasksVisible: boolean);
}

export class ModalProcessViewModel implements IModalProcessViewModel {
    
    constructor(public processViewModel: IProcessViewModel) {} 
    
    //public processModel: IProcess;
    
    public get isUnpublished(): boolean {
        return this.processViewModel.isUnpublished;
    }
    
    public get isChanged(): boolean {
        return this.processViewModel.isChanged;
    }

    public set isChanged(value: boolean) {
        this.processViewModel.isChanged = value;
    }
    
    public get licenseType(): Enums.LicenseTypeEnum {
        return this.processViewModel.licenseType;
    }

    //load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<IProcess>;
    
    //save(): ng.IPromise<IProcess>;

    public dispose() {

    };

    public getNextNode(node: ISystemTaskShape): IProcessShape {
        if (this.processViewModel) {
            for (let link of this.processViewModel.links) {
                if (link.sourceId === node.id) {
                    for (let shape of this.processViewModel.shapes) {
                        if (shape.id === link.destinationId) {
                            return shape;
                        }
                    }
                    return undefined;
                }
            }
        }
        return undefined;
    }

    public setNextNode(node: ISystemTaskShape, value: IProcessShape) {
        if (this.processViewModel && value) {
            let i: number;
            for (i = 0; i < this.processViewModel.links.length; i++) {
                if (this.processViewModel.links[i].sourceId === node.id) {
                    break;
                }
            }
            this.processViewModel.links[i] = new ProcessLinkModel(node.id, value.id);
        }
    }

    public getNextNodes(userTask: IUserTaskShape): IProcessShape[] {
        if (this.processViewModel) {
            return this.processViewModel.shapes.filter(shape => {
                if (shape.id !== userTask.id && shape.baseItemTypePredefined !== Enums.ItemTypePredefined.None) {
                    switch (shape.propertyValues["clientType"].value) {
                        case ProcessShapeType.UserTask:
                        case ProcessShapeType.UserDecision:
                        case ProcessShapeType.End:
                            return true;
                    }
                }
                return false;
            });
        }
        return [];
    }

    // Returns all processes in specified project
    
    // TODO look at this later
    //getProcesses(projectId: number): ng.IPromise<IArtifactReference[]>;

    public isUserToSystemProcess(): boolean {
        return this.processViewModel != null && this.processViewModel.propertyValues["clientType"].value === ProcessType.UserToSystemProcess;
    }

    public updateProcessType(systemTaskVisibilityEnabled: boolean) {
        if (systemTaskVisibilityEnabled) {
            this.processViewModel.propertyValues["clientType"].value = ProcessType.UserToSystemProcess;
        }
        else {
            this.processViewModel.propertyValues["clientType"].value = ProcessType.BusinessProcess;
        }

        this.isChanged = true;
    }
}
