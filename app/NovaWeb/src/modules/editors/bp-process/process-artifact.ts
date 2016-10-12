import {
    IProcess,
    IProcessShape,
    IProcessLink,
    IHashMapOfPropertyValues,
    IItemStatus,
    IVersionInfo,
    ItemTypePredefined}
from "./models/process-models";

import { StatefulArtifact, IStatefulArtifact } from "../../managers/artifact-manager/artifact";
import { IStatefulItem } from "../../managers/artifact-manager/item";
import { ChangeTypeEnum, IChangeSet } from "../../managers/artifact-manager/changeset";
import { Models } from "../../main/models";
import { IStatefulProcessArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulProcessSubArtifact } from "./process-subartifact";

export interface IStatefulProcessItem extends IStatefulItem {
    addChangeset(name: string, value: any);
}
export interface IStatefulProcessArtifact extends IStatefulProcessItem, IStatefulArtifact {
}

export class StatefulProcessArtifact extends StatefulArtifact implements IStatefulProcessArtifact, IProcess {

    // private finalLoadPromise: ng.IPromise<IStatefulArtifact>;
    private loadProcessPromise: ng.IPromise<IStatefulArtifact>;

    public shapes: IProcessShape[];
    public links: IProcessLink[];
    public decisionBranchDestinationLinks: IProcessLink[];
    public propertyValues: IHashMapOfPropertyValues;
    public status: IItemStatus;
    public requestedVersionInfo: IVersionInfo;

    constructor(artifact: Models.IArtifact, protected services: IStatefulProcessArtifactServices) {
        super(artifact, services);
    }
    

    public addChangeset(name: string, value: any) {
        const changeset = {
            type: ChangeTypeEnum.Update,
            key: name,
            value: value              
        } as IChangeSet;
        this.changesets.add(changeset);
        
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
   
    protected runPostGetObservable() {
        this.loadProcessPromise = null;
    }

    public getCustomArtifactPromisesForRefresh (): ng.IPromise<any>[] {
        // Returns promises for operations that are needed to refresh
        // this process artifact

        var loadProcessPromise = this.loadProcess();

        return [loadProcessPromise];
    }

    protected isFullArtifactLoadedOrLoading() {
        return super.isFullArtifactLoadedOrLoading() || this.loadProcessPromise;
    }

    private loadProcess(): ng.IPromise<IStatefulArtifact> {
        const processDeffered = this.services.getDeferred<IStatefulArtifact>();
        this.services.processService.load(this.id.toString())
            .then((process: IProcess) => {
                this.onLoad(process);
                processDeffered.resolve(this);
            }).catch((err: any) => {
                processDeffered.reject(err);
            });
        return processDeffered.promise;
    }

    private onLoad(newProcess: IProcess) {
        // TODO: updating name seems to cause an infinite loading of process, something with base class's 'set' logic.
        //(<IProcess>this).name = process.name;
        let currentProcess = <IProcess> this;
        this.initializeSubArtifacts(newProcess);
        currentProcess.links = newProcess.links;
        currentProcess.decisionBranchDestinationLinks = newProcess.decisionBranchDestinationLinks;
        currentProcess.propertyValues = newProcess.propertyValues;
        currentProcess.requestedVersionInfo = newProcess.requestedVersionInfo;
        currentProcess.status = newProcess.status;
    }
    
    private initializeSubArtifacts(newProcess: IProcess) {

        let statefulSubArtifacts: StatefulProcessSubArtifact[] = newProcess.shapes.map((shape: IProcessShape) => {
            return new StatefulProcessSubArtifact(this, shape, this.services);
        });

        this.shapes = newProcess.shapes;
        this.subArtifactCollection.initialise(statefulSubArtifacts);
    }
}