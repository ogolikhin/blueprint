import {ProcessModels} from "../../";

export interface IProcessService {
    load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<ProcessModels.IProcess>;
    getProcesses(projectId: number): ng.IPromise<ProcessModels.IArtifactReference[]>;
}

export class ProcessService implements IProcessService {
    public static $inject = [
        "$http",
        "$q",
        "$rootScope"
    ];
    
    constructor(
        private $http: ng.IHttpService,
        private $q: ng.IQService,
        private $rootScope: ng.IRootScopeService) {
        
    }

    public load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<ProcessModels.IProcess> {
        let deferred = this.$q.defer<ProcessModels.IProcess>();
        let queryParamData = {
            versionId: isNaN(versionId) ? null : versionId,
            revisionId: isNaN(revisionId) ? null : revisionId,
            baselineId: isNaN(baselineId) ? null : baselineId,
            readOnly: !readOnly ? null : true //Do not send ?readOnly=false query parameter for normal calls
        };

        //Create parameters
        let requestConfig = {
            params: queryParamData
        };

        let restPath = this.processRestPath(processId);

        this.$http.get<ProcessModels.IProcess>(restPath, requestConfig).success((result: ProcessModels.IProcess) => {
            
            result["versionId"] = queryParamData.versionId;
            result["revisionId"] = queryParamData.revisionId;
            result["baselineId"] = queryParamData.baselineId;

            deferred.resolve(result);
            
        }).error((err: any/*Shell.IHttpError*/, status: number) => {

            err.statusCode = status;
            deferred.reject(err);
        });
        return deferred.promise;
    }

    /**
    * Returns all processes in specified project
    */
    public getProcesses(projectId: number): ng.IPromise<ProcessModels.IArtifactReference[]> {
        const restPath = `/svc/components/storyteller/projects/${projectId}/processes`;
        var deferred = this.$q.defer<ProcessModels.IArtifactReference[]>();

        this.$http.get<ProcessModels.IArtifactReference[]>(restPath).success((processes: ProcessModels.IArtifactReference[]) => {

            deferred.resolve(processes);

        }).error((err: any/*Shell.IHttpError*/, status: number) => {

            err.statusCode = status;
            deferred.reject(err);
        });

        return deferred.promise;
    }

    private processRestPath(processId: string): string {
        return "/svc/components/storyteller/processes/" + processId;
    }
}