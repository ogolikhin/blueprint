import {Models, AdminStoreModels, SearchServiceModels} from "../../main/models";
import {IProjectService} from "./project-service";

export class ProjectServiceMock implements IProjectService {

    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public abort(): void {
        return;
    }

    public getFolders(id?: number): ng.IPromise<any[]> {
        const deferred = this.$q.defer<any[]>();

        let folders = [
            {
                "id": 3,
                "parentFolderId": 1,
                "name": "Folder with content",
                "type": "Folder"
            },
            {
                "id": 7,
                "parentFolderId": 1,
                "name": "Empty folder",
                "type": "Folder"
            },
            {
                "id": 8,
                "parentFolderId": 1,
                "name": "<button onclick=\"alert('Hey!')\">Embedded HTML in name</button>",
                "type": "Folder"
            },
            {
                "id": 33,
                "parentFolderId": 1,
                "name": "Process",
                "description": "Process description",
                "type": "Project"
            }
        ];
        if (id || id < 0) {
            folders = null;
        }

        deferred.resolve(folders);
        return deferred.promise;
    }


    public getArtifacts(id?: number, artifactId?: number): ng.IPromise<Models.IArtifact[]> {
        const deferred = this.$q.defer<Models.IArtifact[]>();
        let items: Models.IArtifact[];
        if (!id && !artifactId) {
            items = null;
        } else if (id && !artifactId) {
            items = ([0, 1, 2]).map((it) => {
                return ProjectServiceMock.createArtifact(id * 10 + it, id);
            }) as Models.IArtifact[];
        } else if (id && artifactId) {
            items = ([0, 1, 2, 3, 4]).map(function (it) {
                return ProjectServiceMock.createArtifact((artifactId || id) * 100 + it, id);
            }.bind(this)) as Models.IArtifact[];
        }
        deferred.resolve(items);
        return deferred.promise;
    }

    public getProject(id?: number): ng.IPromise<AdminStoreModels.IInstanceItem> {
        const deferred = this.$q.defer<AdminStoreModels.IInstanceItem>();
        const item: AdminStoreModels.IInstanceItem = {id: 1, name: "test", type: 1, parentFolderId: 0, hasChildren: false};
        deferred.resolve(item);
        return deferred.promise;
    }


    public getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta> {
        const deferred = this.$q.defer<Models.IProjectMeta>();
        const meta = {} as Models.IProjectMeta;
        deferred.resolve(meta);
        return deferred.promise;
    }

    public getSubArtifactTree(artifactId: number): ng.IPromise<Models.ISubArtifactNode[]> {
        const deferred = this.$q.defer<Models.ISubArtifactNode[]>();
        const tree = [] as Models.ISubArtifactNode[];
        deferred.resolve(tree);
        return deferred.promise;
    }

    public getProjectTree(projectId: number, artifactId: number, loadChildren?: boolean): ng.IPromise<Models.IArtifact[]> {
        const deferred = this.$q.defer<Models.IArtifact[]>();
        const tree = [] as Models.IArtifact[];
        deferred.resolve(tree);
        return deferred.promise;
    }

    public searchProjects(searchCriteria: SearchServiceModels.ISearchCriteria,
                          resultCount?: number,
                          separatorString?: string): ng.IPromise<SearchServiceModels.IProjectSearchResultSet> {
        const deferred = this.$q.defer<SearchServiceModels.IProjectSearchResultSet>();
        const result = {} as SearchServiceModels.IProjectSearchResultSet;
        deferred.resolve(result);
        return deferred.promise;
    }

    public searchItemNames(searchCriteria: SearchServiceModels.IItemNameSearchCriteria,
                           startOffset: number = 0,
                           pageSize: number = 100): ng.IPromise<SearchServiceModels.IItemNameSearchResultSet> {
        const deferred = this.$q.defer<SearchServiceModels.IItemNameSearchResultSet>();
        const result = {} as SearchServiceModels.IItemNameSearchResultSet;
        deferred.resolve(result);
        return deferred.promise;
    }

    public static createArtifact(artifactId: number, projectId?: number, children?: number): Models.IArtifact {
        let artifact = {
            id: artifactId,
            parentId: 1,
            name: "Artifact " + artifactId,
            projectId: projectId || artifactId,
            itemTypeId: Math.floor(Math.random() * 100),
            itemTypeVersionId: Math.floor(Math.random() * 100),
            predefinedType: Math.floor(Math.random() * 100)
        } as Models.IArtifact;
        if (children) {
            this.createDependentArtifacts(artifact, children);
        }
        return artifact;
    }

    public static createDependentArtifacts(artifact: Models.IArtifact, count: number): Models.IArtifact {
        if (!count) {
            return;
        }
        artifact.children = artifact.children || [];
        for (let i = 0; i < count; i++) {
            let child = this.createArtifact(artifact.id + 100, artifact.projectId);

            artifact.children.push(child);
        }
        return artifact;
    }

    public static populateMetaData(): Models.IProjectMeta {
        let meta: Models.IProjectMeta = {
            artifactTypes: ProjectServiceMock.populateItemTypes(10, 3),
            propertyTypes: ProjectServiceMock.populatePropertyTypes(100, 3),
            subArtifactTypes: ProjectServiceMock.populateItemTypes(1000, 3)
        };

        return meta;
    }

    public static populatePropertyTypes(id: number, count?: number) {
        const result: Models.IPropertyType[] = [];
        for (let i = 0; i < (count || 0); i++) {
            result.push({
                id: id + 1000,
                name: "Item Type " + id,
                versionId: 1
            });
        }
        return result;
    }

    public static populateItemTypes(id: number, count?: number) {
        const result: Models.IItemType[] = [];
        for (let i = 0; i < (count || 0); i++) {
            result.push({
                id: id + 1000,
                name: "Item Type " + id,
                projectId: 1,
                versionId: 1,
                prefix: "it_",
                predefinedType: 1,
                iconImageId: 1,
                usedInThisProject: true,
                customPropertyTypeIds: [1, 2, 3]
            });
        }
        ;
        return result;
    }
}
