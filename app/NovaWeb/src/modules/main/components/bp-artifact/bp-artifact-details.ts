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
    public model = {
        firstName: "John",
        //sample data
        tinymceControl: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum nunc felis, ullamcorper sed egestas vel, vehicula at lectus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin scelerisque eget ipsum ac iaculis. Etiam sed feugiat nibh, sit amet dictum risus. Phasellus molestie lectus lobortis, luctus purus at, rutrum lectus.",
        tinymceInlineControl: "Fusce pellentesque pellentesque augue, sit amet ultricies mauris dictum sit amet. Vestibulum sed leo suscipit, dignissim nisi non, dictum tellus. Etiam tincidunt nisl at ante vehicula, vitae pretium eros semper. Maecenas eu lacus faucibus, pretium sapien in, ullamcorper magna. Aenean eget bibendum orci, sit amet rutrum metus. Mauris non justo at mauris viverra ultricies sed vitae odio. Nunc volutpat nisi ac magna efficitur, ut dignissim erat sodales. In non lorem mi. Nam ipsum lectus, luctus vitae tellus quis, porta imperdiet nisi. Sed vehicula risus vitae dolor aliquet lacinia. Nam convallis gravida enim. Etiam congue quam in lectus iaculis, at pretium libero ultrices. Integer tempus nunc sed eleifend imperdiet. Cras sed tempus felis, sed sodales ante. Ut auctor vitae dolor eget blandit."
    };
    public fields: Models.IArtifactDetailFields = {
        systemFields: [],
        customFields: [],
        noteFields: []
    };

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

//    private properties: Models.IPropertyType[];
    public loadView(artifact: Models.IArtifactDetails) {
        if (!artifact) {
            return;
        }
        this._artifact = artifact;
        this.fields = this.projectManager.getArtifactPropertyFileds(artifact);
    }

}
