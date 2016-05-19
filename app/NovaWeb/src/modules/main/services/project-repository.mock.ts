import {IProjectRepository, Models} from "./project-repository";

export class ProjectRepositoryMock implements IProjectRepository {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) { }

    public getFolders(id?: number): ng.IPromise<any[]> {
        var deferred = this.$q.defer<any[]>();
        var folders = [
            {
                "Id": 3,
                "ParentFolderId": 1,
                "Name": "Folder with content",
                "Type": "Folder"
            },
            {
                "Id": 7,
                "ParentFolderId": 1,
                "Name": "Empty folder",
                "Type": "Folder"
            },
            {
                "Id": 8,
                "ParentFolderId": 1,
                "Name": "<button onclick=\"alert('Hey!')\">Embedded HTML in name</button>",
                "Type": "Folder"
            },
            {
                "Id": 33,
                "ParentFolderId": 1,
                "Name": "Process",
                "Description": "Process description",
                "Type": "Project"
            }
        ];
        deferred.resolve(folders);
        return deferred.promise;
    }

    public getProject(id?: number, artifactId?: number): ng.IPromise<Models.IArtifact[]> {
        var deferred = this.$q.defer<Models.IArtifact[]>();
        var items: Models.IArtifact[] = [
            {
                id: artifactId || id,
                name: (artifactId ? `Artifact ${artifactId}` : `Project ${id}`) ,
                typeId: (artifactId ? 1 : 0),
                parentId: 0,
                predefinedType: 1,
                projectId: id,
                version: 1,
                hasChildren: false

            }
        ];
        deferred.resolve(items);
        return deferred.promise;
    }
}

