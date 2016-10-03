import { IIStatefulItem } from "../item";
import { Models } from "../../../main/models";
import { ChangeTypeEnum, IChangeCollector, IChangeSet, ChangeSetCollector } from "../changeset";
import { IDispose } from "../../models";

export interface IArtifactProperties extends IDispose {
    initialize(properties: Models.IPropertyValue[]); 
    get(id: number): Models.IPropertyValue;
    set(id: number, value: any): Models.IPropertyValue;
    changes(): Models.IPropertyValue[];
    discard();
    isLoaded: boolean;
}

export class ArtifactProperties implements IArtifactProperties  {
    
    protected properties: Models.IPropertyValue[];
    private changeset: IChangeCollector;

    constructor(private statefulItem: IIStatefulItem, properties?: Models.IPropertyValue[]) {
        this.properties = properties || [];
        this.changeset = new ChangeSetCollector(statefulItem);
        this._isLoaded = false;
//        this.subject = new Rx.BehaviorSubject<Models.IPropertyValue>(null);
        // this.subject.subscribeOnNext((it: Models.IPropertyValue) => {
        //     this.addChangeSet(it);

        // });
    }

    public initialize(properties: Models.IPropertyValue[])  {
        this.properties = properties || [];
        this._isLoaded = true;
    }

    private _isLoaded: boolean;
    public get isLoaded(): boolean {
        return this._isLoaded;
    }

    public dispose() {
        delete this.properties;
        delete this.changeset;
    }

    public get(id: number): Models.IPropertyValue {
        return this.properties.filter((it: Models.IPropertyValue) => it.propertyTypeId === id)[0];
    }


    public set(id: number, value: any): Models.IPropertyValue {
        let property = this.get(id);
        if (property) {
           property.value = value;
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

    public discard() {
        this.changeset.reset();
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