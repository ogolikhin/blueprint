import {IArtifactState} from "../state";
import {Models, Enums, Relationships} from "../../../main/models";
import {ArtifactAttachments, IArtifactAttachments, IArtifactAttachmentsResultSet} from "../attachments";
import {ArtifactProperties, SpecialProperties, IArtifactProperties} from "../properties";
import {ChangeSetCollector, ChangeTypeEnum, IChangeCollector, IChangeSet, IItemChangeSet} from "../changeset";
import {IMetaData} from "../metadata";
import {IDocumentRefs, DocumentRefs} from "../docrefs";
import {IStatefulArtifactServices} from "../services";
import {IArtifactRelationships, ArtifactRelationships} from "../relationships";
import {IApplicationError} from "../../../core/error/applicationError";
import {HttpStatusCode} from "../../../core/http/http-status-code";
import {IPropertyDescriptor} from "../../../editors/configuration/property-descriptor-builder";

export interface IStatefulItem extends Models.IArtifact {
    artifactState: IArtifactState;
    metadata: IMetaData;
    customProperties: IArtifactProperties;
    specialProperties: IArtifactProperties;
    attachments: IArtifactAttachments;
    relationships: IArtifactRelationships;
    docRefs: IDocumentRefs;

    lock();
    discard();
    changes(): Models.ISubArtifact;
    errorObservable(): Rx.Observable<IApplicationError>;
    unsubscribe(): void;
    getEffectiveVersion(): number;
    getProperyObservable(): Rx.Observable<IItemChangeSet>;
    validateItem(propertyDescriptors: IPropertyDescriptor[]): boolean;
}

export interface IIStatefulItem extends IStatefulItem {
    propertyChange: Rx.BehaviorSubject<IItemChangeSet>;

    getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet>;
    getRelationships(): ng.IPromise<Relationships.IArtifactRelationshipsResultSet>;
    getServices(): IStatefulArtifactServices;

}

export abstract class StatefulItem implements IIStatefulItem {
    public metadata: IMetaData;

    protected _attachments: IArtifactAttachments;
    protected _docRefs: IDocumentRefs;
    protected _relationships: IArtifactRelationships;
    protected _customProperties: IArtifactProperties;
    protected _specialProperties: IArtifactProperties;
    protected _changesets: IChangeCollector;
    protected lockPromise: ng.IPromise<IStatefulItem>;
    protected loadPromise: ng.IPromise<IStatefulItem>;
    protected attachmentsAndDocRefsPromise: ng.IPromise<IArtifactAttachmentsResultSet>;
    private _error: Rx.BehaviorSubject<IApplicationError>;
    private _propertyChangeSubject: Rx.BehaviorSubject<IItemChangeSet>;

    constructor(protected artifact: Models.IArtifact, protected services: IStatefulArtifactServices) {
    }

    public dispose() {
        this.artifact.parentId = null;
        this.unsubscribe();
    }

    public unsubscribe() {
        if (this.error) {
            this.error.onCompleted();
        }
        delete this._error;
    }

    protected get error(): Rx.BehaviorSubject<IApplicationError> {
        if (!this._error) {
            this._error = new Rx.BehaviorSubject<IApplicationError>(null);
        }
        return this._error;
    }

    public errorObservable(): Rx.Observable<IApplicationError> {
        return this.error.filter(it => !!it).distinctUntilChanged().asObservable();
    }

    public get propertyChange(): Rx.BehaviorSubject<IItemChangeSet> {
        if (!this._propertyChangeSubject) {
            this._propertyChangeSubject = new Rx.BehaviorSubject<IItemChangeSet>({item: this});
        }
        return this._propertyChangeSubject;
    }

    public getProperyObservable(): Rx.Observable<IItemChangeSet> {
        return this.propertyChange.filter(it => !!it).asObservable();
    }


    public get id(): number {
        return this.artifact.id;
    }

    public set id(value: number) {
        this.artifact.id = value;
    }

    public get projectId() {
        return this.artifact.projectId;
    }

    public set projectId(value: number) {
        this.set("projectId", value);
    }

    public get name(): string {
        return this.artifact.name;
    }

    public set name(value: string) {
        this.set("name", value);
    }

    public get description(): string {
        return this.artifact.description;
    }

    public set description(value: string) {
        this.set("description", value);
    }

    public get itemTypeId(): number {
        return this.artifact.itemTypeId;
    }

    public set itemTypeId(value: number) {
        this.set("itemTypeId", value);
    }

    public get itemTypeVersionId(): number {
        return this.artifact.itemTypeVersionId;
    }

    public get itemTypeIconId(): number {
        return this.artifact.itemTypeIconId;
    }

    public get itemTypeName(): string {
        return this.artifact.itemTypeName;
    }

    public get predefinedType(): Models.ItemTypePredefined {
        return this.artifact.predefinedType;
    }

    public get permissions(): Enums.RolePermissions {
        return this.artifact.permissions;
    }

    public get version() {
        return this.artifact.version;
    }

    public get prefix(): string {
        return this.artifact.prefix;
    }

