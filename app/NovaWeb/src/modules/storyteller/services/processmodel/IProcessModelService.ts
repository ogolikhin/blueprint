module Storyteller {
    export interface IProcessModelService {
        isChanged: boolean;
        isUnpublished: boolean;
        processModel: IProcess;
        licenseType: Shell.LicenseTypeEnum;
        load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<IProcess>;
        save(): ng.IPromise<IProcess>;

        dispose();

        getNextNode(node: ISystemTaskShape): IProcessShape;
        setNextNode(node: ISystemTaskShape, value: IProcessShape);
        getNextNodes(node: IUserTaskShape): IProcessShape[];
        // Returns all processes in specified project
        getProcesses(projectId: number): ng.IPromise<IArtifactReference[]>;
        isUserToSystemProcess(): boolean;
        updateProcessType(systemTasksVisible: boolean);
    }
}
