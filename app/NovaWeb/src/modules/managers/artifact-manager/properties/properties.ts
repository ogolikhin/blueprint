//import {PropertyTypePredefined} from "../../../main/models/enums";
import { IBlock, IStatefulArtifact,IProperty,IArtifactProperties } from "../interfaces";



export class ArtifactProperties implements IArtifactProperties{
    
    private properties: IProperty[];
    private subject: Rx.BehaviorSubject<IProperty[]>;
    private artifact: IStatefulArtifact;

    constructor(artifactState: IStatefulArtifact) {
        this.artifact = artifactState;
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
        this.artifact.manager.lockArtifact();
        return {} as ng.IPromise<IProperty[]>;

    }

    public remove(property: IProperty | IProperty[]): ng.IPromise<IProperty[]> {

        return {} as ng.IPromise<IProperty[]>;

    }

    public lock(){
        this.artifact.lock();
    } 
}