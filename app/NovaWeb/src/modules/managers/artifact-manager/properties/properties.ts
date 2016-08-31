import { Models, Enums } from "../../../main/models";
import { IBlock, IStatefulArtifact, IProperty, IArtifactProperties, ISystemProperty } from "../interfaces";

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
    private _system: SystemProperty;
    private subject: Rx.BehaviorSubject<IProperty[]>;
    private state: IStatefulArtifact;

    constructor(artifactState: IStatefulArtifact, system:SystemProperty, custom?: IProperty[], special?: IProperty[]) {
        this.state = artifactState;
        this._system = system || {} as SystemProperty;
        this._custom = custom || [];
        this._special = special || [];
        this.subject = new Rx.BehaviorSubject<IProperty[]>([]);
    }

    public get name(): string {
        return this._system.name;
    }
    public get value(): ng.IPromise<IProperty[]> {
  
        return {} as ng.IPromise<IProperty[]>;
    }    

    public get observable(): Rx.IObservable<IProperty[]> {

        return this.subject.filter(it => it !== null).asObservable();
    }    

    public system(): SystemProperty {
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
        //this.state.manager..lockArtifact();
        return {} as ng.IPromise<IProperty[]>;

    }

    public remove(property: IProperty | IProperty[]): ng.IPromise<IProperty[]> {
        return {} as ng.IPromise<IProperty[]>;
    }

}