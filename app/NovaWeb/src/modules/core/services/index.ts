import "angular";
import { ILocalizationService } from "../../core/localization";
import { IMessageService } from "../messages";
import { Models} from "../../main/models";

export interface IStateManager {

}

export class StateManager implements IStateManager {

    private _currentProject: Rx.BehaviorSubject<Models.IProject>;
    private _currentArtifact: Rx.BehaviorSubject<Models.IArtifact>;

    static $inject: [string] = ["localization", "messageService"];
    constructor(
        private localization: ILocalizationService,
        private messageService: IMessageService) {
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
