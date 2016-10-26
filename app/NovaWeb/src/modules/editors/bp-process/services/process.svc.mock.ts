import { IProcessService, ProcessModels } from "./process.svc";
import { IProcessUpdateResult } from "./process.svc";

export class ProcessServiceMock implements IProcessService {
    private processModel;

    constructor(private $q: ng.IQService) {
        this.processModel = <any>{
            projectId: 1,
            id: 1111,
            status: {isLocked: true, isLockedByMe: true},
            shapes: [],
            links: []
        };
    }

    public load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<ProcessModels.IProcess> {
        return this.$q.when(this.processModel);
    }

    public save(processVM: ProcessModels.IProcess): ng.IPromise<IProcessUpdateResult> {
        throw new Error("not implemented");
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
