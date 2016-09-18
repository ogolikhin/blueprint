// import {Models, Enums, IArtifactService} from "../";

// export class ArtifactServiceMock implements IArtifactService {

//     public static $inject = ["$http", "$q"];

//     constructor(private $http: ng.IHttpService, private $q: ng.IQService) {
//     }

//     public updateArtifact(artifact: Models.IArtifact) {}

//     public static createArtifact(id: number, properties?: number): any {
//         return new Models.Artifact({
//             id: id,
//             name: "Artifact " + id,
//             parentId: id,
//             itemTypeId: id,
//             itemTypeVersionId: id,
//             orderIndex: 1,
//             version: 1,
//             permissions: 0,
//             lockedByUserId: null,
//             customPropertyValues: this.createPropertyValues(id, properties)
//         });
//     }

//     public static createLightArtifact(id: number, properties?: number): any {
//         return new Models.Artifact({
//             id: id,
//             version: 1,
//             lastSavedOn: '20160831T16:00:00'
//         });
//     }

//     public static createSystemProperty(artifact: Models.IArtifact) {
//         var result: Models.IPropertyValue[] = [];
//         let id: number = Math.floor(Math.random() * 100);
//         result.push({
//             propertyTypeId: id,
//             propertyTypeVersionId: 1,
//             propertyTypePredefined: Models.PropertyTypePredefined.CreatedBy,
//             value: "Creator"
//         });       
//         result.push({
//             propertyTypeId: id + 1,
//             propertyTypeVersionId: 1,
//             propertyTypePredefined: Models.PropertyTypePredefined.CreatedOn,
//             value: new Date()
//         });       
//         result.push({
//             propertyTypeId: id + 2,
//             propertyTypeVersionId: 1,
//             propertyTypePredefined: Models.PropertyTypePredefined.LastEditedBy,
//             value: "Editor"
//         });       
//         result.push({
//             propertyTypeId: id + 3,
//             propertyTypeVersionId: 1,
//             propertyTypePredefined: Models.PropertyTypePredefined.LastEditedOn,
//             value: new Date()
//         });
//         artifact.customPropertyValues = (artifact.customPropertyValues || []).concat(result);
    
//     }

//     public static createLockResult(id: number): Models.ILockResult[] {
//         let data = {
//             result : Enums.LockResultEnum.Success

//         } as Models.ILockResult;
//         return [data];
//     }


//     public static createPropertyValues(id: number, count?: number): any[] {

//         var result: Models.IPropertyValue[] = [];
//         for (var i = 0; i < (count || 0); i++) {
//             result.push({
//                 propertyTypeId: id + i,
//                 propertyTypeVersionId: id * 10,
//                 propertyTypePredefined: id / 5,
//                 value: "Property " + id
//             });
//         }
//         return result;
//     }
//     public static createSubArtifacts(id: number, count?: number): any[] {
//         var result: Models.ISubArtifact[] = [];
//         for (var i = 0; i < (count || 0); i++) {
//             result.push({
//                 id: id + 1000,
//                 name: "SubArtifact",
//                 parentId: id,
//                 itemTypeId: id + 10000,
//                 itemTypeVersionId: id + 1000,
//                 customPropertyValues : this.createPropertyValues(id, count),
//                 traces: []
//             });
//         }
//         return result;
//     }

//     public getArtifact(artifactId: number): ng.IPromise<Models.IArtifact> {
//         var deferred = this.$q.defer<any>();
//         deferred.resolve(ArtifactServiceMock.createArtifact(artifactId));
//         return deferred.promise;
//     }


//     public getSubArtifact(artifactId: number, subArtifactId: number): ng.IPromise<Models.ISubArtifact> {
//         var deferred = this.$q.defer<any>();
//         deferred.resolve(ArtifactServiceMock.createArtifact(artifactId));
//         return deferred.promise;
//     }  
//     public lock(artifactId: number): ng.IPromise<Models.ILockResult[]> {
//         var deferred = this.$q.defer<any>();
//         deferred.resolve(ArtifactServiceMock.createLockResult(artifactId));
//         return deferred.promise;
//     }


//     }
