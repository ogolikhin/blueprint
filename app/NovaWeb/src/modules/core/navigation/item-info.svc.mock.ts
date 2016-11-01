import * as angular from "angular";
import {IItemInfoService, IItemInfoResult} from "./item-info.svc";

export class ItemInfoServiceMock implements IItemInfoService {
    public static $inject: [string] = ["$q"];
    constructor(private $q: ng.IQService) {}

    public get(id: number, timeout?: ng.IPromise<void>): ng.IPromise<IItemInfoResult> {
        const result = <IItemInfoResult>{
            id: 360,
            name: "Actor X",
            projectId: 259,
            parentId: 259,
            itemTypeId: 132,
            prefix: "AC",
            predefinedType: 4104,
            versionCount: 13,
            isDeleted: false,
            hasChanges: false,
            orderIndex: 4.6875,
            permissions: 8159
        };

        return this.$q.resolve(result);
    }

    public isProject(item: IItemInfoResult): boolean {
        return false;
    }

    public isArtifact(item: IItemInfoResult): boolean {
        return false;
    }

    public isSubArtifact(item: IItemInfoResult): boolean {
        return false;
    }
}
