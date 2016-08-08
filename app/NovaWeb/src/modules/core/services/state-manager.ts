import "angular";
import { Models} from "../../main/models";

export interface IStateManager {
    onArtifactChanged: Rx.Observable<ItemState>;
    addChangeSet(origin: Models.IArtifact, changeSet: IPropertyChangeSet): void;
    getArtifactState(artifact: number | Models.IArtifact): ItemState;
    deleteArtifactState(artifact: number | Models.IArtifact);
}

export interface IPropertyChangeSet {
    lookup: string
    id: string | number;
    value: any;
}

export class ItemState {
    private _isChanged: boolean = false;
    constructor(artifact: Models.IArtifact) {
        this.originArtifact = artifact;
        this._changes = [];
    }

    public originArtifact: Models.IArtifact;
    public isLocked: boolean = false;;
    public isReadOnly: boolean = false;

    private _changes: IPropertyChangeSet[] ;
    private _changedArtifact: Models.IArtifact;
    
    public get isChanged(): boolean {
        return this._isChanged;
    }

    public get changedArtifact(): Models.IArtifact {
        return this._changedArtifact || (this._changedArtifact = angular.copy(this.originArtifact));
    }

    public clear() {
        this.originArtifact = null;
        this._changedArtifact = null;
        this._changes = null;
        this._isChanged = false;
        this.isLocked = false;
        this.isReadOnly = false;
    }

    private saveChangeSet(changeSet: IPropertyChangeSet) {
        this._changes.push(changeSet)
    }

    public addChange(changeSet: IPropertyChangeSet) {
        if (!changeSet) {
            return;
        }
        switch ((changeSet.lookup || "").toLowerCase()) {
            case "system":
                if (changeSet.id in this.originArtifact) {
                    this.changedArtifact[changeSet.id] = changeSet.value;
                } else {
                    return;
                }
                break; 
            case "custom":
                let propertyTypeId = changeSet.id as number;
                let customProperty = (this.changedArtifact.customPropertyValues || []).filter((it: Models.IPropertyValue) => {
                    return it.propertyTypeId === propertyTypeId;
                })[0];
                if (customProperty) {
                    customProperty.value = changeSet.value;
                } else {
                    return;
                }
                break; 
            case "special":
                //TODO: needs to be implemented
                break; 
            default:
                break;
        }
        this._changes.push(changeSet);
        this._isChanged = true;
    }

}

export class StateManager implements IStateManager {

    private _collection: ItemState[];
    private _currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    private _artifactChanged: Rx.BehaviorSubject<ItemState>;

    constructor() {
        this._collection = [];
        this._artifactChanged = new Rx.BehaviorSubject<ItemState>(null);
    }

    public dispose() {
        //clear all subjects
        this._collection = null;
        if (this._currentArtifact) {
            this._currentArtifact.dispose();
        }
    }

    public addChangeSet(originArtifact: Models.IArtifact, changeSet: IPropertyChangeSet) {
        let artifactState = this._collection.filter((it: ItemState) => {
            return it.originArtifact.id === originArtifact.id;
        })[0];
        if (!artifactState) {
            this._collection.push(artifactState = new ItemState(originArtifact));
        }

        artifactState.addChange(changeSet);
        this._artifactChanged.onNext(artifactState);
    }


    public get onArtifactChanged(): Rx.Observable<ItemState> {
        return this._artifactChanged
            .filter(it => it != null)
            .asObservable();
    }

    public getArtifactState(artifact: number | Models.IArtifact): ItemState {
        let artifactId = angular.isNumber(artifact) ? artifact as number : (artifact ? artifact.id : -1);
        let state: ItemState = this._collection.filter((it: ItemState) => {
            return it.originArtifact.id === artifactId;
        })[0];

        return state;
    }

    public deleteArtifactState(artifact: number | Models.IArtifact) {
        let artifactId = angular.isNumber(artifact) ? artifact as number : (artifact ? artifact.id : -1);
        this._collection = this._collection.filter((it: ItemState) => {
            if (it.originArtifact.id === artifactId) {
                it.clear();
                return false;
            }
            return true;
        });
    }
}
