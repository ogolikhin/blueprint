module Shell {
    export interface IArtifactVersionControlService {
        publish(model: IProcess, isChanged: boolean): ng.IPromise<boolean>;
        publishProcess(model: IProcess): ng.IPromise<boolean>;
        discardArtifactChanges(artifacts: IArtifactVersionControlServiceRequest[]): ng.IPromise<DiscardResultsInfo>;
        lock(artifacts: IProcess[]): ng.IPromise<ILockResultInfo[]>;
        publishArtifacts(artifacts: IArtifactVersionControlServiceRequest[]): ng.IPromise<boolean>;
    }

    export interface IArtifactVersionControlServiceRequest {
        artifactId: number;
        status: IItemStatus;
    }
}