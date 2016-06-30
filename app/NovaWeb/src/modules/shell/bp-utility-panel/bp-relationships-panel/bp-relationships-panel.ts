import { ILocalizationService } from "../../../core";
import { IProjectManager, Models} from "../../../main";
import {IArtifactRelationships, IArtifactRelationship} from "./artifact-relationships.svc";

interface IOptions {
    value: string;
    label: string;
}


export class BPRelationshipsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-relationships-panel.html");
    public controller: Function = BPRelationshipsPanelController;
}

export class BPRelationshipsPanelController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "projectManager",
        "artifactRelationship"
    ];


    private artifactId: number;
    private _subscribers: Rx.IDisposable[];
    public options: IOptions[];
    public artifactList: IArtifactRelationship[] = [];
    public option: string = "1";

    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private projectManager: IProjectManager,
        private artifactRelationship: IArtifactRelationships) {

        this.options = [     
            { value: "1", label: "Add new" }           
        ];
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
        let selectedArtifactSubscriber: Rx.IDisposable = this.projectManager.currentArtifact.subscribe(this.setArtifactId);
        this._subscribers = [selectedArtifactSubscriber];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        // clean up history list
        this.artifactList = null;
    }


    private setArtifactId = (artifact: Models.IArtifact) => {
        this.artifactList = [];

        if (artifact !== null) {
            this.artifactId = artifact.id;
            this.getRelationships()
                .then((list: any) => {
                    this.artifactList = list;
                });
        }
    }

    private getRelationships(): ng.IPromise<IArtifactRelationship[]> {

        return this.artifactRelationship.getRelationships(this.artifactId)
            .then((list: IArtifactRelationship[]) => {
                return list;
            })
            .finally(() => {

            });
    }
}
