import "angular";
import { Models} from "../../main/models";

export interface IStateManager {
    isArtifactChanged: boolean;
    isArtifactChangedObservable: Rx.Observable<boolean>;
}

export class StateManager implements IStateManager {

    private _currentProject: Rx.BehaviorSubject<Models.IProject>;
    private _currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    private _isArtifactChanged: Rx.BehaviorSubject<boolean>;


//    static $inject: [string] = [];
    constructor() {
        this._isArtifactChanged = new Rx.BehaviorSubject<boolean>(false);
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
        if (this._currentProject) {
            this._currentProject.dispose();
        }
        if (this._currentArtifact) {
            this._currentArtifact.dispose();
        }
    }

    public initialize = () => {
        //subscribe to event
        this.dispose();
        this._currentProject = new Rx.BehaviorSubject<Models.IProject>(null);
        this._currentArtifact = new Rx.BehaviorSubject<Models.IArtifact>(null);
    }



}