    public get parentId(): number {
        return this.artifact.parentId;
    }

    public get orderIndex(): number {
        return this.artifact.orderIndex;
    }

    public get createdOn(): Date {
        return this.artifact.createdOn;
    }

    public get lastEditedOn(): Date {
        return this.artifact.lastEditedOn;
    }

    public get createdBy(): Models.IUserGroup {
        return this.artifact.createdBy;
    }

    public get lastEditedBy(): Models.IUserGroup {
        return this.artifact.lastEditedBy;
    }

    public get hasChildren(): boolean {
        return this.artifact.hasChildren;
    }

    public get readOnlyReuseSettings(): Enums.ReuseSettings {
        return this.artifact.readOnlyReuseSettings;
    }

    public getServices(): IStatefulArtifactServices {
        return this.services;
    }

    public getEffectiveVersion(): number {
        return this.artifactState.historical ? this.version : undefined;
    }

    protected isHeadVersionDeleted() {
        return this.artifactState.deleted && !this.artifactState.historical;
    }

    public set(name: string, value: any) {
        if (name in this) {
            const changeset = {
                type: ChangeTypeEnum.Update,
                key: name,
                value: this.artifact[name] = value
            } as IChangeSet;
            this.changesets.add(changeset);
            this.propertyChange.onNext({item: this, change: changeset} as IItemChangeSet);
            this.lock();
        }
    }

    public get customProperties() {
        if (!this._customProperties) {
            this._customProperties = new ArtifactProperties(this);
        }
        return this._customProperties;
    }

    public get changesets() {
        if (!this._changesets) {
            this._changesets = new ChangeSetCollector(this);
        }
        return this._changesets;
    }

    public get specialProperties() {
        if (!this._specialProperties) {
            this._specialProperties = new SpecialProperties(this);
        }
        return this._specialProperties;
    }

    public get attachments() {
        if (!this._attachments) {
            this._attachments = new ArtifactAttachments(this);
        }
        return this._attachments;
    }

    public get docRefs() {
        if (!this._docRefs) {
            this._docRefs = new DocumentRefs(this);
        }
        return this._docRefs;
    }

    public get relationships() {
        if (!this._relationships) {
            this._relationships = new ArtifactRelationships(this);
        }
        return this._relationships;
    }

    public abstract lock();

    protected isFullArtifactLoadedOrLoading(): boolean {
        return (this._customProperties && this._customProperties.isLoaded &&
            this._specialProperties && this._specialProperties.isLoaded) || !!this.loadPromise;
    }

    public unload() {
        if (this._customProperties) {
            this._customProperties.dispose();
            delete this._customProperties;
        }
        if (this._specialProperties) {
            this._specialProperties.dispose();
            delete this._specialProperties;
        }
        if (this._attachments) {
            this._attachments.dispose();
            delete this._attachments;
        }
        if (this._docRefs) {
            this._docRefs.dispose();
            delete this._docRefs;
        }
        //TODO: REMOVE WHEN AUTO-SAVE GETS COMPLETED. AUTO-SAVE SHOULD ALREADY HAVE THIS FLAG SET TO FALSE.
        if (this.artifactState) {
            this.artifactState.dirty = false;
        }
        //TODO: implement the same for all objects
    }

    public discard() {
        this.changesets.reset();
        if (this._customProperties) {
            this._customProperties.discard();
        }
        if (this._specialProperties) {
            this._specialProperties.discard();
        }
        if (this._attachments) {
            this._attachments.discard();
        }
        if (this._docRefs) {
            this._docRefs.discard();
        }
        if (this._relationships) {
            this._relationships.discard();
        }
    }

    protected initialize(artifact: Models.IArtifact): void {
        this.artifact = artifact;
        this.customProperties.initialize(artifact.customPropertyValues);
        this.specialProperties.initialize(artifact.specificPropertyValues);
    }

    public getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet> {
        if (this.attachmentsAndDocRefsPromise) {
            return this.attachmentsAndDocRefsPromise;
        }
        const deferred = this.services.getDeferred();
        this.attachmentsAndDocRefsPromise = deferred.promise;
        this.getAttachmentsDocRefsInternal().then((result: IArtifactAttachmentsResultSet) => {
            this.attachments.initialize(result.attachments);
            this.docRefs.initialize(result.documentReferences);
            deferred.resolve(result);
        }).catch(error => {
            if (error && error.statusCode === HttpStatusCode.NotFound) {
                this.artifactState.deleted = true;
            }
            deferred.reject(error);
        }).finally(() => {
            this.attachmentsAndDocRefsPromise = undefined;
        });
        return this.attachmentsAndDocRefsPromise;
    }

    protected abstract getAttachmentsDocRefsInternal(): ng.IPromise<IArtifactAttachmentsResultSet>;

