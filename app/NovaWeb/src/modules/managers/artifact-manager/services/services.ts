import { IMessageService } from "../../../core/";
import { IStatefulArtifactServices } from "../../models";
import { 
    IArtifactManager, 
    IStatefulArtifact, 
    ISession, 
    IArtifactAttachmentsService,
    IArtifactService,
 } from "../../models";


export class StatefulArtifactServices implements IStatefulArtifactServices {


    constructor(private $q: ng.IQService,
                private _messageService: IMessageService,
                private _artifactService: IArtifactService,
                private _attachmentService: IArtifactAttachmentsService) {

    }

    public getDeferred<T>(): ng.IDeferred<T> {
        return this.$q.defer<T>();
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