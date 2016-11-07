import { IStatefulArtifact, StatefulArtifact } from "../../managers/artifact-manager/artifact";
import { IArtifact } from "../../main/models/models";
import { ItemTypePredefined } from "../../main/models/enums";
import {ChangeSetCollector, ChangeTypeEnum, IChangeCollector, IChangeSet} from "../../managers/artifact-manager/changeset";
import {Helper} from "../../shared/utils/helper";

export interface ICollection extends IArtifact {
    reviewName: string;
    isCreated: boolean;        
    artifacts: ICollectionArtifact[];    
}

export interface ICollectionArtifact {
    id: number;
    name: string;
    prefix: string;
    description: string;
    itemTypeId: number;    
    artifactPath: string[];
    itemTypePredefined: ItemTypePredefined;   
}

export interface IStatefulCollectionArtifact extends IStatefulArtifact {
    rapidReviewCreated: boolean;
    reviewName: string;      
    artifacts: ICollectionArtifact[];
    addArtifactsToCollection(artifactIds: IArtifact[]);
    collectionObservable(): Rx.Observable<IChangeSet[]>;
}

export class StatefulCollectionArtifact extends StatefulArtifact implements IStatefulCollectionArtifact {

    private _collectionSubject: Rx.Subject<IChangeSet[]>;

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<IArtifact> {
        const url = `/svc/bpartifactstore/collection/${id}`;
        return this.services.artifactService.getArtifactModel<ICollection>(url, id, versionId);
    }

    public unsubscribe() {
        super.unsubscribe();
        this.collectionSubject.onCompleted();
        delete this._collectionSubject;
    }
   
    protected get collectionSubject(): Rx.Subject<IChangeSet[]> {
        if (!this._collectionSubject) {
            this._collectionSubject = new Rx.Subject<IChangeSet[]>();
        }
        return this._collectionSubject;
    }

    protected updateCollectionSubject(changes: IChangeSet[]) {
        this.collectionSubject.onNext(changes);
    }

    public collectionObservable(): Rx.Observable<IChangeSet[]> {
        return this.collectionSubject.asObservable();
    }

    public get rapidReviewCreated() {
        if (this.artifact) {
            return (<ICollection>this.artifact).isCreated;
        }
        return false;
    }

    public get reviewName() {
        if (this.artifact) {
            return (<ICollection>this.artifact).reviewName;
        }
        return undefined;
    }

    public get artifacts() {
        if (this.artifact) {
            return (<ICollection>this.artifact).artifacts;
        }
        return undefined;
    }   

    public addArtifactsToCollection(artifacts: IArtifact[]) {

        if (this.artifact &&
            artifacts &&
            artifacts.length > 0) {

            let changesets: IChangeSet[] = [];
            artifacts.map((artifact: IArtifact) => {
                const newArtifact = <ICollectionArtifact>{
                    id: artifact.id,
                    description: "",
                    itemTypeId: artifact.itemTypeId,
                    itemTypePredefined: artifact.predefinedType,
                    name: artifact.name,
                    prefix: artifact.prefix,
                    artifactPath: Helper.getArtifactPath(artifact)
                };
                this.artifacts.push(newArtifact);
                const changeset = {
                    type: ChangeTypeEnum.Add,
                    key: artifact.id,
                    value: newArtifact
                } as IChangeSet;
                changesets.push(changeset);
                this.changesets.add(changeset);
            });

            this.lock();

            this.updateCollectionSubject(changesets);
        }
    }    
}
