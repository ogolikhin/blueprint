import {IProjectManager, Models} from "../..";

export class BpArtifactDetails implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-details.html");
    public controller: Function = BpArtifactRetailsController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        currentArtifact: "<",
    };
    public transclude: boolean = true;
}

export class BpArtifactRetailsController {
    private _subscribers: Rx.IDisposable[];
    static $inject: [string] = ["$scope", "projectManager"];
    private _artifact: Models.IArtifactDetails;

    public currentArtifact: string;

    constructor(private $scope, private projectManager: IProjectManager) {

    }
    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.projectManager.currentArtifact.subscribeOnNext(this.loadView, this),
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }


    public loadView(artifact: Models.IArtifactDetails) {
        this._artifact = artifact;
    }

}
