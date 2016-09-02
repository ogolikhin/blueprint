import { Models, Enums } from "../../../main/models";
import { IStatefulArtifact, IArtifactProperties } from "../interfaces";
import { ChangeSet, ChangeTypeEnum } from "../changeset";



export class CustomProperties implements IArtifactProperties  {
    
    private properties: Models.IPropertyValue[];
    private stateArtifact: IStatefulArtifact;
    //private subject: Rx.BehaviorSubject<Models.IPropertyValue>;
    private subject: Rx.Observable<Models.IPropertyValue>;
    private changeset: ChangeSet;

    constructor(artifactState: IStatefulArtifact, properties?: Models.IPropertyValue[]) {
        this.stateArtifact = artifactState;
        this.properties = properties || [];
        this.subject  = Rx.Observable.fromArray<Models.IPropertyValue>(this.properties);
//        this.subject = new Rx.BehaviorSubject<Models.IPropertyValue>(null);
        // this.subject.subscribeOnNext((it: Models.IPropertyValue) => {
        //     this.addChangeSet(it);

        // });
    }

    public initialize(artifact: Models.IArtifact): IArtifactProperties {
        if (artifact) {
            this.properties = artifact.customPropertyValues;
        }
        return this;
    }

    // public get value(): ng.IPromise<Models.IPropertyValue[]> {
    //         // try to get custom property through a service
    //         return {} as ng.IPromise<Models.IPropertyValue[]>;
    // }    

    public get observable(): Rx.Observable<Models.IPropertyValue> {
        return this.subject.filter(it => it !== null).asObservable();
    }    


    public get(id: number): Models.IPropertyValue {
        return this.properties.filter((it: Models.IPropertyValue) => it.propertyTypeId === id)[0];
    }


    public set(id: number, value: any): Models.IPropertyValue {
        let property = this.get(id);
        if (property) {
            property.value = value;
            this.changeset.add(ChangeTypeEnum.Update, name, value);
            this.stateArtifact.lock();
        }
        return property;
    }

    public discard() {

        
    }
}