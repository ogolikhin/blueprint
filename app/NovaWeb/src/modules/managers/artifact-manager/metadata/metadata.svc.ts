import { ILocalizationService } from "../../../core";
import { Models } from "../../../main/models";

export interface IMetaDataService {
    getArtifactItemType(projectId, itemTypeId: number): Models.IItemType;
    get(projectId?: number);
    add(projectId?: number);
    remove(projectId?: number);
}


class ProjectMetaData {
    constructor(public id: number, public data: Models.IProjectMeta) {

    }
}


export class MetaDataService implements IMetaDataService {

    private collection: ProjectMetaData[] = [];
    
    public static $inject = [
        "$q",
        "$http",
        "localization"
    ];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private localization: ILocalizationService) {
    }

    private load(projectId?: number): ng.IPromise<ProjectMetaData> {
        var defer = this.$q.defer<ProjectMetaData>();

        let url: string = `svc/artifactstore/projects/${projectId}/meta/customtypes`;

        this.$http.get<Models.IProjectMeta>(url).then(
            (result: ng.IHttpPromiseCallbackArg<Models.IProjectMeta>) => {
                if (angular.isArray(result.data.artifactTypes)) {
                    
                    //add specific types 
                    result.data.artifactTypes.unshift(
                        <Models.IItemType>{
                            id: -1,
                            name: this.localization.get("Label_Project"),
                            predefinedType: Models.ItemTypePredefined.Project,
                            customPropertyTypeIds: []
                        },
                        <Models.IItemType>{
                            id: -2,
                            name: this.localization.get("Label_Collections"),
                            predefinedType: Models.ItemTypePredefined.CollectionFolder,
                            customPropertyTypeIds: []
                        }
                    );
                }
                
                defer.resolve(new ProjectMetaData(projectId, result.data));
            },
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                var error = {
                    statusCode: errResult.status,
                    message: "Project_NotFound"
                };
                defer.reject(error);
            }
        );
        return defer.promise;
    }

    public get(projectId: number): ProjectMetaData {
        return this.collection.filter((it: ProjectMetaData) => it.id === projectId)[0];
    }    

    public add(projectId: number) {
        this.load(projectId).then((projectMeta: ProjectMetaData) => {
            let metadata = this.get(projectId);
            if (metadata) {
                metadata.data = projectMeta.data;
            } else {
                this.collection.push(projectMeta); 
            }
        }).catch((error: any) => {
//            this.messageService.addError(error);
        });

    }
    public remove(id: number) {
        this.collection = this.collection.filter((it: ProjectMetaData) => {
            return it.id !== id;
        });
    }
    

    public getArtifactItemType(projectId, itemTypeId: number): Models.IItemType {
        let itemType: Models.IItemType;
        let metadata = this.get(projectId);
        if (metadata) {
            itemType = metadata.data.artifactTypes.filter((it: Models.IItemType) => {
                return it.id === itemTypeId;
            })[0];
        } 
        
        return itemType;
    }
}
