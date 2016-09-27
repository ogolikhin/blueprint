import {IProcessService, ProcessModels} from "./process.svc";

export class ProcessServiceMock implements IProcessService {
    private processModel;
    constructor(private $q: ng.IQService) {
        this.processModel = <any>{ projectId: 1, id: 1111, status: { isLocked: true, isLockedByMe: true } };
    }

    public load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<ProcessModels.IProcess> {
        return this.$q.when(this.processModel);
    }

    public getProcesses(projectId: number): ng.IPromise<ProcessModels.IArtifactReference[]> {
        return this.$q.when([
            <ProcessModels.IArtifactReference>{ id: 1111, projectId: 1, typePrefix: "PRO", name: "Initial Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 2222, projectId: 1, typePrefix: "PRO", name: "First Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 3333, projectId: 1, typePrefix: "PRO", name: "Second Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 4444, projectId: 1, typePrefix: "PRO", name: "Third Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 5555, projectId: 1, typePrefix: "PRO", name: "Fourth Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 6666, projectId: 1, typePrefix: "PRO", name: "Fifth Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 7777, projectId: 1, typePrefix: "PRO", name: "Yet another Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 8888, projectId: 1, typePrefix: "PRO", name: "Yet another Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 9999, projectId: 1, typePrefix: "PRO", name: "Yet another Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 9998, projectId: 1, typePrefix: "PRO", name: "Yet another Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 9997, projectId: 1, typePrefix: "PRO", name: "Yet another Process", baseItemTypePredefined: 4114 },
            <ProcessModels.IArtifactReference>{ id: 9996, projectId: 1, typePrefix: "PRO", name: "Yet another Process", baseItemTypePredefined: 4114 }
        ]);
    }
}