import "angular";
import { Models} from "../../main/models";

export interface IStateManager {
    isArtifactChanged: boolean;
    isArtifactChangedObservable: Rx.Observable<boolean>;
    addChangeSet(origin: Models.IArtifact, changeSet: IPropertyChangeSet): void;
}

export interface IPropertyChangeSet {
    lookup: string
    key: string | number;
    value: any;
}

export class ArtifactState {
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
                this.changedArtifact[changeSet.key] = changeSet.value;
                break; 
            case "custom":
                if (!angular.isArray(this.changedArtifact.customPropertyValues)) {
                    this.changedArtifact.customPropertyValues = []
                }
                let propertyTypeId = changeSet.key as number;
                let customProperty = this.changedArtifact.customPropertyValues.filter((it: Models.IPropertyValue) => {
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
                        this.changedArtifact.customPropertyValues.push(customProperty);
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

    private _collection: ArtifactState[];

    private _currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    private _isArtifactChanged: Rx.BehaviorSubject<boolean>;

    constructor() {
        this._collection = [];
        this._isArtifactChanged = new Rx.BehaviorSubject<boolean>(false);
    }


    public addChangeSet(originArtifact: Models.IArtifact, changeSet: IPropertyChangeSet) {
        let artifactState = this._collection.filter((it: ArtifactState) => {
            return it.originArtifact.id === originArtifact.id;
        })[0];
        if (!artifactState) {
            this._collection.push(artifactState = new ArtifactState(originArtifact));
        }

        artifactState.addChange(changeSet);
        this.isArtifactChanged = true;
    }


    public get isArtifactChanged(): boolean {
        return this._isArtifactChanged.getValue();
    }

    public set isArtifactChanged(value: boolean) {
        if (!this.isArtifactChanged) {
            this._isArtifactChanged.onNext(value);
        }
    }

    public get isArtifactChangedObservable(): Rx.Observable<boolean> {
        return this._isArtifactChanged.asObservable();
    }


    public dispose() {
        //clear all subjects
        this._collection = null;
        if (this._currentArtifact) {
            this._currentArtifact.dispose();
        }
    }

    public initialize = () => {
        //subscribe to event
        this.dispose();
        this._currentArtifact = new Rx.BehaviorSubject<Models.IArtifact>(null);
    }



}
