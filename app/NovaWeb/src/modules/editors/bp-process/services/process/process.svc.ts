﻿import * as ProcessModels from "../../models/processModels";
export {ProcessModels}

export interface IProcessService {
    load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<ProcessModels.IProcess>;
    getProcesses(projectId: number): ng.IPromise<ProcessModels.IArtifactReference[]>;
}

export class ProcessService implements IProcessService {
    public static $inject = [
        "$http",
        "$q",
        "messageService",
        "$rootScope"
    ];
    
    constructor(
        private $http: ng.IHttpService,
        private $q: ng.IQService,
        private messageService: IMessageService,
        private $rootScope: ng.IRootScopeService) {
        
    }

    public load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<ProcessModels.IProcess> {
        let deferred = this.$q.defer<ProcessModels.IProcess>();
        this.messageService.dispose();
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

        this.$http.get<ProcessModels.IProcess>(restPath, requestConfig).then(
            (result: ng.IHttpPromiseCallbackArg<ProcessModels.IProcess>) => {
            
                result["versionId"] = queryParamData.versionId;
                result["revisionId"] = queryParamData.revisionId;
                result["baselineId"] = queryParamData.baselineId;

                deferred.resolve(result.data);
            
            }, (result: ng.IHttpPromiseCallbackArg<any>) => {

                result.data.statusCode = result.status;
                deferred.reject(result.data);
            }
        );
        return deferred.promise;
    }

   
    public getProcesses(projectId: number): ng.IPromise<ProcessModels.IArtifactReference[]> {
        const restPath = `/svc/components/storyteller/projects/${projectId}/processes`;
        var deferred = this.$q.defer<ProcessModels.IArtifactReference[]>();

        this.$http.get<ProcessModels.IArtifactReference[]>(restPath).then(
            (result: ng.IHttpPromiseCallbackArg<ProcessModels.IArtifactReference[]>) => {

                deferred.resolve(result.data);

            }, (result: ng.IHttpPromiseCallbackArg<any/*Shell.IHttpError*/>) => {

                result.data.statusCode = result.status;
                deferred.reject(result.data);
            }
        );

        return deferred.promise;
    }

    private processRestPath(processId: string): string {
        return "/svc/components/storyteller/processes/" + processId;
    }
}