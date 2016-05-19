﻿import {IProjectRepository, Models} from "./project-repository";

export class ProjectRepositoryMock implements IProjectRepository {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) { }

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

