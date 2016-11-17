import { Models } from "../../main/models";
import { Message, MessageType } from "../../core/messages/message";
import { IProcess, IProcessShape, IProcessLink } from "./models/process-models";
import { IHashMapOfPropertyValues } from "./models/process-models";
import { IVersionInfo, ItemTypePredefined } from "./models/process-models";
import { StatefulArtifact, IStatefulArtifact } from "../../managers/artifact-manager/artifact";
import { IStatefulProcessArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulProcessSubArtifact } from "./process-subartifact";
import { IProcessUpdateResult } from "./services/process.svc";

export interface IStatefulProcessArtifact extends  IStatefulArtifact {
    processOnUpdate();
}

export class StatefulProcessArtifact extends StatefulArtifact implements IStatefulProcessArtifact, IProcess {

    private loadProcessPromise: ng.IPromise<IStatefulArtifact>;
    private artifactPropertyTypes: {} = null;
    public shapes: IProcessShape[];
    public links: IProcessLink[];
    public decisionBranchDestinationLinks: IProcessLink[];
    public propertyValues: IHashMapOfPropertyValues;
    public requestedVersionInfo: IVersionInfo;
    protected hasCustomSave: boolean = true;

    constructor(artifact: Models.IArtifact, protected services: IStatefulProcessArtifactServices) {
        super(artifact, services);
    }

    public processOnUpdate() {
        this.artifactState.dirty = true;
        this.lock();
    }

    public get baseItemTypePredefined(): ItemTypePredefined {
        return this.predefinedType;
    }

    public get typePrefix(): string {
        return this.prefix;
    }

    public getServices(): IStatefulProcessArtifactServices {
        return this.services;
    }

    protected getCustomArtifactPromisesForGetObservable(): angular.IPromise<IStatefulArtifact>[] {
        this.loadProcessPromise = this.loadProcess();

        return [this.loadProcessPromise];
    }

    protected getCustomArtifactPromiseForSave(): angular.IPromise<IStatefulArtifact> {
        let saveProcessPromise = this.saveProcess();
        return saveProcessPromise;
    }

    private getArtifactPropertyTypes(): ng.IPromise<any> {
        const deferred = this.services.getDeferred<any>();
        if (this.artifactPropertyTypes === null) {
            this.services.metaDataService.getArtifactPropertyTypes(this.projectId, this.artifact.itemTypeId).then((result) => {
                this.artifactPropertyTypes = {};

                _.each(result, (propType) => {
                    if (!!propType.id && !this.artifactPropertyTypes[propType.id.toString()]) {
                        this.artifactPropertyTypes[propType.id.toString()] = propType;
                    }
                });

                deferred.resolve();
            }).catch((err) => {
                // log error?
                deferred.reject(err);
            });
        } else {
            deferred.resolve();
        }
        return deferred.promise;
    }

