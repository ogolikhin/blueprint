import {IProcessService, ProcessModels} from "./process.svc";

export class ProcessServiceMock implements IProcessService {
    private processModel;
    constructor(private $q: ng.IQService) {
        this.processModel = <any>{ projectId: 1, id: 1111, status: { isLocked: true, isLockedByMe: true }, shapes:[], links: []};
    }

    public load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<ProcessModels.IProcess> {
        return this.$q.when(this.processModel);
    }

    public getProcesses(projectId: number): ng.IPromise<ProcessModels.IArtifactReference[]> {
        return this.$q.when([
            ProcessServiceMock.createArtifactReference({ id: 1111, name: "Initial Process"}),
            ProcessServiceMock.createArtifactReference({ id: 2222, name: "First Process"}),
            ProcessServiceMock.createArtifactReference({ id: 3333, name: "Second Process"}),
            ProcessServiceMock.createArtifactReference({ id: 4444, name: "Third Process"}),
            ProcessServiceMock.createArtifactReference({ id: 5555, name: "Fourth Process"}),
            ProcessServiceMock.createArtifactReference({ id: 6666, name: "Fifth Process"}),
            ProcessServiceMock.createArtifactReference({ id: 7777, name: "Yet another Process"}),
            ProcessServiceMock.createArtifactReference({ id: 8888, name: "Yet another Process"}),
            ProcessServiceMock.createArtifactReference({ id: 9999, name: "Yet another Process"}),
            ProcessServiceMock.createArtifactReference({ id: 9998, name: "Yet another Process"}),
            ProcessServiceMock.createArtifactReference({ id: 9997, name: "Yet another Process"}),
            ProcessServiceMock.createArtifactReference({ id: 9996, name: "Yet another Process"})
        ]);
    }

    private static createArtifactReference(source: any): ProcessModels.IArtifactReference {
        let reference: ProcessModels.IArtifactReference = {
            id: source.id || -1,
            projectId: source.projectId || 1,
            projectName: source.ProjectName || "Prj",
            typePrefix: source.TypePrefix || "PRO",
            name: source.name || "NoName",
            baseItemTypePredefined: source.baseItemTypePredefined || 4114,
            link: source.link || ""
        };
        return reference;
    }
}