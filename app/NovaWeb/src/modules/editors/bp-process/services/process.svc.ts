import {Models} from "../../../main/models";
import * as ProcessModels from "../models/process-models";
import { IProcessModelProcessor } from "./process-model-processor";
import { ProcessModelProcessor } from "./process-model-processor";
import { IStatefulProcessArtifact } from "../process-artifact";

export { ProcessModels }

export interface IProcessService {
    load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<ProcessModels.IProcess>;
    save(processVM: ProcessModels.IProcess): ng.IPromise<IProcessUpdateResult>;
}

export interface IProcessUpdateResult {
    messages: IOperationMessageResult[];
    result: ProcessModels.IProcess;
    tempIdMap: Models.IKeyValuePair[];
}

interface IHttpError {
    message: string;
    statusCode: number; // client side only
    errorCode: number;
}

const enum MessageLevel {
    None = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}

interface IOperationMessageResult {
    level: MessageLevel;
    propertyTypeId: number;
    itemId: number;
    code: number;
    message: string;
}

export class ProcessService implements IProcessService {

    public static $inject = ["$http", "$q"];

    private processModelProcessor: IProcessModelProcessor;

    constructor(private $http: ng.IHttpService,
        private $q: ng.IQService) {
        this.processModelProcessor = new ProcessModelProcessor();

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

        this.$http.get<ProcessModels.IProcess>(restPath, requestConfig).then(
            (result: ng.IHttpPromiseCallbackArg<ProcessModels.IProcess>) => {

                result["versionId"] = queryParamData.versionId;
                result["revisionId"] = queryParamData.revisionId;
                result["baselineId"] = queryParamData.baselineId;

                deferred.resolve(result.data);

            }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                if (!result) {
                    deferred.reject();
                    return;
                }                
                deferred.reject(result.data);
            }
        );
        return deferred.promise;
    }

    private processRestPath(processId: string | number): string {
        return "/svc/components/storyteller/processes/" + processId;
    }

    public save(process: ProcessModels.IProcess): ng.IPromise<IProcessUpdateResult> {
        const restPath = this.processRestPath(process.id);
        const deferred = this.$q.defer<IProcessUpdateResult>();

        const procModel: ProcessModels.IProcess = this.processModelProcessor.processModelBeforeSave(process);

        this.$http.patch<IProcessUpdateResult>(restPath, procModel).then((result) => {
            // success
            deferred.resolve(result.data);

        }).catch((err) => {
            deferred.reject(err.data);
        });

        return deferred.promise;
    }

}