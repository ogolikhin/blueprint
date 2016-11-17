import * as _ from "lodash";
import {ChangeTypeEnum, IChangeCollector, IChangeSet, ChangeSetCollector} from "../changeset";
import {IStatefulArtifactServices} from "../services";
import {IIStatefulArtifact} from "../artifact";
import {IStatefulSubArtifact} from "../sub-artifact";

export interface ISubArtifactCollection {
    initialise(artifacts: IStatefulSubArtifact[]);
    // getObservable(): Rx.Observable<IStatefulSubArtifact[]>;
    list(): IStatefulSubArtifact[];
    add(subArtifact: IStatefulSubArtifact): IStatefulSubArtifact;
    get(id: number): IStatefulSubArtifact;
    remove(id: number): IStatefulSubArtifact;
    discard();
    validate(): ng.IPromise<boolean>;
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
        return _.find(this.subArtifactList, (subArtifact: IStatefulSubArtifact) => subArtifact.id === id);
    }

    public add(subArtifact: IStatefulSubArtifact): IStatefulSubArtifact {
        const length = this.subArtifactList.push(subArtifact);

        const changeset = {
            type: ChangeTypeEnum.Add,
            key: subArtifact.id,
            value: subArtifact
        } as IChangeSet;
        this.changeset.add(changeset);
        this.artifact.lock();

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
        this.subArtifactList.forEach(subArtifact => {
            subArtifact.discard();
        });
    }

    public update(id: number) {
        // TODO:
    }

    public validate(): ng.IPromise<boolean> {
        let p = [];
        let all = true;
        _.each(this.subArtifactList, (item) => {
            p.push(item.validate());
        });
        return this.services.$q.all(p).then((results: boolean[]) => {
            return  _.every(results);
        });

    }
}
