import { Models, Enums } from "../../../main/models";
import { IStatefulArtifact, IArtifactPropertyValues } from "../interfaces";


export class CustomProperties implements IProperIArtifactPropertyValuestyValues {
    
    private properties: Models.IPropertyValue[];
    private state: IStatefulArtifact;
    private subject: Rx.BehaviorSubject<Models.IPropertyValue>;
    private observableSubject: Rx.Observable<Models.IPropertyValue>;

    constructor(artifactState: IStatefulArtifact, properties: Models.IPropertyValue[]) {
        this.state = artifactState;
        this.properties = properties || [];
        this.observableSubject  = Rx.Observable.fromArray<Models.IPropertyValue>(this.properties);
        this.subject = new Rx.BehaviorSubject<Models.IPropertyValue>(null);
        this.observableSubject.subscribeOnNext((it:Models.IPropertyValue) => {
            this.addChangeSet(it);

        })
    }


    public get(id: number): Models.IPropertyValue {
        return this.properties.filter((it: Models.IPropertyValue) => it.propertyTypeId === id)[0];
    }


    public get observable(): Rx.IObservable<Models.IPropertyValue> {
        
        return this.subject.filter(it => it !== null).asObservable();
    }    


    public update(id: number, value: any): ng.IPromise<Models.IPropertyValue> {
        let deferred = this.state.manager.$q.defer<Models.IPropertyValue>();

        let property = this.get(id);
        if (property) {
            property.value = value;
        }
        //this.state.manager.lockArtifact();
        deferred.resolve(property);
        return deferred.promise;

    }

    private addChangeSet(property: Models.IPropertyValue) {

    }

}