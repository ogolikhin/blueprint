import {IDialogService} from "../../../shared/";
import {ISession} from "../../../shell/login/session.svc";
import {IArtifactService} from "../artifact";
import {IMetaDataService} from "../metadata";
import {IArtifactAttachmentsService} from "../attachments";
import {IArtifactRelationshipsService} from "../relationships";
import {IValidationService} from "../validation/validation.svc";
import {ILoadingOverlayService} from "../../../core/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../core/localization/localization.service";
import {IPropertyDescriptorBuilder} from "../../../editors/configuration/property-descriptor-builder";
import {IUnpublishedArtifactsService} from "../../../editors/unpublished/unpublished.svc";
import {IMessageService} from "../../../main/components/messages/message.svc";

export interface IStatefulArtifactServices {
    getDeferred<T>(): ng.IDeferred<T>;
    $q: ng.IQService;
    $log: ng.ILogService;
    messageService: IMessageService;
    dialogService: IDialogService;
    localizationService: ILocalizationService;
    session: ISession;
    artifactService: IArtifactService;
    attachmentService: IArtifactAttachmentsService;
    relationshipsService: IArtifactRelationshipsService;
    metaDataService: IMetaDataService;
    loadingOverlayService: ILoadingOverlayService;
    publishService: IUnpublishedArtifactsService;
    validationService: IValidationService;
    propertyDescriptor: IPropertyDescriptorBuilder;
}

export class StatefulArtifactServices implements IStatefulArtifactServices {
    constructor(public $q: ng.IQService,
                public $log: ng.ILogService,
                private _session: ISession,
                private _messageService: IMessageService,
                private _dialogService: IDialogService,
                private _localizationService: ILocalizationService,
                private _artifactService: IArtifactService,
                private _attachmentService: IArtifactAttachmentsService,
                private _relationshipsService: IArtifactRelationshipsService,
                private _metadataService: IMetaDataService,
                private _loadingOverlayService: ILoadingOverlayService,
                private _publishService: IUnpublishedArtifactsService,
                private _validationService: IValidationService,
                private _propertyDescriptor: IPropertyDescriptorBuilder) {
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

    public get publishService(): IUnpublishedArtifactsService {
        return this._publishService;
    }

    public get validationService(): IValidationService {
        return this._validationService;
    }

    public get propertyDescriptor(): IPropertyDescriptorBuilder {
        return this._propertyDescriptor;
    }


}
