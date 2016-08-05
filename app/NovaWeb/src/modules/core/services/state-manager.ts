import "angular";
import { Models} from "../../main/models";


export interface IStateManager {
    onArtifactChanged: Rx.Observable<ItemState>;
    addChangeSet(origin: Models.IArtifact, changeSet: IPropertyChangeSet): void;
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
    }
    public originArtifact: Models.IArtifact;
    public changedArtifact: Models.IArtifact;
    public isLocked: boolean;
    public isReadOnly: boolean;
    
    public get isChanged(): boolean {
        return this._isChanged;
    }

    public addChange(changeSet: IPropertyChangeSet) {
        if (!changeSet) {
            return;
        }
        if (!this.changedArtifact) {
            this.changedArtifact = {
                id: this.originArtifact.id
            } as Models.IArtifact;
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
                    customProperty = this.originArtifact.customPropertyValues.filter((it: Models.IPropertyValue) => {
                        return it.propertyTypeId === propertyTypeId;
                    })[0];
                    if (customProperty) {
                        customProperty = angular.copy(customProperty);
                        customProperty.value = changeSet.value;

                        if (!angular.isArray(this.changedArtifact.customPropertyValues)) {
                            this.changedArtifact.customPropertyValues = []
                        }

                        this.changedArtifact.customPropertyValues.push(customProperty);
                    } else {
                        return;
                    }
                }
                break; 
            case "special":
                //TODO: needs to be implemented
                break; 
            default:
                break;
        }
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


}
