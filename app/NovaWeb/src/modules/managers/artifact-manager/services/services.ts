import {IDialogService} from "../../../shared/";
import {ISession} from "../../../shell/login/session.svc";
import {IArtifactService} from "../artifact";
import {IMetaDataService} from "../metadata";
import {IArtifactAttachmentsService} from "../attachments";
import {IArtifactRelationshipsService} from "../relationships";
import {IPublishService} from "../../../managers/artifact-manager/publish.svc";
import {ILoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../core/messages/message.svc";
import {ILocalizationService} from "../../../core/localization/localizationService";

export interface IStatefulArtifactServices {
    //request<T>(config: ng.IRequestConfig): ng.IPromise<T>;
    getDeferred<T>(): ng.IDeferred<T>;
    $q: ng.IQService;
    messageService: IMessageService;
    dialogService: IDialogService;
    localizationService: ILocalizationService;
    session: ISession;
    artifactService: IArtifactService;
    attachmentService: IArtifactAttachmentsService;
    relationshipsService: IArtifactRelationshipsService;
    metaDataService: IMetaDataService;
    loadingOverlayService: ILoadingOverlayService;
    publishService: IPublishService;
}

export class StatefulArtifactServices implements IStatefulArtifactServices {
    constructor(public $q: ng.IQService,
                private _session: ISession,
                private _messageService: IMessageService,
                private _dialogService: IDialogService,
                private _localizationService: ILocalizationService,
                private _artifactService: IArtifactService,
                private _attachmentService: IArtifactAttachmentsService,
                private _relationshipsService: IArtifactRelationshipsService,
                private _metadataService: IMetaDataService,
                private _loadingOverlayService: ILoadingOverlayService,
                private _publishService: IPublishService) {
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

    public get dialogService(): IDialogService {
        return this._dialogService;
    }

    public get localizationService(): ILocalizationService {
        return this._localizationService;
    }

    public get artifactService(): IArtifactService {
        return this._artifactService;
    }

    public get attachmentService(): IArtifactAttachmentsService {
        return this._attachmentService;
    }

    public get relationshipsService(): IArtifactRelationshipsService {
        return this._relationshipsService;
    }

    public get metaDataService(): IMetaDataService {
        return this._metadataService;
    }

    public get loadingOverlayService(): ILoadingOverlayService {
        return this._loadingOverlayService;
    }

    public get publishService(): IPublishService {
        return this._publishService;
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
