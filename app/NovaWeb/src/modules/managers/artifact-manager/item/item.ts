import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";
import {IPropertyDescriptor} from "../../../editorsModule/services";
import {Enums, Models, Relationships} from "../../../main/models";
import {ItemTypePredefined} from "../../../main/models/itemTypePredefined.enum";
import {IApplicationError} from "../../../shell/error/applicationError";
import {ArtifactAttachments, IArtifactAttachments, IArtifactAttachmentsResultSet} from "../attachments";
import {ChangeSetCollector, ChangeTypeEnum, IChangeCollector, IChangeSet, IItemChangeSet} from "../changeset";
import {DocumentRefs, IDocumentRefs} from "../docrefs";
import {IMetaData} from "../metadata";
import {ArtifactProperties, IArtifactProperties, SpecialProperties} from "../properties";
import {ArtifactRelationships, IArtifactRelationships} from "../relationships";
import {IStatefulArtifactServices} from "../services";
import {IArtifactState} from "../state";

export interface IStatefulItem extends Models.IArtifact {
    artifactState: IArtifactState;
    metadata: IMetaData;
    customProperties: IArtifactProperties;
    specialProperties: IArtifactProperties;
    attachments: IArtifactAttachments;
    relationships: IArtifactRelationships;
    docRefs: IDocumentRefs;

    supportRelationships(): boolean;
    lock();
    discard();
    changes(): Models.ISubArtifact;
    errorObservable(): Rx.Observable<IApplicationError>;
    unsubscribe(): void;
    getEffectiveVersion(): number;
    getPropertyObservable(): Rx.Observable<IItemChangeSet>;
    validateItem(propertyDescriptors: IPropertyDescriptor[]): boolean;
    isReuseSettingSRO(reuseSetting: Enums.ReuseSettings): boolean;
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

    public getPropertyObservable(): Rx.Observable<IItemChangeSet> {
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
        this.set("name", value || "");
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

    public get predefinedType(): ItemTypePredefined {
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

    public isReuseSettingSRO(reuseSetting: Enums.ReuseSettings): boolean {
        return (this.artifact.readOnlyReuseSettings & reuseSetting) === reuseSetting;
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
        if (this.supportRelationships() && !this._relationships) {
            this._relationships = new ArtifactRelationships(this);
        }
        return this._relationships;
    }

    public supportRelationships(): boolean {
        return true;
    }

    public abstract lock();

    protected isFullArtifactLoadedOrLoading(): boolean {
        return (this._customProperties && this._customProperties.isLoaded &&
            this._specialProperties && this._specialProperties.isLoaded) || !!this.loadPromise;
    }

    public unload() {
        if (this._customProperties) {
            this._customProperties.dispose();
            this._customProperties = undefined;
        }

        if (this._specialProperties) {
            this._specialProperties.dispose();
            this._specialProperties = undefined;
        }

        if (this._attachments) {
            this._attachments.dispose();
            this._attachments = undefined;
        }

        if (this._docRefs) {
            this._docRefs.dispose();
            this._docRefs = undefined;
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
        if (this.artifact.createdOn) {
            this.artifact.createdOn = this.services.localizationService.current.toDate(this.artifact.createdOn);
        }
        if (this.artifact.lastEditedOn) {
            this.artifact.lastEditedOn = this.services.localizationService.current.toDate(this.artifact.lastEditedOn);
        }
        if (this.artifact.lastSavedOn) {
            this.artifact.lastSavedOn = this.services.localizationService.current.toDate(this.artifact.lastSavedOn);
        }
        if (this.artifact.lockedDateTime) {
            this.artifact.lockedDateTime = this.services.localizationService.current.toDate(this.artifact.lockedDateTime);
        }

        this.customProperties.initialize(artifact.customPropertyValues);
        this.specialProperties.initialize(artifact.specificPropertyValues);
    }

    public getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet> {
        if (this.attachmentsAndDocRefsPromise) {
            return this.attachmentsAndDocRefsPromise;
        }
        const deferred = this.services.getDeferred();
        this.attachmentsAndDocRefsPromise = deferred.promise;
        if (this.hasArtifactEverBeenSavedOrPublished()) {
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
        } else {
            const resultSet = {
                artifactId: this.artifact.id,
                attachments: [],
                documentReferences: []
            };
            this.attachments.initialize(resultSet.attachments);
            this.docRefs.initialize(resultSet.documentReferences);
            deferred.resolve(resultSet);
        }
        return this.attachmentsAndDocRefsPromise;
    }

    protected hasArtifactEverBeenSavedOrPublished() {
        return this.artifact.id > 0;
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

                    if (!_.isBoolean(propertyType.isValidated)) {
                        propertyType.isValidated = true;
                    }

                    break;
                case Enums.PropertyLookupEnum.Special:
                    propertyValue = this.specialProperties.get(propertyType.modelPropertyName as number);
                    if (propertyValue) {
                        value = propertyValue.value;
                    }
                    break;
                default:
                    // Always validate the system property - name
                    if (propertyType.propertyTypePredefined === Models.PropertyTypePredefined.Name) {
                        propertyType.isValidated = true;
                    }
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
                            propertyType.minDate,
                            propertyType.maxDate,
                            propertyType.isValidated,
                            propertyType.isRequired)) {
                        isValid = false;
                    }
                    break;
                case Models.PrimitiveType.Text:
                    if (propertyType.propertyTypePredefined === Models.PropertyTypePredefined.Name) {
                        if (!this.services.validationService.systemValidation.validateName(propValue)) {
                            isValid = false;
                        }
                    } else if (propertyType.isRichText) {
                        if (!this.services.validationService.textRtfValidation.hasValueIfRequired(propertyType.isRequired,
                                propValue)) {
                            isValid = false;
                        }
                    } else {
                        if (!this.services.validationService.textValidation.hasValueIfRequired(propertyType.isRequired,
                                propValue)) {
                            isValid = false;
                        }
                    }
                    break;
                case Models.PrimitiveType.Choice:
                    if (propertyType.isMultipleAllowed) {
                        if (!this.services.validationService.multiSelectValidation.isValid(propertyType.isRequired,
                                propValue , propertyType.isValidated, propertyType.validValues)) {
                            isValid = false;
                        }
                    } else {
                        if (!this.services.validationService.selectValidation.isValid(propertyType.isRequired,
                                propValue,  propertyType.isValidated, propertyType.validValues)) {
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
                    if (!this.services.validationService.userPickerValidation.hasValueIfRequired(propertyType.isRequired, value)) {
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
