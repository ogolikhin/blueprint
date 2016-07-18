import * as Models from "../models/models";
import {IProjectRepository} from "./project-repository";
import {ArtifactServiceMock} from "./artifact.svc.mock";

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
                return ProjectRepositoryMock.createArtifact(id, id * 10 + it);
            }) as Models.IArtifact[];
        } else if (id && artifactId) {
            items = ([0, 1, 2, 3, 4]).map(function (it) {
                return ProjectRepositoryMock.createArtifact(id, (artifactId || id) * 100 + it);
            }.bind(this)) as Models.IArtifact[];
        }
        deferred.resolve(items);
        return deferred.promise;
    }


    public getProjectMeta(projectId?: number): ng.IPromise<Models.IProjectMeta> {
        var deferred = this.$q.defer<Models.IProjectMeta>();
        let meta = {} as Models.IProjectMeta;
        deferred.resolve(meta);
        return deferred.promise;
    }

    public static createArtifact(artifactId: number, projectId?: number, children?: number): Models.IArtifact {
        let artifact = new Models.Artifact({
            id: artifactId,
            name: "Artifact " + artifactId,
            projectId: projectId || artifactId,
            itemTypeId: Math.floor(Math.random() * 100),
            itemTypeVersionId: Math.floor(Math.random() * 100),
            predefinedType: Math.floor(Math.random() * 100),
        });
        if (children) {
            this.createDependentArtifacts(artifact, children)
        }
        return artifact;
    }

    public static createDependentArtifacts(artifact: Models.IArtifact, count: number): Models.IArtifact {
        if (!count)
            return;
        artifact.artifacts = artifact.artifacts || [];
        for (var i = 0; i < count; i++) {
            let child = this.createArtifact(artifact.id + 100, artifact.projectId);

            artifact.artifacts.push(child);
        }
        return artifact;
    }


}

