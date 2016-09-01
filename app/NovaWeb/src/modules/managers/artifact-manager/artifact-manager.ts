import { IArtifactManager, IStatefulArtifact } from "./interfaces";
import { StatefullArtifact  } from "./artifact";
import { Models, Enums } from "../../main/models";


export class ArtifactManager  implements IArtifactManager {

    public static $inject = [
        "$http", 
        "$q"
    ];

    private artifactList: IStatefulArtifact[];

    constructor(
        public $http: ng.IHttpService, 
        public $q: ng.IQService) {

        this.artifactList = [];
    }

    public list(): IStatefulArtifact[] {
        return this.artifactList;
    }

    public get(id: number): IStatefulArtifact {
        const foundArtifacts = this.artifactList.filter((artifact: IStatefulArtifact) => 
            artifact.id === id);

        return foundArtifacts.length ? foundArtifacts[0] : null;
    }
    
    public add(artifact: Models.IArtifact) {
        this.artifactList.push(new StatefullArtifact(this, artifact));
    }

    public remove(id: number) {
        this.artifactList = this.artifactList.filter((artifact: IStatefulArtifact) => 
            artifact.id !== id);
    }

    public update(id: number) {
        // TODO: 
    }

    public load<T>(request: ng.IRequestConfig): ng.IPromise<T> {
        var defer = this.$q.defer<T>();
        this.$http(request).then(
            (result: ng.IHttpPromiseCallbackArg<T>) => defer.resolve(result.data),
            (errResult: ng.IHttpPromiseCallbackArg<any>) => {
                if (!errResult) {
                    defer.reject();
                    return;
                }
                var error = {
                    statusCode: errResult.status,
                    message: (errResult.data ? errResult.data.message : "")
                };
                defer.reject(error);
            }
        );
        return defer.promise;

    }

}
