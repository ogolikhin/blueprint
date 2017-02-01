import {IArtifact} from "../../../../main/models/models";

export interface IUseCase extends IArtifact {
    preCondition: IStep;
    steps: IStep[];
    postCondition: IStep;
}

export interface IStep extends IUseCaseElement {
    description: string;
    stepOf: StepOfType;
    flows: IFlow[];
    //client side only properties
    condition: boolean;
    external: boolean;
}

export interface IUseCaseElement {
    id: number;
    name: string;
    orderIndex: number;
}

export interface IFlow extends IUseCaseElement {
    isExternal: boolean;
    steps: IStep[];
    returnToStepName: string;
}

export const enum StepOfType {
    System = 0,
    Actor = 1
}
