export enum ChangeTypeEnum {
    Update,
    Add,
    Delete
}
export interface IChangeSet {
    type: ChangeTypeEnum,
    id: string | number;
    value: any;
}

export class ChangeSet {
    private changesets: IChangeSet[];

    constructor() {
        this.reset();
    }

    public add(type: ChangeTypeEnum, key: string | number, value: any) {
        if (!angular.isArray(this.changesets)) {
            this.changesets = [];
        }
                 
        let changeset = this.changesets.filter((it: IChangeSet) => {
            return it.id === key && type === ChangeTypeEnum.Update;
        })[0];

        if (changeset) {
            changeset.value = value;
        } else {
            this.changesets.push({
                type: type,
                id: name,
                value: value
            } as IChangeSet);
        }
    }

    public reset() {
        this.changesets = [];
    }

}