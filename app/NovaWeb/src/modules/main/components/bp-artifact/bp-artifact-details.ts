import {IProjectManager, Models} from "../..";

require("script!mxClient");

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
    static $inject: [string] = ["$scope", "$element", "projectManager"];
    private _artifact: Models.IArtifactDetails;

    public currentArtifact: string;

    constructor(private $scope, $element, private projectManager: IProjectManager) {
        let container = $element[0].children[0].children[1].children[0];
        let model: MxGraphModel = new mxGraphModel();
        let graph: MxGraph = new mxGraph(container, null);
        // Gets the default parent for inserting new cells. This
        // is normally the first child of the root (ie. layer 0).
        var parent = graph.getDefaultParent();

        // Adds cells to the model in a single step
        model.beginUpdate();
        try {
            var v1 = graph.insertVertex(parent, null, 'Hello,', 20, 20, 80, 30);
            var v2 = graph.insertVertex(parent, null, 'World!', 200, 150, 80, 30);
            var e1 = graph.insertEdge(parent, null, '', v1, v2);
        }
        finally {
            // Updates the display
            model.endUpdate();
        }
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
