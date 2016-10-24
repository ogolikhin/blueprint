import {
    IProcess,
    IProcessShape,
    IProcessLink,
    IHashMapOfPropertyValues,
    IVersionInfo,
    ItemTypePredefined
} from "./models/process-models";
import { StatefulArtifact, IStatefulArtifact } from "../../managers/artifact-manager/artifact";
import { Models } from "../../main/models";
import { IStatefulProcessArtifactServices } from "../../managers/artifact-manager/services";
import { StatefulProcessSubArtifact } from "./process-subartifact";

export interface IStatefulProcessArtifact extends  IStatefulArtifact {
    processOnUpdate();
}

export class StatefulProcessArtifact extends StatefulArtifact implements IStatefulProcessArtifact, IProcess {
    private loadProcessPromise: ng.IPromise<IStatefulArtifact>;

    public shapes: IProcessShape[];
    public links: IProcessLink[];
    public decisionBranchDestinationLinks: IProcessLink[];
    public propertyValues: IHashMapOfPropertyValues;
    public requestedVersionInfo: IVersionInfo;

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

    protected runPostGetObservable() {
        this.loadProcessPromise = null;
    }

    // Returns promises for operations that are needed to refresh this process artifact
    public getCustomArtifactPromisesForRefresh(): ng.IPromise<any>[] {
        const loadProcessPromise = this.loadProcess();

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
}
