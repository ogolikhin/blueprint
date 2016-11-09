import {Models, Enums, IArtifactService} from "./artifact.svc";

export class ArtifactServiceMock implements IArtifactService {

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService) {
    }

    public updateArtifact(artifact: Models.IArtifact) {
        const defer = this.$q.defer<Models.IArtifact>();
        defer.resolve(artifact);
        return defer.promise;
    }

    public static createNewArtifact(name: string, projectId: number, parentId: number, itemTypeId: number, orderIndex?: number): any {
        const now = new Date();

        return new Models.Artifact({
            projectId: projectId,
            version: -1,
            createdOn: null,
            createdBy: {
                id: 1,
                displayName: "Default Instance Admin"
            },
            lastEditedOn: null,
            lastEditedBy: null,
            lastSavedOn: now.toISOString(),
            permissions: 8159,
            id: Math.round(Math.random() * 1000),
            name: name,
            description: "",
            parentId: parentId,
            orderIndex: orderIndex ? orderIndex : 1,
            itemTypeId: itemTypeId,
            itemTypeName: "Mock artifact",
            itemTypeVersionId: 1,
            itemTypeIconId: 1,
            prefix: "MA",
            customPropertyValues: [],
            specificPropertyValues: [],
            predefinedType: 4102
        });
    }

    public static createArtifact(id: number, properties?: number): any {
        return new Models.Artifact({
            id: id,
            name: "Artifact " + id,
            parentId: id,
            itemTypeId: id,
            itemTypeVersionId: id,
            orderIndex: 1,
            version: 1,
            permissions: 0,
            lockedByUserId: null,
            customPropertyValues: this.createPropertyValues(id, properties)
        });
    }

    public static createLightArtifact(id: number, properties?: number): any {
        return new Models.Artifact({
            id: id,
            version: 1,
            lastSavedOn: "20160831T16:00:00"
        });
    }

    public static createSystemProperty(artifact: Models.IArtifact) {
        const result: Models.IPropertyValue[] = [];
        let id: number = Math.floor(Math.random() * 100);
        result.push({
            propertyTypeId: id,
            propertyTypeVersionId: 1,
            propertyTypePredefined: Models.PropertyTypePredefined.CreatedBy,
            value: "Creator"
        });
        result.push({
            propertyTypeId: id + 1,
            propertyTypeVersionId: 1,
            propertyTypePredefined: Models.PropertyTypePredefined.CreatedOn,
            value: new Date()
        });
        result.push({
            propertyTypeId: id + 2,
            propertyTypeVersionId: 1,
            propertyTypePredefined: Models.PropertyTypePredefined.LastEditedBy,
            value: "Editor"
        });
        result.push({
            propertyTypeId: id + 3,
            propertyTypeVersionId: 1,
            propertyTypePredefined: Models.PropertyTypePredefined.LastEditedOn,
            value: new Date()
        });
        artifact.customPropertyValues = (artifact.customPropertyValues || []).concat(result);

    }

    public static createLockResult(id: number): Models.ILockResult[] {
        let data = {
            result: Enums.LockResultEnum.Success,
            info: {
                versionId: 0
            }

        } as Models.ILockResult;
        return [data];
    }


    public static createPropertyValues(id: number, count?: number): any[] {

        const result: Models.IPropertyValue[] = [];
        for (let i = 0; i < (count || 0); i++) {
            result.push({
                propertyTypeId: id + i,
                propertyTypeVersionId: id * 10,
                propertyTypePredefined: id / 5,
                value: "Property " + id
            });
        }
        return result;
    }

    public static createSpecificPropertyValue(versionId: number, value: any, typePredefined: Models.PropertyTypePredefined): Models.IPropertyValue {

        const result: Models.IPropertyValue = {
            propertyTypeId: typePredefined,
            propertyTypeVersionId: versionId,
            propertyTypePredefined: typePredefined,
            value: value
        };
        return result;
    }

    public static createSubArtifacts(id: number, count?: number): any[] {
        const result: Models.ISubArtifact[] = [];
        for (let i = 0; i < (count || 0); i++) {
            result.push({
                id: id + 1000,
                name: "SubArtifact",
                parentId: id,
                itemTypeId: id + 10000,
                itemTypeVersionId: id + 1000,
                customPropertyValues: this.createPropertyValues(id, count),
                traces: []
            });
        }
        return result;
    }

    public getArtifact(artifactId: number): ng.IPromise<Models.IArtifact> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(ArtifactServiceMock.createArtifact(artifactId));
        return deferred.promise;
    }


    public create(name: string, projectId: number, parentId: number, itemTypeId: number, orderIndex?: number): ng.IPromise<Models.IArtifact> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(ArtifactServiceMock.createNewArtifact(name, projectId, parentId, itemTypeId, orderIndex));
        return deferred.promise;
    }


    public getSubArtifact(artifactId: number, subArtifactId: number): ng.IPromise<Models.ISubArtifact> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(ArtifactServiceMock.createSubArtifacts(artifactId));
        return deferred.promise;
    }

    public lock(artifactId: number): ng.IPromise<Models.ILockResult[]> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(ArtifactServiceMock.createLockResult(artifactId));
        return deferred.promise;
    }



     public getArtifactModel<T extends Models.IArtifact>(url: string, id: number, versionId?: number, timeout?: ng.IPromise<any>): ng.IPromise<T> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(ArtifactServiceMock.createArtifact(id));
        return deferred.promise;
     }

     public static createChildren(artifactId: number, count: number): Models.IArtifact[] {
        const result: Models.IArtifact[] = [];
        for (let i = 1; i <= count; i++) {
            result.push(ArtifactServiceMock.createArtifact(artifactId + i));
        }
        return result;
     }

    public getChilden(projectId: number, artifactId?: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact[]> {
        const deferred = this.$q.defer<Models.IArtifact[]>();
        const result = ArtifactServiceMock.createChildren(artifactId, 5);
        deferred.resolve(result);
        return deferred.promise;
    }

    public deleteArtifact(artifactId: number, timeout?: ng.IPromise<any>): ng.IPromise<Models.IArtifact[]> {
        const deferred = this.$q.defer<Models.IArtifact[]>();
        let result: Models.IArtifact[] = [];

        if (artifactId === 200) {
            result = ArtifactServiceMock.createChildren(artifactId, 5);
        } else {
            result = [ArtifactServiceMock.createArtifact(artifactId, 2)];
        }
        deferred.resolve(result);
        return deferred.promise;
        
    }
     public getArtifactNavigationPath(artifactId: number): ng.IPromise<Models.IArtifact[]> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(ArtifactServiceMock.createArtifact(artifactId));
        return deferred.promise;
     }

}
