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
//            this.projectManager.currentArtifact.subscribeOnNext(this.loadView, this),
        ];
    }
    public model = {
        firstName: "John"
    };
    public fields = [
        {
            className: "",
            fieldGroup: [{
                className: "property-group",
                key: "name",
                type: "input",
                templateOptions: {
                    label: "Name",
                    required: true
                }
            },
            {   className: "property-group",
                key: "type",
                    type: "select",
                    templateOptions: {
                    label: "Type",
                    options: [
                        {
                            "name": "Snickers",
                            "value": "snickers"
                        },
                        {
                            "name": "Baby Ruth",
                            "value": "baby_ruth"
                        }]
                    }
                },
                {
                    className: "property-group",
                    key: "createdBy",
                    type: "input",
                    templateOptions: {
                        label: "Created by",
                    }
                },
                {
                    className: "property-group",
                    key: "createdOn",
                    type: "input",
                    templateOptions: {
                        label: "Created on",
                    }
                },
                {
                    className: "property-group",
                    key: "lastEditBy",
                    type: "input",
                    templateOptions: {
                        label: "Last edited by",
                    }
                },
                {
                    className: "property-group",
                    key: "lastEditOn",
                    type: "input",
                    templateOptions: {
                        label: "Last edited by",
                    }
                },
            ]
        },
    ];
    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private properties: Models.IPropertyType[];
    public loadView(artifact: Models.IArtifactDetails) {
        this._artifact = artifact;
        this.properties = this.projectManager.getArtifactPropertyFileds(artifact);
        this.properties.forEach((it: Models.IPropertyType) => {
            return {
                key: it.name,
                type: "input",
                templateOptions: {
                    type: "text",
                    label: "Last Name",
                    required: it
                }
            };
        });
    }

}
