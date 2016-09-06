import { IProjectManager } from "./project-manager";
import { Models } from "./project-repository";


export class ProjectManagerMock implements IProjectManager {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) { }

    public initialize = () => { };
    public dispose = () => { };

    private _projectCollection: Rx.BehaviorSubject<Models.IProject[]>;

    public get projectCollection(): Rx.BehaviorSubject<Models.IProject[]> {
        return this._projectCollection || (this._projectCollection = new Rx.BehaviorSubject<Models.IProject[]>([]));
    }

    public loadProject = (project: Models.IProject) => { };
    public loadArtifact = (project: Models.IArtifact) => { };
    public loadFolders = (id?: number) => {
        var deferred = this.$q.defer<Models.IProjectNode[]>();
        let items = [{ id: 1, name: "test", type: 1, parentFolderId: 0, hasChildren: false }];
        deferred.resolve(items);
        return deferred.promise;
    };
    public closeProject = (all?: boolean) => { };
    public getProject = (id: number) => { return null; };
    public getArtifact = (artifactId: number, project?: Models.IArtifact) => {
        let artifact: Models.IArtifact = { hasChildren: true, id: 1 };
        return artifact;
    };
    public getSubArtifact = (artifact: number | Models.IArtifact, subArtifactId: number) => { return null; };
    public getArtifactType = (artifact: number | Models.IArtifact, project?: number | Models.IProject) => { return null; };
    public getArtifactPropertyTypes = (artifact: number | Models.IArtifact, subArtifact: Models.ISubArtifact): Models.IPropertyType[] => {
        var result: Models.IPropertyType[] = [{id: 1}];
        return result;
    };
    public getSubArtifactPropertyTypes = (subArtifact: number | Models.IArtifact) => { return null; };
    public getPropertyTypes = (project: number, propertyTypeId: number) => { return null; };

    public updateArtifactName(artifact: Models.IArtifact) {}
}