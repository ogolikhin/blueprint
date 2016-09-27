import {
    IProcess,
    IProcessShape,
    IProcessLink,
    IHashMapOfPropertyValues,
    IItemStatus,
    IVersionInfo,
    ItemTypePredefined}
from "../../../../editors/bp-process/models/process-models";
import { IProcessService } from "../../../../editors/bp-process/services/process/process.svc";

import { StatefulArtifact } from "../artifact";
import { IStatefulArtifact } from "../../../../managers/models";

import { Models } from "../../../../main/models";
import { IStatefulProcessArtifactServices } from "../../services";

export class StatefulProcessArtifact extends StatefulArtifact implements IStatefulArtifact, IProcess {
    private totalLoadPromise: ng.IPromise<IStatefulArtifact>;
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

        let artifactPromise = super.load(force);
        
        const totalDeffered = this.services.getDeferred<IStatefulArtifact>();
        const processDeffered = this.services.getDeferred<IStatefulArtifact>();

        if (this.totalLoadPromise) {
            return this.totalLoadPromise;
        }
        this.totalLoadPromise = totalDeffered.promise;

        this.services.processService.load(this.id.toString())
            .then((process: IProcess) => {
                this.onLoad(process);
                console.log("process Loaded");
                processDeffered.resolve(this);
        }).catch((err: any) => {
            processDeffered.reject(err);
        });

        let sourceBase = Rx.Observable.fromPromise(artifactPromise);
        let processPromise = Rx.Observable.fromPromise(processDeffered.promise);

        let combination = Rx.Observable.merge(sourceBase, processPromise);

        let observer = Rx.Observer.create(
            (result: IStatefulArtifact) => {
            },
            err => {
                totalDeffered.reject(err);
            },
            () => {
                console.log("everything Loaded");
                this.totalLoadPromise = null;
                totalDeffered.resolve(this);
            }
        );
        combination.subscribe(observer);
        return totalDeffered.promise;
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