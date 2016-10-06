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
import { Models } from "../../main/models";
import { IStatefulProcessArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulProcessSubArtifact } from "./process-subartifact";

export class StatefulProcessArtifact extends StatefulArtifact implements IStatefulArtifact, IProcess {

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
    public get baseItemTypePredefined(): ItemTypePredefined {
        return this.predefinedType;
    }
    public get typePrefix(): string {
        return this.prefix;
    }
    
    public getServices(): IStatefulProcessArtifactServices {
        return this.services;
    }
    
    public getObservable(): Rx.Observable<IStatefulArtifact> {
        if (!this.isFullArtifactLoadedOrLoading()) {
            this.loadPromise = this.load();
            this.loadProcessPromise = this.loadProcess();

            this.getServices().$q.all([this.loadPromise, this.loadProcessPromise]).then(() => {
                this.subject.onNext(this);
            }).catch((error) => {
                this.artifactState.readonly = true;
                this.subject.onError(error);
            }).finally(() => {
                this.loadPromise = null;
                this.loadProcessPromise = null;
            });
        }

        return this.subject.filter(it => !!it).asObservable();
    }

    public getPromisesForRefreshOperations (): ng.IPromise<any>[] {
        // Note: this method overrides the base class method of the same name.
        // It returns promises for operations that are needed to refresh
        // this process artifact

        var promises: ng.IPromise<IStatefulArtifact>[];
        var loadProcessPromise = this.loadProcess();

        promises = [loadProcessPromise];

        return promises;
    }

    protected isFullArtifactLoadedOrLoading() {
        return (this._customProperties && this._customProperties.isLoaded &&
         this._specialProperties && this._specialProperties.isLoaded)  || 
         this.loadPromise || this.loadProcessPromise;
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