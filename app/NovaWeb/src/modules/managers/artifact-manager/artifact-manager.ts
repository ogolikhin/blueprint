import { IState, ArtifactState } from "./artifact-state"
import { IArtifactProperties, ArtifactProperties } from "./artifact-properties";
import { Models } from "../../main/models"

export interface IStatefulArtifact {
    artifactId: number;
    state: IState;
    properties: IArtifactProperties;
}


export class StatefullArtifact implements IStatefulArtifact {
    public artifactId: number;
    public state: IState;
    public properties: IArtifactProperties; 

    public static $inject = ["$http", "$q"];

    constructor(private $http: ng.IHttpService, private $q: ng.IQService, artifact: any) {

        this.artifactId = artifact.id;
        this.state = new ArtifactState();
        this.properties = new ArtifactProperties(this.$q, this.lockArtifact );

    }

    private lockArtifact() {
        var defer = this.$q.defer<Models.ILockResult>();

        // if (this.state.locked || state.lockedBy !== Enums.LockedByEnum.None) {
        //     defer.resolve(state.lock);
        // } else {
            const request: ng.IRequestConfig = {
                url: `/svc/shared/artifacts/lock`,
                method: "post",
                data: angular.toJson([this.artifactId])
            };

            this.$http(request).then(
                (result: ng.IHttpPromiseCallbackArg<Models.ILockResult[]>) => {
                    this.state.lock = result.data[0];
                    defer.resolve(state.lock);
                },
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
        }
        // return defer.promise;
    }

 
}