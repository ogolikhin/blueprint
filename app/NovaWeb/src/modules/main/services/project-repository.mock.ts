import {IProjectRepository, Models} from "./project-repository"

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

    private createArtifact(projectId: number, artifactId: number) {
        return {
            id: artifactId,
            name: `Artifact ${artifactId}`,
            typeId: Math.floor(Math.random() * 100),
            parentId: 0,
            predefinedType: Math.floor(Math.random() * 100),
            projectId: projectId,
            hasChildren: false
        } as Models.IArtifact;
    }

    public getArtifacts(id?: number, artifactId?: number): ng.IPromise<Models.IArtifact[]> {

        var deferred = this.$q.defer<Models.IArtifact[]>();
        let items: any;
        if (!id && !artifactId) {
            items = null;
        } else {
            items = ([0, 1, 2, 3, 4]).map(function (it) {
                return this.createArtifact(id, (artifactId || id) * 100 + it);
            }.bind(this)) as Models.IArtifact[];
        }
        deferred.resolve(items);
        return deferred.promise;
    }
}

