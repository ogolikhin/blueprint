import { Models, Enums } from "../../../main/models";
import { IStatefulArtifact, IArtifactPropertyValues } from "../interfaces";


export class CustomProperties implements IArtifactPropertyValues  {
    
    private properties: Models.IPropertyValue[];
    private stateArtifact: IStatefulArtifact;
    private subject: Rx.BehaviorSubject<Models.IPropertyValue>;
    private observableSubject: Rx.Observable<Models.IPropertyValue>;

    constructor(artifactState: IStatefulArtifact, properties?: Models.IPropertyValue[]) {
        this.stateArtifact = artifactState;
        this.properties = properties || [];
        this.observableSubject  = Rx.Observable.fromArray<Models.IPropertyValue>(this.properties);
        this.subject = new Rx.BehaviorSubject<Models.IPropertyValue>(null);
        this.observableSubject.subscribeOnNext((it:Models.IPropertyValue) => {
            this.addChangeSet(it);

        })
    }

    public initialize(artifact: Models.IArtifact): IArtifactPropertyValues {
        if (artifact) {
            this.properties = artifact.customPropertyValues;
        }
        return this;
    }

    public get value(): ng.IPromise<Models.IPropertyValue[]> {
            // try to get custom property through a service
            return {} as ng.IPromise<Models.IPropertyValue[]>;
    }    


    public get(id: number): Models.IPropertyValue {
        return this.properties.filter((it: Models.IPropertyValue) => it.propertyTypeId === id)[0];
    }


    public get observable(): Rx.IObservable<Models.IPropertyValue> {
        
        return this.subject.filter(it => it !== null).asObservable();
    }    


    public update(id: number, value: any): ng.IPromise<Models.IPropertyValue> {
        let deferred = this.stateArtifact.manager.$q.defer<Models.IPropertyValue>();

        let property = this.get(id);
        if (property) {
            property.value = value;
        }
        this.stateArtifact.lock();
        deferred.resolve(property);
        return deferred.promise;

    }

    private addChangeSet(property: Models.IPropertyValue) {

    }

}