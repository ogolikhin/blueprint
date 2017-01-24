import {IUserGroup} from "../../main/models/models";

export interface IItemInfoService {
    get(id: number): ng.IPromise<IItemInfoResult>;
    isProject(item: IItemInfoResult): boolean;
    isSubArtifact(item: IItemInfoResult): boolean;
    isArtifact(item: IItemInfoResult): boolean;
}

export interface IItemInfoResult {
    id: number;
    subArtifactId?: number;
    name: string;
    projectId: number;
    parentId: number;
    itemTypeId: number;
    prefix: string;
    predefinedType: number;
    version: number;
    versionCount: number;
    isDeleted: boolean;
    hasChanges: boolean;
    orderIndex: number;
    permissions: number;
    lockedByUser: IUserGroup;
    lockedDateTime: Date;
    deletedByUser: IUserGroup;
    deletedDateTime: Date;
}

export class ItemInfoService implements IItemInfoService {
    public static $inject: [string] = [
        "$q",
        "$http"
    ];

    constructor(private $q: ng.IQService,
                private $http: ng.IHttpService) {
    }

    public get(id: number, timeout?: ng.IPromise<void>): ng.IPromise<IItemInfoResult> {
        const defer = this.$q.defer<IItemInfoResult>();
        const request: ng.IRequestConfig = {
            url: `/svc/artifactstore/artifacts/versionControlInfo/${id}`,
            method: "GET",
            timeout: timeout
        };

        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<IItemInfoResult>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                const error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "")
                };
                defer.reject(error);
            }
        );

        return defer.promise;
    }

    public isProject(item: IItemInfoResult): boolean {
        return !item.subArtifactId && item.id === item.projectId;
    }

    public isArtifact(item: IItemInfoResult): boolean {
        return !item.subArtifactId && item.id !== item.projectId;
    }

    public isSubArtifact(item: IItemInfoResult): boolean {
        return !!item.subArtifactId;
    }
}
