import {IProjectManager, Models} from "../..";

export class BpArtifactDetails implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-details.html");
    public controller: Function = BpArtifactDetailsController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        currentArtifact: "<",
    };
    public transclude: boolean = true;
}

export class BpArtifactDetailsController {
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
    public model = {};
    public tabs = [];


    public fields: Models.IArtifactDetailFields = {
        systemFields: [],
        customFields: [],
        noteFields: []
    };

    public get isCustomPropertyAvailable(): boolean {
        return this.fields && this.fields.customFields && this.fields.customFields.length > 0;
    }
    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        this._artifact = null;
        this.model = null;
        this.fields = null;
        this.tabs = null;

    }

    private createModel(artifact: Models.IArtifactDetails): any {
        let _model = {
            name: artifact.name,
            type: artifact.predefinedType.toString(),
            createdBy: "user",
            createdOn: new Date(),
            lastEditedBy: "user",
            lastEditedOn: new Date(),
        };
        for (let key in artifact) {
            if (key.toLowerCase() === "properyValues") {
                    <Models.IPropertyValue>artifact[key].forEach((it: Models.IPropertyValue) => {
                        _model[`property_${it.typeId}`] = it.value;
                    })
            }
        };
        return _model;
    }
    private updateArtifact(artifact: Models.IArtifactDetails): any {
        return {};
    }

//    private properties: Models.IPropertyType[];
    public loadView(artifact: Models.IArtifactDetails) {
        if (!artifact) {
            return;
        }
        this._artifact = angular.copy(artifact);
        this.model = this.createModel(this._artifact);
        this.fields = this.projectManager.getArtifactPropertyFileds(this._artifact);
        this.tabs = this.fields.noteFields.map((it: AngularFormly.IFieldConfigurationObject, index: number) => {
            let tab = {
                title: it.templateOptions.label,
                fields: [it],
                active: index === 0,
                disable: it.templateOptions.disabled
            };
            delete it.templateOptions.label;
            return tab;
        })

    }

}
