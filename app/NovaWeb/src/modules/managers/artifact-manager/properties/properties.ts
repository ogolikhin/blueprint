import {IIStatefulItem} from "../item";
import {Models} from "../../../main/models";
import {ChangeTypeEnum, IChangeCollector, IChangeSet, IItemChangeSet, ChangeSetCollector} from "../changeset";
import {IDispose} from "../../models";

export interface IArtifactProperties extends IDispose {
    initialize(properties: Models.IPropertyValue[]);
    list(): Models.IPropertyValue[];
    get(id: number): Models.IPropertyValue;
    set(id: number, value: any): Models.IPropertyValue;
    changes(): Models.IPropertyValue[];
    discard();
    isLoaded: boolean;
}

export class ArtifactProperties implements IArtifactProperties {

    protected properties: Models.IPropertyValue[];
    private changeset: IChangeCollector;

    //TODO: Remove properties in constructor, not getting used anywhere.
    constructor(private statefulItem: IIStatefulItem, properties?: Models.IPropertyValue[]) {
        this.properties = properties || [];
        this.changeset = new ChangeSetCollector(statefulItem);
        this._isLoaded = false;
    }
    
    public list() {
        return this.properties;
    }

    public initialize(properties: Models.IPropertyValue[]) {
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
            this.statefulItem.propertyChange.onNext({item: this.statefulItem, change: changeset} as IItemChangeSet);
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

export class SpecialProperties extends ArtifactProperties {

    public get(id: number): Models.IPropertyValue {
        return this.properties.filter((it: Models.IPropertyValue) => it.propertyTypePredefined === id)[0];
    }
}