    protected validateCustomArtifactPromiseForSave(changes:  Models.IArtifact, ignoreValidation: boolean): ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        if (ignoreValidation) {
            deferred.resolve(this);
        }
        this.getArtifactPropertyTypes().then((artifactPropertyTypes) => {
            _.each(changes.customPropertyValues, (propValue) => {
                const itemType: Models.IPropertyType = this.artifactPropertyTypes[propValue.propertyTypeId];
                switch (itemType.primitiveType) {
                    case Models.PrimitiveType.Number:
                        if (!this.services.validationService.numberValidation.isValid(propValue.value, 
                            propValue.value,
                            itemType.decimalPlaces,
                            this.services.localizationService,
                            itemType.minNumber,
                            itemType.maxNumber,
                            itemType.isValidated,
                            itemType.isRequired)) {
                            return this.set_400_114_error(deferred);
                        }
                        break;
                    case Models.PrimitiveType.Date:
                        if (!this.services.validationService.dateValidation.isValid(propValue.value,
                            propValue.value,
                            this.services.localizationService,
                            itemType.minDate,
                            itemType.maxDate,
                            itemType.isValidated,
                            itemType.isRequired)) {
                            return this.set_400_114_error(deferred);
                        }
                        break;
                    case Models.PrimitiveType.Text:
                        if (itemType.isRichText) {
                            if (!this.services.validationService.textRtfValidation.hasValueIfRequred(itemType.isRequired, 
                                propValue.value,
                                propValue.value)) {
                                return this.set_400_114_error(deferred);
                            } 
                        } else {
                            if (!this.services.validationService.textValidation.hasValueIfRequred(itemType.isRequired, 
                                propValue.value,
                                propValue.value)) {
                                return this.set_400_114_error(deferred);
                            } 
                        }
                        break;
                    case Models.PrimitiveType.Choice:
                        if (itemType.isMultipleAllowed) {
                            if (!this.services.validationService.multiSelectValidation.hasValueIfRequred(itemType.isRequired, 
                                propValue.value,
                                propValue.value)) {
                                return this.set_400_114_error(deferred);
                            } 
                        } else {
                            if (!this.services.validationService.selectValidation.hasValueIfRequred(itemType.isRequired, 
                                propValue.value,
                                propValue.value)) {
                                return this.set_400_114_error(deferred);
                            } 
                        }
                        break;
                    case Models.PrimitiveType.User:
                        // user group values gets wrapped in this [usersGroup] property in bp-property-editor.ts, in 'convertToModelValue' method.
                        const userOrGroupValue = propValue.value ? propValue.value.usersGroups : propValue.value;
                        if (!this.services.validationService.userPickerValidation.hasValueIfRequred(itemType.isRequired, 
                            userOrGroupValue,
                            userOrGroupValue)) {
                            return this.set_400_114_error(deferred);
                        } 
                        break;
                    default:
                        deferred.reject(new Error(this.services.localizationService.get("App_Save_Artifact_Error_Other")));
                        break;
                }
            });
            deferred.resolve();
        }).catch((err) => {
            // log error?
            deferred.reject(err);
        });
        return deferred.promise;
    }

    protected customHandleSaveFailed(): void {
        this.notifySubscribers();
    }

    protected runPostGetObservable() {
        this.loadProcessPromise = null;
    }

    // Returns promises for operations that are needed to refresh this process artifact
    public getCustomArtifactPromisesForRefresh(): ng.IPromise<any>[] {
        const loadProcessPromise = this.loadProcess();

        return [loadProcessPromise];
    }

    protected isFullArtifactLoadedOrLoading(): boolean {
        return super.isFullArtifactLoadedOrLoading() || !!this.loadProcessPromise;
    }

    private loadProcess(): ng.IPromise<IStatefulArtifact> {
        const processDeffered = this.services.getDeferred<IStatefulArtifact>();

        this.services.processService.load(this.id.toString(), this.getEffectiveVersion())
            .then((process: IProcess) => {
                this.onLoad(process);
                processDeffered.resolve(this);
            })
            .catch((err: any) => {
                processDeffered.reject(err);
            });

        return processDeffered.promise;
    }

    private onLoad(newProcess: IProcess) {
        this.initializeSubArtifacts(newProcess);

        const currentProcess = <IProcess>this;
        // TODO: updating name seems to cause an infinite loading of process, something with base class's 'set' logic.
        //currentProcess.name = newProcess.name;
        currentProcess.links = newProcess.links;
        currentProcess.decisionBranchDestinationLinks = newProcess.decisionBranchDestinationLinks;
        currentProcess.propertyValues = newProcess.propertyValues;
        currentProcess.requestedVersionInfo = newProcess.requestedVersionInfo;
    }

    private mapTempIdsAfterSave(tempIdMap: Models.IKeyValuePair[]) {
        if (tempIdMap && tempIdMap.length > 0) {

            for (let counter = 0; counter < tempIdMap.length; counter++) {

                //update decisionBranchDestinationLinks temporary ids
                if (this.decisionBranchDestinationLinks) {
                    this.decisionBranchDestinationLinks.forEach((link) => {
                        if (link.destinationId === tempIdMap[counter].key) {
                            link.destinationId = tempIdMap[counter].value;
                        }
                        if (link.sourceId === tempIdMap[counter].key) {
                            link.sourceId = tempIdMap[counter].value;
                        }
                    });

                //Update shapes temporary ids
                for (let sCounter = 0; sCounter < this.shapes.length; sCounter++) {
                        const shape = this.shapes[sCounter];
                        if (shape.id <= 0 && shape.id === tempIdMap[counter].key) {
                            shape.id = tempIdMap[counter].value;
                            break;
                        }
                    }
                }

                //Update links temporary ids
                if (this.links) {
                    this.links.forEach((link) => {
                        if (link.destinationId === tempIdMap[counter].key) {
                            link.destinationId = tempIdMap[counter].value;
                        }
                        if (link.sourceId === tempIdMap[counter].key) {
                            link.sourceId = tempIdMap[counter].value;
                        }
                    });
                }
            }

            //update sub artifact collection temporary ids
            this.subArtifactCollection.list().forEach(item => {
                if (item.id <= 0) {
                    // subartifact id is temporary
                    for (let i = 0; i < tempIdMap.length; i++) {
                        if (item.id === tempIdMap[i].key) {
                            item.id = tempIdMap[i].value;
                            break;
                        }
                    }
                }
            });
        }
    }

    private initializeSubArtifacts(newProcess: IProcess) {
        const statefulSubArtifacts = [];
        this.shapes = [];

        for (const shape of newProcess.shapes) {
            const statefulShape = new StatefulProcessSubArtifact(this, shape, this.services);
            this.shapes.push(statefulShape);
            statefulSubArtifacts.push(statefulShape);
        }

        this.subArtifactCollection.initialise(statefulSubArtifacts);
    }
    private saveProcess(): ng.IPromise<IStatefulArtifact> {
        const deferred = this.services.getDeferred<IStatefulArtifact>();
        if (!this.artifactState.readonly) {
            this.services.processService.save(<IProcess>this)
                .then((result: IProcessUpdateResult) => {
                    this.mapTempIdsAfterSave(result.tempIdMap);
                    deferred.resolve(this);
                }).catch((error: any) => { 
                    // if error is undefined it means that it handled on upper level (http-error-interceptor.ts)
                    if (error) {
                        deferred.reject(this.handleSaveError(error));
                    } else {
                        deferred.reject(error);
                    }
                });
        } else {
            let message = new Message(MessageType.Error,
                this.services.localizationService.get("ST_View_OpenedInReadonly_Message"));
            this.services.messageService.addMessage(message);
            deferred.reject();
        }
        
        return deferred.promise;
    }

}
