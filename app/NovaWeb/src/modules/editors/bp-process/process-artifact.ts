import {
    IProcess,
    IProcessShape,
    IProcessLink,
    IHashMapOfPropertyValues,
    IItemStatus,
    IVersionInfo,
    ItemTypePredefined}
from "./models/process-models";

import { StatefulArtifact } from "../../managers/artifact-manager/artifact";
import { IStatefulArtifact } from "../../managers/models";

import { Models } from "../../main/models";
import { IStatefulProcessArtifactServices } from "../../managers/artifact-manager/services";

export class StatefulProcessArtifact extends StatefulArtifact implements IStatefulArtifact, IProcess {

    private finalLoadPromise: ng.IPromise<IStatefulArtifact>;

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

    
    public load(force: boolean = true): ng.IPromise<IStatefulArtifact> {
        
        const finalDeffered = this.services.getDeferred<IStatefulArtifact>();
        if (this.finalLoadPromise) {
            return this.finalLoadPromise;
        }
        this.finalLoadPromise = finalDeffered.promise;

        let artifactPromise = super.load(force);
        let processPromise = this.loadProcess();

        let artifactObservable = Rx.Observable.fromPromise(artifactPromise);
        let processObservable = Rx.Observable.fromPromise(processPromise);

        let combination = Rx.Observable.merge(artifactObservable, processObservable);

        let observer = Rx.Observer.create(
            (result: IStatefulArtifact) => {
            },
            err => {
                this.finalLoadPromise = null;
                finalDeffered.reject(err);
            },
            () => {
                this.finalLoadPromise = null;
                finalDeffered.resolve(this);
            }
        );
        combination.subscribe(observer);
        return finalDeffered.promise;
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
    private onLoad(process: IProcess) {
        // TODO: updating name seems to cause an infinite loading of process, something with base class's 'set' logic.
        //(<IProcess>this).name = process.name;
        (<IProcess>this).shapes = process.shapes;
        (<IProcess>this).links = process.links;
        (<IProcess>this).decisionBranchDestinationLinks = process.decisionBranchDestinationLinks;
        (<IProcess>this).propertyValues = process.propertyValues;
        (<IProcess>this).requestedVersionInfo = process.requestedVersionInfo;
        (<IProcess>this).status = process.status;
    }
}