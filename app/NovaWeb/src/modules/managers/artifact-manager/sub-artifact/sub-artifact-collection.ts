// import { Models, Enums } from "../../../main/models";
// import { ArtifactState} from "../state";
// import { IArtifactManager } from "../";
// import { ArtifactAttachments } from "../attachments";
// import { CustomProperties } from "../properties";
// import { ChangeTypeEnum, IChangeCollector, IChangeSet  } from "../../models";
import { IStatefulArtifactServices } from "../services";
import { 
    ChangeSetCollector, 
    IChangeCollector, 
    ChangeTypeEnum, 
    IChangeSet
} from "../";

import {
         IIStatefulArtifact,
         IStatefulSubArtifact
} from "../../models";

export interface ISubArtifactCollection {
    initialise(artifacts: IStatefulSubArtifact[]);
    getObservable(): Rx.Observable<IStatefulSubArtifact[]>;
    list(): IStatefulSubArtifact[];
    add(subArtifact: IStatefulSubArtifact): IStatefulSubArtifact;
    get(id: number): IStatefulSubArtifact;
    remove(id: number): IStatefulSubArtifact;
    discard();
}

export class StatefulSubArtifactCollection implements ISubArtifactCollection {

    private artifact: IIStatefulArtifact;
    private subArtifactList: IStatefulSubArtifact[];
    private services: IStatefulArtifactServices;
    private changeset: IChangeCollector;

    constructor(artifact: IIStatefulArtifact, services: IStatefulArtifactServices) {
        this.artifact = artifact;
        this.services = services;
        this.subArtifactList = [];
        this.changeset = new ChangeSetCollector(artifact);
    }

    public initialise(subartifacts: IStatefulSubArtifact[]) {
        this.subArtifactList = subartifacts;
    }

    public list(): IStatefulSubArtifact[] {
        return this.subArtifactList;
    }

    public get(id: number): IStatefulSubArtifact {
        return this.subArtifactList.filter((subArtifact: IStatefulSubArtifact) => subArtifact.id === id)[0] || null;
    }
    
    public add(subArtifact: IStatefulSubArtifact): IStatefulSubArtifact {
        const length = this.subArtifactList.push(subArtifact);

        const changeset = {
            type: ChangeTypeEnum.Add,
            key: subArtifact.id,
            value: subArtifact
        } as IChangeSet;
        this.changeset.add(changeset);
        //this.artifact.lock();

        return this.subArtifactList[length - 1];
    }

    public remove(id: number): IStatefulSubArtifact {
        let statefulSubArtifact: IStatefulSubArtifact;
        this.subArtifactList = this.subArtifactList.filter((subArtifact: IStatefulSubArtifact) => {
            if (subArtifact.id === id) {
                statefulSubArtifact = subArtifact;

                const changeset = {
                    type: ChangeTypeEnum.Delete,
                    key: subArtifact.id,
                    value: subArtifact
                } as IChangeSet;
                this.changeset.add(changeset);
                this.artifact.lock();

                return false;
            }
            return true;
        });
        return statefulSubArtifact;
    }

    public discard() {
        this.subArtifactList.forEach(subArtifact => { subArtifact.discard(); });
    }

    public update(id: number) {
        // TODO: 
    }
}
