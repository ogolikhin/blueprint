import "angular";
import {ILocalizationService} from "../../core/localization";


export interface IProjectNode {
    Type: string;
    Id: number;
    ParentFolderId: number;
    Name: string;
    Description?: string;
    Children?: IProjectNode[];
}

export interface IProjectService {
    getFolders(id?: number): ng.IPromise<any[]>;
    getProject(id?: number): ng.IPromise<any[]>;
}

export class ProjectService implements IProjectService {
    static $inject: [string] = ["$q", "$http", "localization"];
    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private localization: ILocalizationService) {
    }


    public getFolders(id?: number): ng.IPromise<any[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<any>(`svc/adminstore/instance/folders/${id || 1}/children`)
            .success((result: IProjectNode[]) => {
                angular.forEach(result, (it) => {
                    if (it.Type === "Folder") {
                        it.Children = new Array<IProjectNode>();
                    }
                });
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.Message : "") || this.localization.get("", "Error")
                };
                defer.reject(error);
            });
        return defer.promise;
    }

    public getProject(id?: number): ng.IPromise<any[]> {
        var defer = this.$q.defer<any>();
        this.$http.get<any>(`svc/adminstore/project/${id}`)
            .success((result: IProjectNode[]) => {
                defer.resolve(result);
            }).error((err: any, statusCode: number) => {
                var error = {
                    statusCode: statusCode,
                    message: (err ? err.Message : "") || this.localization.get("", "Error")
                };
                defer.reject(error);
            });
        return defer.promise;
    }

    public loadProject(id?: number): Project {
        let project: Project = null;
        this.getProject(id).then((data: any) => {
            project = new Project(data);
        }).catch(() => {
        
        })

        return project;
    }
}

interface IProject {
    id: number;
    name: string;
}

export class Project implements IProject {
    public id: number;
    public name: string;

    constructor(data: any) {
        this.id = data.id;
        this.name = data.name;
    }
}
