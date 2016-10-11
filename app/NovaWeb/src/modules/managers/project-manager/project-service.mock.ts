import {Models} from "../../main/models";
import {IProjectService} from "./project-service";

export class ProjectServiceMock implements IProjectService {

    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public abort(): void {
    }

    public getFolders(id?: number): ng.IPromise<any[]> {
        var deferred = this.$q.defer<any[]>();

        var folders = [
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

        var deferred = this.$q.defer<Models.IArtifact[]>();
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

    public getProject(id?: number): ng.IPromise<Models.IProjectNode> {

        var deferred = this.$q.defer<Models.IProjectNode>();
        let item: Models.IProjectNode = {id: 1, name: "test", type: 1, parentFolderId: 0, hasChildren: false};
        deferred.resolve(item);
        return deferred.promise;
    }


    public getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta> {
        var deferred = this.$q.defer<Models.IProjectMeta>();
        let meta = {} as Models.IProjectMeta;
        deferred.resolve(meta);
        return deferred.promise;
    }

    public getSubArtifactTree(artifactId: number): ng.IPromise<Models.ISubArtifactNode[]> {
        var deferred = this.$q.defer<Models.ISubArtifactNode[]>();
        let tree = [] as Models.ISubArtifactNode[];
        deferred.resolve(tree);
        return deferred.promise;
    }

    public getProjectTree(projectId: number, artifactId: number, loadChildren?: boolean): ng.IPromise<Models.IArtifact[]> {
        var deferred = this.$q.defer<Models.IArtifact[]>();
        let tree = [] as Models.IArtifact[];
        deferred.resolve(tree);
        return deferred.promise;
    }

    public searchProjects(query: string): ng.IPromise<Models.IProjectNode[]> {
        var deferred = this.$q.defer<Models.IArtifact[]>();
        let result = [] as Models.IProjectNode[];
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
            predefinedType: Math.floor(Math.random() * 100),
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
        for (var i = 0; i < count; i++) {
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
        var result: Models.IPropertyType[] = [];
        for (var i = 0; i < (count || 0); i++) {
            result.push({
                id: id + 1000,
                name: "Item Type " + id,
                versionId: 1,
                //primitiveType?: PrimitiveType;
                //instancePropertyTypeId?: number;
                //isRichText?: boolean;
                //decimalDefaultValue?: number;
                //dateDefaultValue?: Date;
                //userGroupDefaultValue?: any[];
                //stringDefaultValue?: string;
                //decimalPlaces?: number;
                //maxNumber?: number;
                //minNumber?: number;
                //maxDate?: Date;
                //minDate?: Date;
                //isMultipleAllowed?: boolean;
                //isRequired?: boolean;
                //isValidated?: boolean;
                //validValues?: IOption[];
                //defaultValidValueId?: number;
            });
        }
        return result;

    }

    public static populateItemTypes(id: number, count?: number) {
        var result: Models.IItemType[] = [];
        for (var i = 0; i < (count || 0); i++) {
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