    public getRelationships(): ng.IPromise<Relationships.IArtifactRelationshipsResultSet> {
        const deferred = this.services.getDeferred();
        this.getRelationshipsInternal().then((result: Relationships.IArtifactRelationshipsResultSet) => {
            deferred.resolve(result);
        }, (error) => {
            if (error && error.statusCode === HttpStatusCode.NotFound) {
                this.artifactState.deleted = true;
            }
            deferred.reject(error);
        });

        return deferred.promise;
    }

    protected abstract getRelationshipsInternal(): ng.IPromise<Relationships.IArtifactRelationshipsResultSet> ;

    public changes(): Models.IArtifact {
        const delta = {} as Models.IArtifact;

        delta.id = this.id;
        delta.projectId = this.projectId;
        delta.customPropertyValues = [];
        this.changesets.get().forEach((it: IChangeSet) => {
            delta[it.key as string] = it.value;
        });

        delta.customPropertyValues = this.customProperties.changes();
        delta.specificPropertyValues = this.specialProperties.changes();
        delta.attachmentValues = this.attachments.changes();
        delta.docRefValues = this.docRefs.changes();

        return delta;
    }


    //TODO: moved from bp-artifactinfo
    public abstract get artifactState(): IArtifactState;


    public validateItem(propertyDescriptors: IPropertyDescriptor[]): boolean {

        let result = _.every(propertyDescriptors, (propertyType: IPropertyDescriptor) => {
            let value: any;
            let propertyValue: Models.IPropertyValue;
            switch (propertyType.lookup) {
                case Enums.PropertyLookupEnum.Custom:

                    // do not validate unloaded custom properties
                    if (!this.customProperties.isLoaded) {
                        return true;
                    }

                    propertyValue = this.customProperties.get(propertyType.modelPropertyName as number);
                    if (propertyValue) {
                        value = propertyValue.value;
                    }
                    break;
                case Enums.PropertyLookupEnum.Special:
                    propertyValue = this.specialProperties.get(propertyType.modelPropertyName as number);
                    if (propertyValue) {
                        value = propertyValue.value;
                    }
                    break;
                default:
                    value = this[propertyType.modelPropertyName];
                    break;
            }

            let isValid: boolean = !_.isBoolean(propertyType.isValidated);
            if (!isValid) {
                isValid = this.validateProperty(propertyType, value);
            }

            return isValid;
        });
        return result;
    }

    private validateProperty(propertyType: IPropertyDescriptor, propValue: any): boolean {
        let value = null;
        let isValid = true;

        try {
            switch (propertyType.primitiveType) {
                case Models.PrimitiveType.Number:
                    if (!this.services.validationService.numberValidation.isValid(propValue,
                            propValue,
                            propertyType.decimalPlaces,
                            propertyType.minNumber,
                            propertyType.maxNumber,
                            propertyType.isValidated,
                            propertyType.isRequired)) {
                        isValid = false;
                    }
                    break;
                case Models.PrimitiveType.Date:
                    if (!this.services.validationService.dateValidation.isValid(propValue,
                            propValue,
                            propertyType.minDate,
                            propertyType.maxDate,
                            propertyType.isValidated,
                            propertyType.isRequired)) {
                        isValid = false;
                    }
                    break;
                case Models.PrimitiveType.Text:
                    if (propertyType.isRichText) {
                        if (!this.services.validationService.textRtfValidation.hasValueIfRequired(propertyType.isRequired,
                                propValue,
                                propValue, propertyType.isValidated)) {
                            isValid = false;
                        }
                    } else {
                        if (!this.services.validationService.textValidation.hasValueIfRequired(propertyType.isRequired,
                                propValue,
                                propValue, propertyType.isValidated)) {
                            isValid = false;
                        }
                    }
                    break;
                case Models.PrimitiveType.Choice:
                    value = propValue ? propValue.validValues : null;
                    if (propertyType.isMultipleAllowed) {
                        if (!this.services.validationService.multiSelectValidation.hasValueIfRequired(propertyType.isRequired,
                                value, value, propertyType.isValidated)) {
                            isValid = false;
                        }
                    } else {
                        if (!this.services.validationService.selectValidation.hasValueIfRequired(propertyType.isRequired,
                                value, value, propertyType.isValidated)) {
                            isValid = false;
                        }
                    }
                    break;
                case Models.PrimitiveType.User:
                    if (!!propValue) {
                        if (!!propValue.usersGroups) {
                            value = propValue.usersGroups;
                        } else {
                            if (!!propValue.label) {
                                value = propValue.label.split(",");
                            }
                        }
                    }
                    if (!this.services.validationService.userPickerValidation.hasValueIfRequired(propertyType.isRequired,
                            value, value, propertyType.isValidated)) {
                        isValid = false;
                    }
                    break;
                default:
                    this.services.$log.error(`ERROR: PrimitiveType ${propertyType.primitiveType} is not defined`);
                    isValid = false;
            }
        } catch (err) {
            // log error
            this.services.$log.error(err);
            isValid = false;
        }
        if (!isValid) {
            this.services.$log.log("----------------------validateProperty------------------------");
            this.services.$log.log(propertyType);
            this.services.$log.log(`value = ${value}`);
        }
        return isValid;
    }

}
