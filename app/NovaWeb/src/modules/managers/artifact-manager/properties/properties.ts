import { Models } from "../../../main/models";
import { IIStatefulItem, IArtifactProperties } from "../../models";
import {
    ChangeTypeEnum, 
    IChangeCollector, 
    IChangeSet,
    ChangeSetCollector
} from "../";

export class ArtifactProperties implements IArtifactProperties  {
    
    protected properties: Models.IPropertyValue[];
    //private subject: Rx.BehaviorSubject<Models.IPropertyValue>;
    private subject: Rx.Observable<Models.IPropertyValue>;
    private changeset: IChangeCollector;

    constructor(private statefulItem: IIStatefulItem, properties?: Models.IPropertyValue[]) {
        this.properties = properties || [];
        this.changeset = new ChangeSetCollector(statefulItem);
        this.subject  = Rx.Observable.fromArray<Models.IPropertyValue>(this.properties);
//        this.subject = new Rx.BehaviorSubject<Models.IPropertyValue>(null);
        // this.subject.subscribeOnNext((it: Models.IPropertyValue) => {
        //     this.addChangeSet(it);

        // });
    }

    public initialize(properties: Models.IPropertyValue[])  {
        this.properties = properties || [];
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
           let changeset = {
               type: ChangeTypeEnum.Update,
               key: id,
               value: property              
           } as IChangeSet;
           this.changeset.add(changeset);
           
           this.statefulItem.lock();
        }
        return property;
    }

    public discard(all: boolean = false) {
        this.changeset.reset().forEach((it: IChangeSet) => {
            if (!all) {
                this.get(it.key as number).value = it.value;
            }
        });
        
    }

    public changes(): Models.IPropertyValue[] {
        const propertyChanges = new Array<Models.IPropertyValue>();
        const changes = this.changeset.get() || [];
        changes.filter(change => change.type === ChangeTypeEnum.Update)
            .forEach(change => {
                propertyChanges.push(change.value);
            });
        return propertyChanges;
    }
}

export class SpecialProperties extends ArtifactProperties  {

    public get(id: number): Models.IPropertyValue {
        return this.properties.filter((it: Models.IPropertyValue) => it.propertyTypePredefined === id)[0];
    }
}