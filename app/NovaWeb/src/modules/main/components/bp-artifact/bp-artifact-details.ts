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

interface IFieldTab {
    title: string,
    index: number,
    fields: [AngularFormly.IFieldConfigurationObject],
    active?: boolean
};

class FieldType {
    public id: number;
    public name: string;
    public value: any;
    public predefinedType: Models.PropertyTypePredefined;
    public propertyType: Models.IPropertyType;
    public get group(): string {
        if (this.predefinedType == Models.PropertyTypePredefined.name)
            return "system";
        //else {
        //    return "tabbed"
        //}
        return "custom";
    }

    constructor(name: string | number, value: any, predefinedType?: Models.PropertyTypePredefined) {
        this.predefinedType = predefinedType; 
        if (typeof name === "string") {
            this.name = name;
            if (!this.predefinedType) {
                this.predefinedType = Models.PropertyTypePredefined[name];
            }
        } else {
            this.id = <number>name;
            this.name = Models.PropertyTypePredefined[predefinedType] || `property_${name}`;
        }
         
        this.value = value;
        
    }
}


export class BpArtifactDetailsController {
    private _subscribers: Rx.IDisposable[];
    static $inject: [string] = ["$scope", "projectManager"];
    private _artifact: Models.IArtifact;

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
    public activeTab: number = 1;


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



    private createModel(artifact: Models.IArtifact): any {
        let system: [Models.PropertyTypePredefined] = [
            Models.PropertyTypePredefined.name,
            Models.PropertyTypePredefined.createdby,
            Models.PropertyTypePredefined.createdon,
            Models.PropertyTypePredefined.lasteditedby,
            Models.PropertyTypePredefined.lasteditedon,
        ]

        let _model = {};
        for (let key in artifact) {
            switch (key.toLowerCase()) {
                case "properyvalues":
                    <Models.IPropertyValue>artifact[key].forEach((it: Models.IPropertyValue) => {
                        let name: string;
                        if (system.indexOf(it.propertyTypePredefined) > -1) {
                            name = Models.PropertyTypePredefined[it.propertyTypePredefined];
                        } else {
                            name = `property_${it.propertyTypeId}`;
                        }
                        _model[name] = it.value;
                    });
                    break;
                default:
                    _model[key] = artifact[key];
                    break;

            };
        }
        return _model;
    }

    private createFields(artifact: Models.IArtifact): any {
        let fields: FieldType[] = [];
        //let _model = {
        //    id: artifact.id,
        //    name: artifact.name,
        //    itemTypeId: artifact.predefinedType.toString(),
        //};
        for (let key in artifact) {
            switch (key.toLowerCase()) {
                case "properyvalues":
                    <Models.IPropertyValue>artifact[key].forEach((it: Models.IPropertyValue) => {
                        fields.push(new FieldType(it.propertyTypeId, it.value, it.propertyTypePredefined))
                    });
                    break;

                default:
                    let predefined = Models.PropertyTypePredefined[key.toLowerCase()];
                    if (predefined)
                        fields.push(new FieldType(key, artifact[key]));
                    break;


            }
        };
        return fields;
    }

    private updateArtifact(artifact: Models.IArtifact): any {
        return {};
    }

//    private properties: Models.IPropertyType[];
    public loadView(artifact: Models.IArtifact) {
        if (!artifact) {
            return;
        } 
        this.activeTab = -1;
        this._artifact = angular.copy(artifact);
        this.model = this.createModel(this._artifact);
        this.fields = this.projectManager.getArtifactPropertyFileds(this._artifact);
        this.tabs = this.fields.noteFields.map((it: AngularFormly.IFieldConfigurationObject, index: number) => {
            let tab = <IFieldTab>{
                title: it.templateOptions.label,
                index: index,
                fields: [it],
            };
            delete it.templateOptions.label;
            return tab;
        });
        
        this.activeTab = 0;

    }

}
