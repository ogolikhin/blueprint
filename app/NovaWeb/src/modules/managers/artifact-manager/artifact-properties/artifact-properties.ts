import {PropertyTypePredefined} from "../../../main/models/enums";
import { Models } from "../../../main/models";
import { IBlock } from "../interfaces";

export enum PropertyLookupEnum {
    None = 0,
    System = 1,
    Custom = 2,
    Special = 3,
}

export interface IProperty {
    propertyLookup: PropertyLookupEnum;
    propertyTypeId: number | string;
    propertyTypeVersionId?: number;
    propertyTypePredefined?: PropertyTypePredefined;
    isReuseReadOnly?: boolean;
    value: any;

}


export interface IArtifactProperties extends IBlock<IProperty>{

}


export class ArtifactProperties implements IArtifactProperties{
    
    private properties: IProperty[];
    private subject: Rx.BehaviorSubject<IProperty[]>;
    private $lock: () => void;//ng.IPromise<Models.ILockResult>;

    constructor(private $q: ng.IQService, lock: () => void) {
        this.$lock = lock;
        this.subject = new Rx.BehaviorSubject<IProperty[]>([]);
    }

    
    public get value(): ng.IPromise<IProperty[]> {
  
        return {} as ng.IPromise<IProperty[]>;
    }    

    public get observable(): Rx.IObservable<IProperty[]> {

        return this.subject.filter(it => it !== null).asObservable();
    }    

    public add(property: IProperty | IProperty[]): ng.IPromise<IProperty[]> {

        return {} as ng.IPromise<IProperty[]>;

    }    

    public update(property: IProperty | IProperty[]): ng.IPromise<IProperty[]> {
        
        this.$lock();
        return {} as ng.IPromise<IProperty[]>;

    }

    public remove(property: IProperty | IProperty[]): ng.IPromise<IProperty[]> {

        return {} as ng.IPromise<IProperty[]>;

    }
}