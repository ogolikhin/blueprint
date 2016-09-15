import { IMessageService } from "../../../core/";
import { IStatefulArtifactServices } from "../../models";
import { 
    ISession, 
    IArtifactAttachmentsService,
    IArtifactService,
    IArtifactHistoryService
 } from "../../models";
import { IMetaDataService } from "../";

export class StatefulArtifactServices implements IStatefulArtifactServices {
    constructor(private $q: ng.IQService,
                private _session: ISession,
                private _messageService: IMessageService,
                private _artifactService: IArtifactService,
                private _attachmentService: IArtifactAttachmentsService,
                private _historyService: IArtifactHistoryService,
                private _metaDataService: IMetaDataService) {
    }

    public getDeferred<T>(): ng.IDeferred<T> {
        return this.$q.defer<T>();
    }

    public get session(): ISession {
        return this._session;
    }

    public get messageService(): IMessageService {
        return this._messageService;
    }

    public get artifactService(): IArtifactService {
        return this._artifactService;
    }
    
    public get attachmentService(): IArtifactAttachmentsService {
        return this._attachmentService;
    }
    
    public get historyService(): IArtifactHistoryService {
        return this._historyService;
    }

    public get metaDataService(): IMetaDataService {
        return this._metaDataService;
    }
    
    // public request<T>(request: ng.IRequestConfig): ng.IPromise<T> {
    //     var defer = this.services.$q.defer<T>();
    //     this.$http(request).then(
    //         (result: ng.IHttpPromiseCallbackArg<T>) => defer.resolve(result.data),
    //         (errResult: ng.IHttpPromiseCallbackArg<any>) => {
    //             if (!errResult) {
    //                 defer.reject();
    //                 return;
    //             }
    //             var error = {
    //                 statusCode: errResult.status,
    //                 message: (errResult.data ? errResult.data.message : "")
    //             };
    //             defer.reject(error);
    //         }
    //     );
    //     return defer.promise;

    // }


}