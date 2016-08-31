import { Models, Enums } from "../../../main/models";
import { IStatefulArtifact, IProperty, IArtifactProperties, ISystemProperty } from "../interfaces";

class SystemProperty {
    public id: number;
    public name: string;
    public description: string;
    public prefix: string;
    public parentId: number;
    public itemTypeId: number;
    public itemTypeVersionId: number;
    public version: number;
    public createdOn: Date; 
    public lastEditedOn: Date;
    public createdBy: Models.IUserGroup;
    public lastEditedBy: Models.IUserGroup;

}

export class ArtifactProperties implements IArtifactProperties {
    
    private _custom: IProperty[];
    private _special: IProperty[];
    private _system: ISystemProperty;
    private subject: Rx.BehaviorSubject<IProperty[]>;
    private state: IStatefulArtifact;

    constructor(artifactState: IStatefulArtifact, artifact: Models.IArtifact) {
        this.state = artifactState;
        this.load(artifact);
        this.subject = new Rx.BehaviorSubject<IProperty[]>([]);
    }

    private load(artifact: Models.IArtifact) {
        this._system = {} as SystemProperty;
        this._custom = [];
        this._special = [];
        for(let key in artifact) {
            if (key === "customPropertyValues") {
                this._custom.concat(
                    artifact.customPropertyValues.map((it: Models.IPropertyValue) => {
                        return angular.extend({}, it, {
                            propertyLookup: Enums.PropertyLookupEnum.Custom
                        } );
                    })
                )
            } else if (key === "specificPropertyValues") {
                this._special.concat(
                    artifact.specificPropertyValues.map((it: Models.IPropertyValue) => {
                        return angular.extend({}, it, {
                            propertyLookup: Enums.PropertyLookupEnum.Special
                        });
                    })
                )
            } else {
                angular.extend(this._system, {
                    key : artifact[key]
                }); 
            }

        }
        
    }




    public get value(): ng.IPromise<IProperty[]> {
  
        return {} as ng.IPromise<IProperty[]>;
    }    

    public get observable(): Rx.IObservable<IProperty[]> {

        return this.subject.filter(it => it !== null).asObservable();
    }    

    public system(): ISystemProperty {
        return this._system; 
    }

    public custom(id: number): IProperty {
        return this._custom.filter((it: IProperty) => it.propertyTypeId === id )[0];
    }

    public special(id): IProperty {
        return this._special.filter((it: IProperty) => it.propertyTypeId === id )[0];
    }
    
    public add(properties:IProperty[]): ng.IPromise<IProperty[]> {
        return null;
    }    

    public update(property: IProperty | IProperty[]): ng.IPromise<IProperty[]> {
        //this.state.manager.lockArtifact();
        return {} as ng.IPromise<IProperty[]>;

    }

    public remove(property: IProperty | IProperty[]): ng.IPromise<IProperty[]> {
        return {} as ng.IPromise<IProperty[]>;
    }

}