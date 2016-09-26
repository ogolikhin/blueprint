﻿import {
    IProcess,
    IProcessShape,
    IProcessLink,
    IHashMapOfPropertyValues,
    IItemStatus,
    IVersionInfo,
    ItemTypePredefined}
from "../../../../editors/bp-process/models/process-models";

import { StatefulArtifact } from "../artifact";
import { IStatefulArtifact } from "../../../../managers/models";

import { Models } from "../../../../main/models";
import { IStatefulProcessArtifactServices } from "../../services";

export class StatefulProcessArtifact  extends StatefulArtifact implements IStatefulArtifact, IProcess {
    
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
        return this.artifact.predefinedType;
    }
    public get typePrefix(): string {
        return this.artifact.prefix;
    }

    public getServices(): IStatefulProcessArtifactServices {
        return this.services;
    }

    public load(force: boolean = true): ng.IPromise<IStatefulArtifact> {

        const deferred = this.services.getDeferred<IStatefulArtifact>();
        let promise = this.services.processService.load(this.artifact.id.toString())
            .then((process: IProcess) => {
                this.onLoad(process);
                deferred.resolve(this);
        }).catch((err: any) => {
            deferred.reject(err);
        });

        return deferred.promise;
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